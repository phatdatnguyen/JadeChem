using Accord.Math;

namespace JadeChem.Utils
{
    public class TrainTestSpliter
    {
        #region Fields
        private int[] testIndices = Array.Empty<int>();
        private int[] trainIndices = Array.Empty<int>();
        #endregion

        #region Properties
        public int[] TrainIndices
        {
            get { return trainIndices; }
        }

        public int[] TestIndices
        {
            get { return testIndices; }
        }
        #endregion

        #region Constructor
        public TrainTestSpliter() { }
        #endregion

        #region Methods
        public (double[][], double[][], double[][], double[][]) Split(double[][] inputColumns, double[][] outputColumns, double testSize = 0.3, int randomSeed = 0)
        {
            ArgumentNullException.ThrowIfNull(inputColumns);
            ArgumentNullException.ThrowIfNull(outputColumns);
            SplitIndices(inputColumns.Length, outputColumns.Length, testSize, randomSeed);

            // Get the train and test matrix
            double[][] trainInputColumns = inputColumns.GetRows(trainIndices);
            double[][] trainOutputColumns = outputColumns.GetRows(trainIndices);
            double[][] testInputColumns = inputColumns.GetRows(testIndices);
            double[][] testOutputColumns = outputColumns.GetRows(testIndices);

            return (trainInputColumns, trainOutputColumns, testInputColumns, testOutputColumns);
        }

        public (double[][], string[][], double[][], string[][]) Split(double[][] inputColumns, string[][] outputColumns, double testSize = 0.3, int randomSeed = 0)
        {
            ArgumentNullException.ThrowIfNull(inputColumns);
            ArgumentNullException.ThrowIfNull(outputColumns);
            SplitIndices(inputColumns.Length, outputColumns.Length, testSize, randomSeed);

            // Get the train and test matrix
            double[][] trainInputColumns = inputColumns.GetRows(trainIndices);
            string[][] trainOutputColumns = outputColumns.GetRows(trainIndices);
            double[][] testInputColumns = inputColumns.GetRows(testIndices);
            string[][] testOutputColumns = outputColumns.GetRows(testIndices);

            return (trainInputColumns, trainOutputColumns, testInputColumns, testOutputColumns);
        }

        private void SplitIndices(int inputRowCount, int totalRowCount, double testSize, int randomSeed)
        {
            if (inputRowCount != totalRowCount)
                throw new ArgumentException("Input and output data must have the same number of rows.");
            if (!double.IsFinite(testSize) || testSize <= 0 || testSize >= 1)
                throw new ArgumentOutOfRangeException(nameof(testSize), "Test size must be between 0 and 1.");

            int testRowCount = (int)Math.Round(testSize * totalRowCount);
            if (testRowCount == 0 || totalRowCount - testRowCount == 0)
                throw new ArgumentException("The split must contain at least one training row and one test row.", nameof(testSize));

            // Sample without replacement. A partially filled, zero-initialized array
            // cannot track selected rows because it incorrectly marks row 0 as selected.
            int[] shuffledIndices = Enumerable.Range(0, totalRowCount).ToArray();
            Random random = new(randomSeed);
            for (int index = 0; index < testRowCount; index++)
            {
                int selectedIndex = random.Next(index, totalRowCount);
                (shuffledIndices[index], shuffledIndices[selectedIndex]) = (shuffledIndices[selectedIndex], shuffledIndices[index]);
            }

            testIndices = shuffledIndices.Take(testRowCount).OrderBy(index => index).ToArray();
            trainIndices = shuffledIndices.Skip(testRowCount).OrderBy(index => index).ToArray();
        }
        #endregion
    }
}
