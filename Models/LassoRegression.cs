using MathNet.Numerics.LinearAlgebra;

namespace JadeChem.Models
{
    public class LassoRegression
    {
        #region Fields
        private readonly double lambda;
        private readonly double learningRate;
        private readonly int maxIterations;
        private Vector<double> coefficients;
        #endregion

        #region Property
        public double[] Coefficients { get { return coefficients.ToArray(); } }
        #endregion

        #region Constructor
        public LassoRegression(double lambda, double learningRate, int maxIterations)
        {
            this.lambda = lambda;
            this.learningRate = learningRate;
            this.maxIterations = maxIterations;
        }
        #endregion

        #region Methods
        public void Learn(double[][]? inputColumns, double[]? outputColumn)
        {
            if (inputColumns == null)
                throw new ArgumentNullException(nameof(inputColumns));

            if (outputColumn == null)
                throw new ArgumentNullException(nameof(outputColumn));

            var inputMatrix = Matrix<double>.Build.DenseOfRowArrays(inputColumns);
            inputMatrix = inputMatrix.Append(Matrix<double>.Build.Dense(inputMatrix.RowCount, 1, 1)); // Add a column of 1s for intercept
            var outputVector = Vector<double>.Build.Dense(outputColumn);

            int numberOfColumns = inputMatrix.ColumnCount;
            int interceptIndex = numberOfColumns - 1; // intercept is the appended column

            // Initialize coefficients to zeros
            coefficients = Vector<double>.Build.Dense(numberOfColumns);

            // Proximal gradient method:
            //   1) gradient step on the smooth (squared-error) loss
            //   2) soft-thresholding (proximal operator of the L1 penalty), skipping the intercept
            // This produces exact zero coefficients (true sparsity) and avoids the
            // earlier bug where the L1 step was scaled by learningRate^2.
            var inputMatrixTranspose = inputMatrix.Transpose();
            double l1Threshold = lambda * learningRate;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                Vector<double> residuals = outputVector - inputMatrix * coefficients;
                Vector<double> gradient = -2 * inputMatrixTranspose * residuals;
                coefficients -= learningRate * gradient;

                for (int j = 0; j < interceptIndex; j++)
                {
                    double v = coefficients[j];
                    if (v > l1Threshold) coefficients[j] = v - l1Threshold;
                    else if (v < -l1Threshold) coefficients[j] = v + l1Threshold;
                    else coefficients[j] = 0;
                }
            }
        }

        public double[] Transform(double[][]? inputColumns)
        {
            if (inputColumns == null)
                throw new ArgumentNullException(nameof(inputColumns));

            double[] transformedData = new double[inputColumns.Length];

            for (int i = 0; i < inputColumns.Length; i++)
            {
                Vector<double> inputVector = Vector<double>.Build.DenseOfArray(inputColumns[i]);
                inputVector = Vector<double>.Build.DenseOfEnumerable(inputVector.Append(1)); // For intercept
                transformedData[i] = inputVector.DotProduct(coefficients);
            }

            return transformedData;
        }

        public double Transform(double[]? inputRow)
        {
            if (inputRow == null)
                throw new ArgumentNullException(nameof(inputRow));

            Vector<double> inputVector = Vector<double>.Build.DenseOfArray(inputRow);
            inputVector = Vector<double>.Build.DenseOfEnumerable(inputVector.Append(1)); // For intercept
            return inputVector.DotProduct(coefficients);
        }
        #endregion
    }
}
