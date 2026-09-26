using MathNet.Numerics.LinearAlgebra;

namespace JadeChem.Models
{
    public class LassoRegression
    {
        private readonly double lambda;
        private readonly double learningRate;
        private readonly int maxIterations;
        private Vector<double>? coefficients;

        public double[] Coefficients => RegressionMath.RequireFitted(coefficients).ToArray();

        public LassoRegression(double lambda, double learningRate, int maxIterations)
        {
            RegressionMath.ValidatePenalty(lambda);
            RegressionMath.ValidateIterations(learningRate, maxIterations);
            this.lambda = lambda;
            this.learningRate = learningRate;
            this.maxIterations = maxIterations;
        }

        public void Learn(double[][]? inputColumns, double[]? outputColumn)
        {
            coefficients = RegressionMath.Fit(inputColumns, outputColumn, lambda, 0, learningRate, maxIterations);
        }

        public double[] Transform(double[][]? inputColumns) => RegressionMath.Transform(inputColumns, coefficients);

        public double Transform(double[]? inputRow) => RegressionMath.Transform(inputRow, coefficients);
    }
}
