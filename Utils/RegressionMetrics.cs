using Accord.Statistics;

namespace JadeChem.Utils
{
    public static class RegressionMetrics
    {
        #region Methods
        private static void ValidatePair(double[] array1, double[] array2)
        {
            if (array1 == null) throw new ArgumentNullException(nameof(array1));
            if (array2 == null) throw new ArgumentNullException(nameof(array2));
            if (array1.Length != array2.Length)
                throw new ArgumentException($"Arrays must have the same length ({array1.Length} vs {array2.Length}).");
            if (array1.Length == 0)
                throw new ArgumentException("Arrays must not be empty.");
        }

        public static double MeanAbsoluteError(double[] array1, double[] array2)
        {
            ValidatePair(array1, array2);

            double meanAbsoluteError = 0;

            for (int rowIndex = 0; rowIndex < array1.Length; rowIndex++)
                meanAbsoluteError += Math.Abs(array1[rowIndex] - array2[rowIndex]);

            meanAbsoluteError /= array1.Length;

            return meanAbsoluteError;
        }

        public static double MeanSquaredError(double[] array1, double[] array2)
        {
            ValidatePair(array1, array2);

            double meanSquaredError = 0;

            for (int rowIndex = 0; rowIndex < array1.Length; rowIndex++)
                meanSquaredError += Math.Pow(array1[rowIndex] - array2[rowIndex], 2);

            meanSquaredError /= array1.Length;

            return meanSquaredError;
        }

        public static double RootMeanSquaredError(double[] array1, double[] array2)
        {
            ValidatePair(array1, array2);

            double rootMeanSquaredError = 0;

            for (int rowIndex = 0; rowIndex < array1.Length; rowIndex++)
                rootMeanSquaredError += Math.Pow(array1[rowIndex] - array2[rowIndex], 2);

            rootMeanSquaredError = Math.Sqrt(rootMeanSquaredError / array1.Length);

            return rootMeanSquaredError;
        }

        public static double PearsonCorrelationCoefficient(double[] array1, double[] array2)
        {
            ValidatePair(array1, array2);

            double std1 = array1.StandardDeviation();
            double std2 = array2.StandardDeviation();

            // Pearson r is undefined when either input is constant.
            if (std1 == 0 || std2 == 0)
                return double.NaN;

            double covarience = array1.Covariance(array2);
            double r = covarience / (std1 * std2);

            return r;
        }

        public static double RSquared(double[] array1, double[] array2)
        {
            ValidatePair(array1, array2);

            double sumOfSquaresResidual = 0;
            double totalSumOfSquares = 0;

            double expectedMean = array2.Mean();

            for (int rowIndex = 0; rowIndex < array1.Length; rowIndex++)
            {
                sumOfSquaresResidual += Math.Pow(array2[rowIndex] - array1[rowIndex], 2);
                totalSumOfSquares += Math.Pow(array2[rowIndex] - expectedMean, 2);
            }

            // R² is undefined when the target is constant. If predictions also match it perfectly, treat it as 1.
            if (totalSumOfSquares == 0)
                return sumOfSquaresResidual == 0 ? 1.0 : double.NaN;

            double rSquared = 1 - (sumOfSquaresResidual / totalSumOfSquares);

            return rSquared;
        }
        #endregion

    }
}
