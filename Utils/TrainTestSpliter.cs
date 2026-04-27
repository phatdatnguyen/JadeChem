using Accord.Math;
using System.Data;

namespace JadeChem.Utils
{
    public class TrainTestSpliter
    {
        #region Fields
        private int[] testIndices;
        private int[] trainIndices;
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
        // Pick `testRowCount` distinct random indices in [0, totalRowCount) and return (train, test).
        // Uses a HashSet for O(1) membership checks (the previous int[].Contains() was O(n²))
        // and avoids the all-zeros pre-allocation that caused index 0 to be biased out of the test set.
        private static (int[] train, int[] test) SelectIndices(int totalRowCount, int testRowCount, int randomSeed)
        {
            Random random = new(randomSeed);
            HashSet<int> testSet = new(testRowCount);
            while (testSet.Count < testRowCount)
                testSet.Add(random.Next(totalRowCount));

            int[] testIndices = testSet.OrderBy(x => x).ToArray();
            int[] trainIndices = new int[totalRowCount - testRowCount];
            int trainPos = 0;
            for (int rowIndex = 0; rowIndex < totalRowCount; rowIndex++)
                if (!testSet.Contains(rowIndex))
                    trainIndices[trainPos++] = rowIndex;

            return (trainIndices, testIndices);
        }

        public (double[][], double[][], double[][], double[][]) Split(double[][] inputColumns, double[][] outputColumns, double testSize = 0.3, int randomSeed = 0)
        {
            // Get the number of rows in the processedDataset
            int totalRowCount = outputColumns.Rows();

            // Calculate the number of rows for the test set
            int testRowCount = (int)Math.Round(testSize * totalRowCount);

            // Throw new exception if train split or test split have 0 row
            if (testRowCount == 0 || totalRowCount - testRowCount == 0)
            {
                throw new Exception("Invalid split");
            }

            (trainIndices, testIndices) = SelectIndices(totalRowCount, testRowCount, randomSeed);

            // Get the train and test matrix
            double[][] trainInputColumns = inputColumns.GetRows(trainIndices);
            double[][] trainOutputColumns = outputColumns.GetRows(trainIndices);
            double[][] testInputColumns = inputColumns.GetRows(testIndices);
            double[][] testOutputColumns = outputColumns.GetRows(testIndices);

            return (trainInputColumns, trainOutputColumns, testInputColumns, testOutputColumns);
        }

        public (double[][], string[][], double[][], string[][]) Split(double[][] inputColumns, string[][] outputColumns, double testSize = 0.3, int randomSeed = 0)
        {
            // Get the number of rows in the processedDataset
            int totalRowCount = outputColumns.Rows();

            // Calculate the number of rows for the test set
            int testRowCount = (int)Math.Round(testSize * totalRowCount);

            // Throw new exception if train split or test split have 0 row
            if (testRowCount == 0 || totalRowCount - testRowCount == 0)
            {
                throw new Exception("Invalid split");
            }

            (trainIndices, testIndices) = SelectIndices(totalRowCount, testRowCount, randomSeed);

            // Get the train and test matrix
            double[][] trainInputColumns = inputColumns.GetRows(trainIndices);
            string[][] trainOutputColumns = outputColumns.GetRows(trainIndices);
            double[][] testInputColumns = inputColumns.GetRows(testIndices);
            string[][] testOutputColumns = outputColumns.GetRows(testIndices);

            return (trainInputColumns, trainOutputColumns, testInputColumns, testOutputColumns);
        }
        #endregion
    }
}
