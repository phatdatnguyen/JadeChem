using Accord.Math;
using JadeChem;
using JadeChem.Utils;
using System.Reflection;

internal static class LeakageChecks
{
    public static void Run()
    {
        Check.Run("Input and target scaling learn only from training rows", () =>
        {
            using var form = CreateForm();
            var settings = new Dictionary<string, (double, double)>
            {
                ["Standardization"] = (0, 1), ["Min-max scaling"] = (-1, 1)
            };
            Set(form, "inputScalersDictionary", new Dictionary<string, Dictionary<string, (double, double)>> { ["x"] = settings });
            Set(form, "outputScalersDictionary", new Dictionary<string, Dictionary<string, (double, double)>> { ["y"] = settings });
            var raw = Get<double[][]>(form, "unscaledInputColumns");
            var targets = Get<double[]>(form, "unscaledOutputColumnForRegression");
            var split = GetSplit(raw, 0);
            Fit(form, 0);
            var baseline = Get<double[][]>(form, "processedInputColumns");
            var baselineTargets = Get<double[]>(form, "processedOutputColumnForRegression");
            Check.Near(-1, split.TrainIndices.Min(i => baseline[i][0]));
            Check.Near(1, split.TrainIndices.Max(i => baseline[i][0]));
            Check.Near(-1, split.TrainIndices.Min(i => baselineTargets[i]));
            Check.Near(1, split.TrainIndices.Max(i => baselineTargets[i]));

            foreach (int index in split.TestIndices)
            {
                raw[index][0] += 10000;
                targets[index] -= 10000;
            }
            Fit(form, 0);
            var transformed = Get<double[][]>(form, "processedInputColumns");
            var transformedTargets = Get<double[]>(form, "processedOutputColumnForRegression");
            foreach (int index in split.TrainIndices)
            {
                Check.Near(baseline[index][0], transformed[index][0]);
                Check.Near(baselineTargets[index], transformedTargets[index]);
            }
            Check.Equal(true, transformed[split.TestIndices[0]][0] > 1);

            // Prediction and inverse output scaling must use the same fitted training statistics.
            var standard = Get<Dictionary<string, StandardScaler>>(form, "standardScalers");
            var minMax = Get<Dictionary<string, MinMaxScaler>>(form, "minMaxScalers");
            Check.Near(transformed[0][0], minMax["x"].Transform(standard["x"].Transform(raw[0][0])));
            Check.Near(targets[0], standard["y"].InverseTransform(minMax["y"].InverseTransform(transformedTargets[0])));

            Fit(form, 3);
            var secondSplit = GetSplit(raw, 3);
            transformed = Get<double[][]>(form, "processedInputColumns");
            Check.Near(-1, secondSplit.TrainIndices.Min(i => transformed[i][0]));
            Check.Near(1, secondSplit.TrainIndices.Max(i => transformed[i][0]));
        });

        Check.Run("Variance selection ignores features that vary only in held-out data", () =>
        {
            using var form = CreateForm();
            var raw = Get<double[][]>(form, "unscaledInputColumns");
            var split = GetSplit(raw, 0);
            foreach (int index in split.TrainIndices) raw[index][1] = 0;
            foreach (int index in split.TestIndices) raw[index][1] = 1000;
            Set(form, "dimensionalityReductionStepsDictionary", new Dictionary<string, Dictionary<string, double>>
            {
                ["Variance threshold"] = new() { ["threshold"] = 0.01 }
            });
            Fit(form, 0);
            Check.Equal("x", string.Join(",", Get<string[]>(form, "processedInputColumnNames")));
            Check.Equal(1, Get<double[][]>(form, "processedInputColumns")[0].Length);
        });

        Check.Run("PCA fit is unaffected by changes to held-out rows", () =>
        {
            using var form = CreateForm();
            var raw = Get<double[][]>(form, "unscaledInputColumns");
            var split = GetSplit(raw, 0);
            Set(form, "dimensionalityReductionStepsDictionary", new Dictionary<string, Dictionary<string, double>>
            {
                ["Principle component analysis"] = new() { ["nComponents"] = 1 }
            });
            Fit(form, 0);
            var baseline = Get<double[][]>(form, "processedInputColumns");
            foreach (int index in split.TestIndices) raw[index][1] += 100000;
            Fit(form, 0);
            var transformed = Get<double[][]>(form, "processedInputColumns");
            foreach (int index in split.TrainIndices) Check.Near(baseline[index][0], transformed[index][0]);
            Check.Equal("PC1", Get<string[]>(form, "processedInputColumnNames")[0]);
        });
        Check.Run("A split missing a training class preserves existing preprocessing", () =>
        {
            using var form = CreateForm();
            Fit(form, 0);
            var previous = Get<double[][]>(form, "processedInputColumns");
            var raw = Get<double[][]>(form, "unscaledInputColumns");
            var split = GetSplit(raw, 0);
            var labels = Enumerable.Repeat("a", raw.Length).ToArray();
            foreach (int index in split.TestIndices) labels[index] = "b";
            Set(form, "predictionType", PredictionTaskForm.PredictionType.BinaryClassification);
            Set(form, "processedOutputColumnForClassification", labels);
            Set(form, "classLabels", new[] { "a", "b" });
            Check.Throws<InvalidOperationException>(() => Fit(form, 0));
            Check.Equal(true, ReferenceEquals(previous, Get<double[][]>(form, "processedInputColumns")));
        });
    }

    private static PredictionTaskForm CreateForm()
    {
        var form = new PredictionTaskForm();
        Set(form, "unscaledInputColumns", Enumerable.Range(0, 12).Select(i => new[] { (double)i, (double)(i * i % 17) }).ToArray());
        Set(form, "unscaledOutputColumnForRegression", Enumerable.Range(0, 12).Select(i => 3.0 * i + 5).ToArray());
        Set(form, "preprocessedInputColumnNames", new[] { "x", "z" });
        Set(form, "outputColumnName", "y");
        return form;
    }

    private static TrainTestSpliter GetSplit(double[][] raw, int seed)
    {
        var splitter = new TrainTestSpliter();
        splitter.Split(raw, raw, 0.25, seed);
        return splitter;
    }

    private static void Fit(PredictionTaskForm form, int seed)
    {
        try
        {
            typeof(PredictionTaskForm).GetMethod("RefitPreprocessingForSplit", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(form, new object[] { 0.25, seed });
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static void Set(PredictionTaskForm form, string name, object value) => typeof(PredictionTaskForm)
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, value);

    private static T Get<T>(PredictionTaskForm form, string name) => (T)typeof(PredictionTaskForm)
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
}
