using MathNet.Numerics.LinearAlgebra;

namespace JadeChem.Models
{
    public class ElasticNetRegression
    {
        private readonly double lambda;
        private readonly double alpha;
        private readonly double learningRate;
        private readonly int maxIterations;
        private Vector<double>? coefficients;

        public double[] Coefficients => RegressionMath.RequireFitted(coefficients).ToArray();

        public ElasticNetRegression(double lambda, double alpha, double learningRate, int maxIterations)
        {
            RegressionMath.ValidatePenalty(lambda);
            RegressionMath.ValidateIterations(learningRate, maxIterations);
            if (!double.IsFinite(alpha) || alpha < 0 || alpha > 1)
                throw new ArgumentOutOfRangeException(nameof(alpha));
            this.lambda = lambda;
            this.alpha = alpha;
            this.learningRate = learningRate;
            this.maxIterations = maxIterations;
        }

        public void Learn(double[][]? inputColumns, double[]? outputColumn)
        {
            // Preserve the application's convention: alpha = 0 is Lasso, alpha = 1 is Ridge.
            coefficients = RegressionMath.Fit(inputColumns, outputColumn, lambda * (1 - alpha),
                lambda * alpha, learningRate, maxIterations);
        }

        public double[] Transform(double[][]? inputColumns) => RegressionMath.Transform(inputColumns, coefficients);

        public double Transform(double[]? inputRow) => RegressionMath.Transform(inputRow, coefficients);
    }
}
