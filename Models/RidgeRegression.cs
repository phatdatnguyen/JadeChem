using MathNet.Numerics.LinearAlgebra;

namespace JadeChem.Models
{
    public class RidgeRegression
    {
        private readonly double lambda;
        private Vector<double>? coefficients;

        public double[] Coefficients => RegressionMath.RequireFitted(coefficients).ToArray();

        public RidgeRegression(double lambda)
        {
            RegressionMath.ValidatePenalty(lambda);
            this.lambda = lambda;
        }

        public void Learn(double[][]? inputColumns, double[]? outputColumn)
        {
            var (inputs, outputs) = RegressionMath.Prepare(inputColumns, outputColumn);
            if (lambda > 0)
            {
                // Solve augmented least squares directly, leaving the intercept unpenalized.
                // Avoid forming/inverting X'X, which squares its condition number.
                int featureCount = inputs.ColumnCount - 1;
                var penalty = Matrix<double>.Build.Dense(featureCount, inputs.ColumnCount);
                for (int column = 0; column < featureCount; column++)
                    penalty[column, column] = Math.Sqrt(lambda);
                inputs = inputs.Stack(penalty);
                outputs = Vector<double>.Build.DenseOfEnumerable(outputs.Concat(new double[featureCount]));
            }

            // PseudoInverse truncates zero singular values; SVD.Solve divides by them.
            var fitted = inputs.PseudoInverse() * outputs;
            if (fitted.Any(value => !double.IsFinite(value)))
                throw new InvalidOperationException("Training produced non-finite coefficients. Scale the input features.");
            coefficients = fitted;
        }

        public double[] Transform(double[][]? inputColumns) => RegressionMath.Transform(inputColumns, coefficients);

        public double Transform(double[]? inputRow) => RegressionMath.Transform(inputRow, coefficients);
    }
}
