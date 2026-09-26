using JadeChem.CustomControls.ModelControls;
using JadeChem.Models;
using System.Reflection;
using TorchSharp;

internal static class ModelChecks
{
    public static void Run()
    {
        double[][] inputs = { new[] { -1.0 }, new[] { 1.0 } };
        double[] outputs = { 1, 5 }; // slope 2, intercept 3

        Check.Run("Fractional regularization settings survive UI changes", () =>
        {
            using var ridge = new RidgeRegressionModelControl();
            using var lasso = new LassoRegressionModelControl();
            using var elastic = new ElasticNetRegressionModelControl();
            foreach (var control in new UserControl[] { ridge, lasso, elastic })
                ((NumericUpDown)control.Controls.Find("lambdaNumericUpDown", true).Single()).Value = 0.125m;
            Check.Near(0.125, ridge.Hyperparameters["lambda"]);
            Check.Near(0.125, lasso.Hyperparameters["lambda"]);
            Check.Near(0.125, elastic.Hyperparameters["lambda"]);
        });
        Check.Run("Ridge shrinks weights while preserving the intercept", () =>
        {
            var model = new RidgeRegression(2);
            model.Learn(inputs, outputs);
            Check.Near(1, model.Coefficients[0]);
            Check.Near(3, model.Coefficients[1]);
        });
        Check.Run("Zero-penalty Ridge handles duplicate and constant features", () =>
        {
            double[][] collinear = { new[] { 0.0, 0.0, 1.0 }, new[] { 1.0, 1.0, 1.0 }, new[] { 2.0, 2.0, 1.0 } };
            var model = new RidgeRegression(0);
            model.Learn(collinear, new double[] { 3, 5, 7 });
            Check.Near(9, model.Transform(new[] { 3.0, 3.0, 1.0 }), 1e-8);
        });
        Check.Run("Lasso strength is independent of learning rate", () =>
        {
            foreach (double rate in new[] { 0.01, 0.05 })
            {
                var model = new LassoRegression(1, rate, 2000);
                model.Learn(inputs, outputs);
                Check.Near(1.5, model.Coefficients[0]);
                Check.Near(3, model.Coefficients[1]);
            }
        });
        Check.Run("Lasso produces exact zeros without shrinking a constant target", () =>
        {
            var model = new LassoRegression(10, 0.01, 2000);
            model.Learn(inputs, outputs);
            Check.Near(0, model.Coefficients[0], 0);
            Check.Near(3, model.Coefficients[1]);
        });
        Check.Run("Elastic Net matches its analytic solution and Ridge endpoint", () =>
        {
            var model = new ElasticNetRegression(2, 0.5, 0.01, 2000);
            model.Learn(inputs, outputs);
            Check.Near(1, model.Coefficients[0]);
            Check.Near(3, model.Coefficients[1]);
            var ridge = new RidgeRegression(2);
            ridge.Learn(inputs, outputs);
            var ridgeEndpoint = new ElasticNetRegression(2, 1, 0.01, 2000);
            ridgeEndpoint.Learn(inputs, outputs);
            Check.Near(ridge.Coefficients[0], ridgeEndpoint.Coefficients[0]);
        });
        Check.Run("Regression rejects invalid input and untrained predictions", () =>
        {
            Check.Throws<ArgumentOutOfRangeException>(() => new RidgeRegression(-1));
            Check.Throws<ArgumentOutOfRangeException>(() => new LassoRegression(1, double.NaN, 10));
            Check.Throws<ArgumentOutOfRangeException>(() => new ElasticNetRegression(1, 2, 0.01, 10));
            var model = new RidgeRegression(1);
            Check.Throws<InvalidOperationException>(() => model.Transform(new[] { 1.0 }));
            Check.Throws<ArgumentNullException>(() => model.Learn(inputs, null));
            Check.Throws<ArgumentException>(() => model.Learn(inputs, new[] { 1.0 }));
            Check.Throws<ArgumentException>(() => model.Learn(new[] { new[] { double.NaN } }, new[] { 1.0 }));
            model.Learn(inputs, outputs);
            Check.Throws<ArgumentException>(() => model.Transform(new[] { 1.0, 2.0 }));
        });
        Check.Run("Failed regression refits preserve the last successful model", () =>
        {
            var model = new LassoRegression(1, 0.01, 2000);
            model.Learn(inputs, outputs);
            double prediction = model.Transform(new[] { 1.0 });
            Check.Throws<InvalidOperationException>(() => model.Learn(
                new[] { new[] { double.MaxValue }, new[] { double.MaxValue } }, outputs));
            Check.Near(prediction, model.Transform(new[] { 1.0 }));
        });
        Check.Run("MLP validation reports mean loss without retaining tensors", () =>
        {
            using var control = new MLPModelControl(JadeChem.PredictionTaskForm.PredictionType.Regression,
                new[] { "x" }, new[] { new[] { 0.0 }, new[] { 0.0 } }, new[] { 1.0, 1.0 });
            var model = new MLP("test", 1, new[] { 2 }, new[] { "ReLU" }, 1);
            typeof(MLPModelControl).GetField("mlp", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(control, model);
            using (var scope = torch.NewDisposeScope())
            using (var noGrad = torch.no_grad())
                foreach (var parameter in model.parameters())
                    parameter.zero_();
            using var x = torch.tensor(new float[,] { { 0 }, { 0 } });
            using var y = torch.tensor(new float[,] { { 1 }, { 1 } });
            var validate = typeof(MLPModelControl).GetMethod("Validate", BindingFlags.Instance | BindingFlags.NonPublic)!;
            long before = torch.Tensor.TotalCount;
            for (int iteration = 0; iteration < 3; iteration++)
                Check.Near(1, (float)validate.Invoke(control, new object[] { x, y })!);
            Check.Equal(before, torch.Tensor.TotalCount);

            var optimizer = torch.optim.Adam(model.parameters(), 0.01);
            typeof(MLPModelControl).GetField("optimizer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(control, optimizer);
            var train = typeof(MLPModelControl).GetMethod("Train", BindingFlags.Instance | BindingFlags.NonPublic)!;
            train.Invoke(control, new object[] { x, y }); // initialize Adam state and gradients
            before = torch.Tensor.TotalCount;
            for (int iteration = 0; iteration < 3; iteration++)
                train.Invoke(control, new object[] { x, y });
            Check.Equal(before, torch.Tensor.TotalCount);
            Check.Equal(1m, ((NumericUpDown)control.Controls.Find("epochsNumericUpDown", true).Single()).Minimum);
            Check.Equal(1m, ((NumericUpDown)control.Controls.Find("saveIntervalNumericUpDown", true).Single()).Minimum);
        });
        Check.Run("MLP checkpoints roundtrip and failed writes preserve the previous file", () =>
        {
            string directory = Path.Combine(Path.GetTempPath(), "JadeChem-checkpoint-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var save = typeof(MLPModelControl).GetMethod("SaveCheckpoint", BindingFlags.Static | BindingFlags.NonPublic)!
                    .CreateDelegate<Func<MLP, string, string, bool>>();
                var load = typeof(MLPModelControl).GetMethod("LoadCheckpoint", BindingFlags.Static | BindingFlags.NonPublic)!
                    .CreateDelegate<Func<MLP, string, torch.ScalarType, DeviceType, MLP>>();
                using var source = new MLP("source", 1, new[] { 2 }, new[] { "ReLU" }, 1);
                using var current = new MLP("current", 1, new[] { 2 }, new[] { "ReLU" }, 1);
                FillModel(source, 1);
                FillModel(current, 0);
                string path = Path.Combine(directory, "network_1.ckpt");
                Check.Equal(true, save(source, directory, "network_1.ckpt"));
                byte[] previous = File.ReadAllBytes(path);
                using (var loaded = load(current, path, torch.ScalarType.Float32, DeviceType.CPU))
                    Check.Near(5, Predict(loaded));
                Check.Near(0, Predict(current));

                Check.Equal(false, save(null!, directory, "network_1.ckpt"));
                Check.Equal(true, previous.SequenceEqual(File.ReadAllBytes(path)));
                FillModel(source, 2);
                using (var locked = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    Check.Equal(false, save(source, directory, "network_1.ckpt"));
                Check.Equal(true, previous.SequenceEqual(File.ReadAllBytes(path)));
                Check.Equal(1, Directory.GetFiles(directory).Length);

                Check.Equal(true, save(source, directory, "network_1.ckpt"));
                using (var loaded = load(current, path, torch.ScalarType.Float32, DeviceType.CPU))
                    Check.Near(18, Predict(loaded));
                Check.Equal(1, Directory.GetFiles(directory).Length);

                byte[] truncated = File.ReadAllBytes(path);
                File.WriteAllBytes(path, truncated.Take(truncated.Length - 1).ToArray());
                Check.Throws<Exception>(() =>
                {
                    using var failed = load(current, path, torch.ScalarType.Float32, DeviceType.CPU);
                });
                Check.Near(0, Predict(current));
            }
            finally
            {
                foreach (string file in Directory.GetFiles(directory))
                    File.Delete(file);
                Directory.Delete(directory);
            }
        });
    }

    private static void FillModel(MLP model, float value)
    {
        using var scope = torch.NewDisposeScope();
        using var noGrad = torch.no_grad();
        foreach (var parameter in model.parameters())
            parameter.fill_(value);
    }

    private static double Predict(MLP model)
    {
        using var scope = torch.NewDisposeScope();
        using var noGrad = torch.no_grad();
        return model.forward(torch.tensor(new float[,] { { 1 } })).ToSingle();
    }
}
