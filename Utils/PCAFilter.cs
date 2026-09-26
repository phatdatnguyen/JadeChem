using Accord.Math;
using Accord.Statistics.Analysis;

namespace JadeChem.Utils
{
    public class PCAFilter
    {
        #region Fields
        private string[] filteredColumnNames = Array.Empty<string>();
        private PrincipalComponentAnalysis? pca;
        #endregion

        #region Constructor
        public PCAFilter() { }
        #endregion

        #region Methods
        public (string[], double[][]) FitTransform(double[][] inputColumns, int nComponents)
        {
            Fit(inputColumns, nComponents);
            return Transform(inputColumns);
        }

        public void Fit(double[][] inputColumns, int nComponents)
        {
            ArgumentNullException.ThrowIfNull(inputColumns);
            if (nComponents < 1)
                throw new ArgumentOutOfRangeException(nameof(nComponents), "PCA requires at least one component.");
            if (inputColumns.Length < 2)
                throw new ArgumentException("PCA requires at least two training rows.", nameof(inputColumns));
            int columnCount = inputColumns[0]?.Length ?? 0;
            if (columnCount == 0 || inputColumns.Any(row => row == null || row.Length != columnCount || row.Any(value => !double.IsFinite(value))))
                throw new ArgumentException("PCA requires a rectangular matrix of finite values with at least one column.", nameof(inputColumns));

            // Create a pca instance
            PrincipalComponentAnalysis fittedPca = new()
            {
                Method = PrincipalComponentMethod.Center,
                NumberOfOutputs = Math.Min(columnCount, nComponents),
                Whiten = true
            };

            // Train the pca
            fittedPca.Learn(inputColumns);

            // Whitening divides each component by its singular value. Centered
            // data with few rows or dependent features has zero-variance components;
            // retaining them produces NaN or amplifies round-off error in predictions.
            double largestSingularValue = fittedPca.SingularValues.Max();
            double tolerance = largestSingularValue * Math.Max(inputColumns.Length, columnCount) * 2.2204460492503131e-16;
            int rank = fittedPca.SingularValues.Count(value => double.IsFinite(value) && value > tolerance);
            if (rank == 0)
                throw new ArgumentException("PCA requires variation in the training data.", nameof(inputColumns));

            fittedPca.NumberOfOutputs = Math.Min(fittedPca.NumberOfOutputs, rank);

            // Get the column names
            string[] names = new string[fittedPca.NumberOfOutputs];

            for (int columnIndex = 0; columnIndex < fittedPca.NumberOfOutputs; columnIndex++)
                names[columnIndex] = "PC" + (columnIndex + 1).ToString();

            pca = fittedPca;
            filteredColumnNames = names;
        }

        public (string[], double[][]) Transform(double[][] inputColumns)
        {
            if (pca == null)
                throw new InvalidOperationException("Fit the PCA filter before transforming data.");

            double[][] filteredColumns = pca.Transform(inputColumns);

            return (filteredColumnNames, filteredColumns);
        }

        public double[] Transform(double[] inputRow)
        {
            if (pca == null)
                throw new InvalidOperationException("Fit the PCA filter before transforming data.");

            double[][] filteredData = pca.Transform(inputRow.ToJagged(false));
            return filteredData.GetRow(0);
        }
        #endregion
    }
}
