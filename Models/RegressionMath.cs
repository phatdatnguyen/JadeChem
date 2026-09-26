using MathNet.Numerics.LinearAlgebra;

namespace JadeChem.Models
{
    internal static class RegressionMath
    {
        internal static void ValidatePenalty(double lambda)
        {
            if (!double.IsFinite(lambda) || lambda < 0)
                throw new ArgumentOutOfRangeException(nameof(lambda), "Regularization must be finite and nonnegative.");
        }

        internal static void ValidateIterations(double learningRate, int maxIterations)
        {
            if (!double.IsFinite(learningRate) || learningRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(learningRate));
            if (maxIterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxIterations));
        }

        internal static (Matrix<double> Inputs, Vector<double> Outputs) Prepare(double[][]? inputColumns, double[]? outputColumn)
        {
            ArgumentNullException.ThrowIfNull(inputColumns);
            ArgumentNullException.ThrowIfNull(outputColumn);
            if (inputColumns.Length == 0 || inputColumns[0] == null || inputColumns[0].Length == 0)
                throw new ArgumentException("Training data must contain rows and features.", nameof(inputColumns));
            if (inputColumns.Length != outputColumn.Length)
                throw new ArgumentException("Input and output row counts must match.", nameof(outputColumn));

            int featureCount = inputColumns[0].Length;
            foreach (double[] row in inputColumns)
                ValidateRow(row, featureCount, nameof(inputColumns));
            if (outputColumn.Any(value => !double.IsFinite(value)))
                throw new ArgumentException("Output values must be finite.", nameof(outputColumn));

            var inputs = Matrix<double>.Build.DenseOfRowArrays(inputColumns);
            return (inputs.Append(Matrix<double>.Build.Dense(inputs.RowCount, 1, 1)),
                Vector<double>.Build.DenseOfArray(outputColumn));
        }

        // Minimize RSS + 2 * lassoPenalty * |weights| + ridgePenalty * weights^2.
        // Soft thresholding handles the L1 corner at zero without penalizing the intercept.
        internal static Vector<double> Fit(double[][]? inputColumns, double[]? outputColumn,
            double lassoPenalty, double ridgePenalty, double learningRate, int maxIterations)
        {
            var (inputs, outputs) = Prepare(inputColumns, outputColumn);
            var transpose = inputs.Transpose();
            var fitted = Vector<double>.Build.Dense(inputs.ColumnCount);
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                var gradients = 2 * transpose * (inputs * fitted - outputs);
                for (int column = 0; column < fitted.Count; column++)
                {
                    bool isIntercept = column == fitted.Count - 1;
                    double gradient = gradients[column] + (isIntercept ? 0 : 2 * ridgePenalty * fitted[column]);
                    double next = fitted[column] - learningRate * gradient;
                    if (!double.IsFinite(next))
                        throw new InvalidOperationException("Training diverged. Reduce the learning rate or scale the input features.");
                    fitted[column] = isIntercept ? next :
                        Math.Sign(next) * Math.Max(0, Math.Abs(next) - 2 * learningRate * lassoPenalty);
                }
            }
            return fitted;
        }

        internal static Vector<double> RequireFitted(Vector<double>? coefficients) =>
            coefficients ?? throw new InvalidOperationException("Train the model before using its coefficients or making predictions.");

        internal static double Transform(double[]? inputRow, Vector<double>? coefficients)
        {
            ArgumentNullException.ThrowIfNull(inputRow);
            var fitted = RequireFitted(coefficients);
            ValidateRow(inputRow, fitted.Count - 1, nameof(inputRow));
            double prediction = fitted[fitted.Count - 1];
            for (int column = 0; column < inputRow.Length; column++)
                prediction += inputRow[column] * fitted[column];
            return prediction;
        }

        internal static double[] Transform(double[][]? inputColumns, Vector<double>? coefficients)
        {
            ArgumentNullException.ThrowIfNull(inputColumns);
            RequireFitted(coefficients);
            return inputColumns.Select(row => Transform(row, coefficients)).ToArray();
        }

        private static void ValidateRow(double[]? row, int featureCount, string parameterName)
        {
            if (row == null || row.Length != featureCount)
                throw new ArgumentException("All input rows must have the expected number of features.", parameterName);
            if (row.Any(value => !double.IsFinite(value)))
                throw new ArgumentException("Input values must be finite.", parameterName);
        }
    }
}
