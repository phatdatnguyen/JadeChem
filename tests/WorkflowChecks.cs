using Accord.Statistics.Models.Regression.Linear;
using JadeChem;
using JadeChem.CustomControls.EvaluationControls;
using JadeChem.CustomEventArgs;
using System.Data;
using System.Reflection;
using System.Runtime.ExceptionServices;

internal static class WorkflowChecks
{
    public static void Run()
    {
        Check.Run("Numeric-only processing keeps every row and column", () =>
        {
            using var form = new PredictionTaskForm();
            LoadAndProcessNumeric(form);
            var processed = Get<DataTable>(form, "processedDataset");
            Check.Equal(12, processed.Rows.Count);
            Check.Equal(3, processed.Columns.Count);
            Check.Near(11, Convert.ToDouble(processed.Rows[11]["x"]));
            Check.Near(22, Convert.ToDouble(processed.Rows[11]["z"]));
            Check.Near(40, Convert.ToDouble(processed.Rows[11]["y"]));
        });

        Check.Run("RDKit feature extraction keeps headers after an invalid first molecule", () =>
        {
            using var form = new WarningCapturingPredictionTaskForm();
            var data = new DataTable();
            data.Columns.Add("smiles", typeof(string));
            data.Columns.Add("class", typeof(string));
            data.Rows.Add("not-a-smiles", "a");
            data.Rows.Add("C", "a");
            data.Rows.Add("CC", "b");
            data.Rows.Add("CCC", "a");
            Invoke(form, "OnInputDataLoaded", new DataTableEventArgs { Dataset = data });
            Set(form, "featuresDictionary", new Dictionary<string, Dictionary<string, Dictionary<string, double>>>
            {
                ["smiles"] = new() { ["AMW"] = new() }
            });
            Set(form, "isFeatureExtractionDialogResetNeeded", false);
            Invoke(form, "ProcessButton_Click", form, EventArgs.Empty);
            var processed = Get<DataTable>(form, "processedDataset");
            Check.Equal(3, processed.Rows.Count);
            Check.Equal("smiles_AMW", processed.Columns[0].ColumnName);
            Check.Near(16.043, Convert.ToDouble(processed.Rows[0][0]), 0.001);
            Check.Equal("a", Convert.ToString(processed.Rows[0][1]));
            Check.Equal("b", Convert.ToString(processed.Rows[1][1]));
            Check.Equal(true, form.Warning?.Contains("1 of 4 row(s)") == true);
            Check.Equal(true, form.Warning?.Contains("not-a-smiles") == true);
        });

        Check.Run("Reprocessing clears old trained model and evaluation data", () =>
        {
            using var form = new PredictionTaskForm();
            LoadAndProcessNumeric(form);
            Set(form, "model", new MultipleLinearRegression { Weights = new[] { 1.0, 2.0 } });
            Set(form, "testInputColumns", new[] { new[] { 1.0, 2.0 } });
            Set(form, "predictionOutputColumnForRegression", new[] { 42.0 });
            Get<TableLayoutPanel>(form, "predictionTableLayoutPanel").Enabled = true;
            Invoke(form, "ProcessButton_Click", form, EventArgs.Empty);
            Check.Equal<object?>(null, Get<object?>(form, "model"));
            Check.Equal<double[][]?>(null, Get<double[][]?>(form, "testInputColumns"));
            Check.Equal<double[]?>(null, Get<double[]?>(form, "predictionOutputColumnForRegression"));
            Check.Equal(false, Get<TableLayoutPanel>(form, "predictionTableLayoutPanel").Enabled);
            Check.Equal(12, Get<DataTable>(form, "processedDataset").Rows.Count);
        });

        Check.Run("Changing column roles invalidates processed state", () =>
        {
            using var form = new PredictionTaskForm();
            LoadAndProcessNumeric(form);
            Get<DataGridView>(form, "columnsDataGridView").Rows[0].Cells[2].Value = false;
            Check.Equal<DataTable?>(null, Get<DataTable?>(form, "processedDataset"));
            Check.Equal<double[][]?>(null, Get<double[][]?>(form, "unscaledInputColumns"));
            Check.Equal(false, Get<TableLayoutPanel>(form, "modelTableLayoutPanel").Enabled);
            Check.Equal(true, Get<RadioButton>(form, "regressionRadioButton").Enabled);
        });

        Check.Run("Prediction maps reordered CSV inputs to training feature order", () =>
        {
            using var form = CreatePredictionForm();
            var normal = ((double[], string))Invoke(form, "PredictRow", new[] { "x", "z" }, new[] { "3", "11" })!;
            var reordered = ((double[], string))Invoke(form, "PredictRow", new[] { "z", "x" }, new[] { "11", "3" })!;
            Check.Near(68, double.Parse(normal.Item2));
            Check.Equal(normal.Item2, reordered.Item2);
            Check.Near(3, reordered.Item1[0]);
            Check.Near(11, reordered.Item1[1]);
        });

        Check.Run("Prediction rejects missing, duplicate, and nonfinite inputs", () =>
        {
            using var form = CreatePredictionForm();
            Check.Throws<ArgumentException>(() => Invoke(form, "PredictRow", new[] { "x", "other" }, new[] { "3", "11" }));
            Check.Throws<ArgumentException>(() => Invoke(form, "PredictRow", new[] { "x", "x" }, new[] { "3", "11" }));
            Check.Throws<ArgumentException>(() => Invoke(form, "PredictRow", new[] { "x", "z" }, new[] { "NaN", "11" }));
        });

        Check.Run("Prediction parses decimal-point inputs independently of Windows culture", () =>
        {
            var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("de-DE");
                using var form = CreatePredictionForm();
                var prediction = ((double[], string))Invoke(form, "PredictRow", new[] { "z", "x" }, new[] { "2.5", "1.25" })!;
                Check.Near(22, double.Parse(prediction.Item2));
                Check.Near(1.25, prediction.Item1[0]);
                Check.Near(2.5, prediction.Item1[1]);
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = previousCulture; }
        });

        Check.Run("Loading prediction data clears previous visualization arrays", () =>
        {
            using var form = CreatePredictionForm();
            Set(form, "predictionInputColumns", new[] { new[] { 99.0, 88.0 } });
            Get<Button>(form, "visualizePredictionButton").Enabled = true;
            var data = new DataTable();
            data.Columns.Add("x", typeof(double));
            data.Columns.Add("z", typeof(double));
            data.Rows.Add(1, 2);
            Invoke(form, "OnPredictionDataLoaded", new DataTableEventArgs { Dataset = data });
            Check.Equal<double[][]?>(null, Get<double[][]?>(form, "predictionInputColumns"));
            Check.Equal(false, Get<Button>(form, "visualizePredictionButton").Enabled);
        });

        Check.Run("Binary metrics initially use the selected positive class", () =>
        {
            using var control = new BinaryClassificationEvaluationControl(new[] { "0", "1" }, new[] { 0, 1, 1, 1 }, new[] { 0, 0, 1, 1 });
            Check.Equal(1, Get<ComboBox>(control, "positiveClassComboBox").SelectedIndex);
            var table = (DataTable)Get<DataGridView>(control, "confusionMatrixDataGridView").DataSource;
            Check.Equal("1 (Expected)", (string)table.Rows[0][1]);
            Check.Equal("2", (string)table.Rows[1][1]);
            Check.Equal("1", (string)table.Rows[1][2]);
        });

        Check.Run("Multiclass metrics include classes absent from the test split", () =>
        {
            using var control = new MulticlassClassificationEvaluationControl(new[] { "a", "b", "c" }, new[] { 0, 0, 1 }, new[] { 0, 1, 1 });
            var table = (DataTable)Get<DataGridView>(control, "confusionMatrixDataGridView").DataSource;
            Check.Equal(4, table.Rows.Count);
            Check.Equal("0", (string)table.Rows[3][3]);
            Check.Equal("1", (string)table.Rows[1][2]);
            Check.Equal("0", (string)table.Rows[2][1]);
        });
    }

    private static void LoadAndProcessNumeric(PredictionTaskForm form)
    {
        var data = new DataTable();
        data.Columns.Add("x", typeof(double));
        data.Columns.Add("z", typeof(double));
        data.Columns.Add("y", typeof(double));
        for (int i = 0; i < 12; i++)
            data.Rows.Add(i, i * 2, i * 3 + 7);
        Invoke(form, "OnInputDataLoaded", new DataTableEventArgs { Dataset = data });
        Invoke(form, "ProcessButton_Click", form, EventArgs.Empty);
    }

    private static PredictionTaskForm CreatePredictionForm()
    {
        var form = new PredictionTaskForm();
        Set(form, "predictionType", PredictionTaskForm.PredictionType.Regression);
        Set(form, "inputColumnNamesForModel", new[] { "x", "z" });
        Set(form, "inputColumnNamesForFeatureExtraction", Array.Empty<string>());
        Set(form, "preprocessedInputColumnNames", new[] { "x", "z" });
        Set(form, "outputColumnName", "y");
        Set(form, "model", new MultipleLinearRegression { Weights = new[] { 2.0, 5.0 }, Intercept = 7 });
        return form;
    }

    private static Type ReflectedType(object instance) => instance is PredictionTaskForm ? typeof(PredictionTaskForm) : instance.GetType();

    private static T Get<T>(object instance, string field) =>
        (T)ReflectedType(instance).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(instance)!;

    private static void Set(object instance, string field, object value) =>
        ReflectedType(instance).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(instance, value);

    private static object? Invoke(object instance, string method, params object[] args)
    {
        try
        {
            return ReflectedType(instance).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(instance, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private sealed class WarningCapturingPredictionTaskForm : PredictionTaskForm
    {
        public string? Warning { get; private set; }
        protected override void ShowFeatureExtractionWarnings(string message) => Warning = message;
    }
}
