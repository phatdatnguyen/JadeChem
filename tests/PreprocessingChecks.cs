using System.Reflection;
using System.Windows.Forms;
using JadeChem.Dialogs;
using JadeChem.Utils;

internal static class PreprocessingChecks
{
    public static void Run()
    {
        Check.Run("Train/test split can select every row, including the first", () =>
        {
            double[][] rows = Enumerable.Range(0, 4).Select(index => new[] { (double)index }).ToArray();
            HashSet<int> selected = new();
            for (int seed = 0; seed < 50; seed++)
            {
                TrainTestSpliter splitter = new();
                splitter.Split(rows, rows, 0.5, seed);
                selected.UnionWith(splitter.TestIndices);
            }
            Check.Equal(true, selected.SetEquals(Enumerable.Range(0, rows.Length)));
        });

        Check.Run("Train/test splits are reproducible, disjoint, exhaustive, and aligned", () =>
        {
            double[][] inputs = Enumerable.Range(0, 20).Select(index => new[] { (double)index }).ToArray();
            double[][] outputs = inputs.Select(row => new[] { row[0] + 100 }).ToArray();
            string[][] labels = inputs.Select(row => new[] { "label " + row[0] }).ToArray();
            TrainTestSpliter first = new();
            TrainTestSpliter second = new();
            var numeric = first.Split(inputs, outputs, 0.3, 42);
            var classification = second.Split(inputs, labels, 0.3, 42);

            Check.Equal(true, first.TestIndices.SequenceEqual(second.TestIndices));
            Check.Equal(true, first.TrainIndices.SequenceEqual(second.TrainIndices));
            Check.Equal(6, first.TestIndices.Length);
            Check.Equal(14, first.TrainIndices.Length);
            Check.Equal(0, first.TestIndices.Intersect(first.TrainIndices).Count());
            Check.Equal(true, first.TestIndices.Concat(first.TrainIndices).Order().SequenceEqual(Enumerable.Range(0, 20)));
            for (int index = 0; index < numeric.Item1.Length; index++)
                Check.Near(numeric.Item1[index][0] + 100, numeric.Item2[index][0]);
            for (int index = 0; index < numeric.Item3.Length; index++)
            {
                Check.Near(numeric.Item3[index][0] + 100, numeric.Item4[index][0]);
                Check.Equal("label " + classification.Item3[index][0], classification.Item4[index][0]);
            }
        });

        Check.Run("Invalid split ratios and mismatched rows are rejected", () =>
        {
            double[][] rows = new[] { new[] { 1d }, new[] { 2d } };
            foreach (double ratio in new[] { -1d, 0, 1, 2, double.NaN, double.PositiveInfinity })
                Check.Throws<ArgumentOutOfRangeException>(() => new TrainTestSpliter().Split(rows, rows, ratio));
            Check.Throws<ArgumentException>(() => new TrainTestSpliter().Split(rows, new[] { rows[0] }));
            Check.Throws<ArgumentException>(() => new TrainTestSpliter().Split(rows, new[] { new[] { "a" } }));
            Check.Throws<ArgumentException>(() => new TrainTestSpliter().Split(rows, rows, 0.01));
        });

        Check.Run("Standard scaling handles constant and single-row training columns", () =>
        {
            foreach (double[] values in new[] { new[] { 7d }, new[] { 7d, 7d, 7d } })
            {
                StandardScaler scaler = new();
                double[] scaled = scaler.FitTransform(values);
                Check.Equal(true, scaled.All(value => value == 0));
                Check.Equal(true, scaler.InverseTransform(scaled).SequenceEqual(values));
                Check.Near(8, scaler.InverseTransform(scaler.Transform(8)));
            }
            StandardScaler regular = new();
            double[] normalized = regular.FitTransform(new[] { 1d, 2d, 3d });
            Check.Near(-1, normalized[0]);
            Check.Near(0, normalized[1]);
            Check.Near(1, normalized[2]);
        });

        Check.Run("Min-max scaling handles constant columns and retains inverse transforms", () =>
        {
            MinMaxScaler scaler = new(-1, 1);
            double[] values = new[] { 7d, 7d, 7d };
            double[] scaled = scaler.FitTransform(values);
            Check.Equal(true, scaled.All(value => value == -1));
            Check.Equal(true, scaler.InverseTransform(scaled).SequenceEqual(values));
            Check.Near(8, scaler.InverseTransform(scaler.Transform(8)));
            scaled = scaler.FitTransform(new[] { 1d, 2d, 3d });
            Check.Near(-1, scaled[0]);
            Check.Near(0, scaled[1]);
            Check.Near(1, scaled[2]);
        });

        Check.Run("Scaling rejects empty, non-finite inputs and invalid output ranges", () =>
        {
            foreach (double[] values in new[] { Array.Empty<double>(), new[] { double.NaN }, new[] { double.PositiveInfinity } })
            {
                Check.Throws<ArgumentException>(() => new StandardScaler().Fit(values));
                Check.Throws<ArgumentException>(() => new MinMaxScaler().Fit(values));
            }
            Check.Throws<ArgumentException>(() => new MinMaxScaler(1, 1));
            Check.Throws<ArgumentException>(() => new MinMaxScaler(1, -1));
            Check.Throws<ArgumentException>(() => new MinMaxScaler(double.NaN, 1));
        });

        Check.Run("PCA with fewer training rows than features produces only finite components", () =>
        {
            double[][] training = new[] { new[] { 1d, 2d, 3d }, new[] { 2d, 3d, 4d } };
            PCAFilter filter = new();
            var result = filter.FitTransform(training, 32);
            Check.Equal(1, result.Item1.Length);
            Check.Equal(true, result.Item2.SelectMany(row => row).All(double.IsFinite));
            double[] heldOut = new[] { 3d, 5d, 8d };
            double[] single = filter.Transform(heldOut);
            double[] batch = filter.Transform(new[] { heldOut }).Item2[0];
            Check.Equal(1, single.Length);
            Check.Near(batch[0], single[0]);
        });

        Check.Run("PCA drops numerically singular directions before whitening held-out data", () =>
        {
            PCAFilter filter = new();
            var result = filter.FitTransform(new[] { new[] { 1d, 2d }, new[] { 2d, 4d }, new[] { 3d, 6d } }, 2);
            Check.Equal(1, result.Item1.Length);
            double[] predicted = filter.Transform(new[] { 4d, 7d });
            Check.Equal(1, predicted.Length);
            Check.Equal(true, predicted.All(value => double.IsFinite(value) && Math.Abs(value) < 10));
        });

        Check.Run("PCA rejects insufficient variation without replacing a previously fitted filter", () =>
        {
            PCAFilter filter = new();
            filter.Fit(new[] { new[] { 1d, 2d }, new[] { 2d, 4d }, new[] { 3d, 5d }, new[] { 4d, 3d } }, 2);
            double[] expected = filter.Transform(new[] { 2d, 3d });
            Check.Equal(2, expected.Length);
            Check.Throws<ArgumentException>(() => filter.Fit(new[] { new[] { 1d, 2d } }, 2));
            Check.Throws<ArgumentException>(() => filter.Fit(new[] { new[] { 1d, 2d }, new[] { 1d, 2d } }, 2));
            double[] actual = filter.Transform(new[] { 2d, 3d });
            Check.Near(expected[0], actual[0]);
            Check.Near(expected[1], actual[1]);
        });

        Check.Run("Reopening processing settings restores dimensionality reduction selections", () =>
        {
            var reduction = new Dictionary<string, Dictionary<string, double>>
            {
                ["Variance threshold"] = new() { ["threshold"] = 0.25 },
                ["Principle component analysis"] = new() { ["nComponents"] = 3 }
            };
            using DataProcessingDialog dialog = new(new(), new(), new(), reduction);
            Invoke(dialog, "DataProcessingDialog_Load", dialog, EventArgs.Empty);
            ListView steps = Field<ListView>(dialog, "processingStepsListView");
            Check.Equal(true, steps.Items[2].Checked);
            Check.Equal(true, steps.Items[3].Checked);
            Check.Equal("{ nComponents=3 }", steps.Items[3].SubItems[1].Text);
            Check.Near(0.25, reduction["Variance threshold"]["threshold"]);
        });

        Check.Run("Unchecking one scaler preserves the other scaler and its settings", () =>
        {
            var scalers = new Dictionary<string, Dictionary<string, (double, double)>>
            {
                ["feature"] = new() { ["Min-max scaling"] = (-1, 1), ["Standardization"] = (0, 1) }
            };
            using DataProcessingDialog dialog = new(scalers, new(), new(), new());
            Invoke(dialog, "DataProcessingDialog_Load", dialog, EventArgs.Empty);
            ListView steps = Field<ListView>(dialog, "processingStepsListView");
            steps.Items[1].Checked = false;
            Invoke(dialog, "ProcessingStepsView_ItemChecked", steps, new ItemCheckedEventArgs(steps.Items[1]));
            Check.Equal(false, scalers["feature"].ContainsKey("Standardization"));
            Check.Equal((-1d, 1d), scalers["feature"]["Min-max scaling"]);
        });

        Check.Run("Feature extraction handles no selected molecule columns", () =>
        {
            using FeatureExtractionDialog dialog = new(new());
            Invoke(dialog, "FeatureExtractionDialog_Load", dialog, EventArgs.Empty);
            Check.Equal(-1, Field<ListBox>(dialog, "columnListBox").SelectedIndex);
            Check.Equal(false, Field<ListView>(dialog, "featuresListView").Enabled);
        });

        Check.Run("Existing Morgan fingerprint parameters are preserved when selected", () =>
        {
            var features = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>
            {
                ["SMILES"] = new() { ["Morgan_FP"] = new() { ["radius"] = 4, ["nBits"] = 1024 } }
            };
            using FeatureExtractionDialog dialog = new(features);
            Invoke(dialog, "FeatureExtractionDialog_Load", dialog, EventArgs.Empty);
            ListView list = Field<ListView>(dialog, "featuresListView");
            ListViewItem morgan = list.Items.Cast<ListViewItem>().Single(item => item.Text == "Morgan_FP");
            Invoke(dialog, "FeaturesListView_ItemChecked", list, new ItemCheckedEventArgs(morgan));
            Check.Near(4, features["SMILES"]["Morgan_FP"]["radius"]);
            Check.Near(1024, features["SMILES"]["Morgan_FP"]["nBits"]);
        });
    }

    private static T Field<T>(object target, string name) =>
        (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;

    private static void Invoke(object target, string name, params object[] arguments) =>
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(target, arguments);
}
