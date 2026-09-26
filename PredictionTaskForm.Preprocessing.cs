using Accord.Math;
using JadeChem.Utils;
using System.Data;

namespace JadeChem
{
    public partial class PredictionTaskForm
    {
        // Retain extracted, unscaled rows so every new split can fit independently.
        private double[][]? unscaledInputColumns;
        private double[]? unscaledOutputColumnForRegression;

        private void RefitPreprocessingForSplit(double testSize, int randomSeed)
        {
            if (unscaledInputColumns == null || preprocessedInputColumnNames == null || outputColumnName == null)
                throw new InvalidOperationException("Process the data before splitting it.");

            TrainTestSpliter splitter = new();
            splitter.Split(unscaledInputColumns, unscaledInputColumns, testSize, randomSeed);
            int[] trainIndices = splitter.TrainIndices;
            if (predictionType != PredictionType.Regression &&
                (processedOutputColumnForClassification == null || classLabels == null ||
                 trainIndices.Select(index => processedOutputColumnForClassification[index]).Distinct().Count() != classLabels.Length))
                throw new InvalidOperationException("The training split must contain every class. Choose another split ratio or random seed.");

            Dictionary<string, StandardScaler> fittedStandardScalers = new();
            Dictionary<string, MinMaxScaler> fittedMinMaxScalers = new();

            double[] FitColumn(double[] values, string name, Dictionary<string, (double, double)> settings)
            {
                if (settings.ContainsKey("Standardization"))
                {
                    StandardScaler scaler = new();
                    scaler.Fit(values.Get(trainIndices));
                    values = scaler.Transform(values);
                    fittedStandardScalers[name] = scaler;
                }
                if (settings.TryGetValue("Min-max scaling", out var range))
                {
                    MinMaxScaler scaler = new(range.Item1, range.Item2);
                    scaler.Fit(values.Get(trainIndices));
                    values = scaler.Transform(values);
                    fittedMinMaxScalers[name] = scaler;
                }
                return values;
            }

            double[][] transformed = unscaledInputColumns.Select(row => (double[])row.Clone()).ToArray();
            for (int columnIndex = 0; columnIndex < preprocessedInputColumnNames.Length; columnIndex++)
            {
                string name = preprocessedInputColumnNames[columnIndex];
                if (!inputScalersDictionary.TryGetValue(name, out var settings) &&
                    !featureScalersDictionary.TryGetValue(name, out settings))
                    continue; // Fingerprint bits are left unscaled.

                double[] column = FitColumn(unscaledInputColumns.GetColumn(columnIndex), name, settings);
                for (int rowIndex = 0; rowIndex < transformed.Length; rowIndex++)
                    transformed[rowIndex][columnIndex] = column[rowIndex];
            }

            string[] names = preprocessedInputColumnNames;
            VarianceThresholdFilter? fittedVarianceFilter = null;
            PCAFilter? fittedPcaFilter = null;
            if (dimensionalityReductionStepsDictionary.TryGetValue("Variance threshold", out var varianceSettings))
            {
                fittedVarianceFilter = new();
                fittedVarianceFilter.Fit(names, transformed.GetRows(trainIndices), varianceSettings["threshold"]);
                (names, transformed) = fittedVarianceFilter.Transform(transformed);
            }
            if (dimensionalityReductionStepsDictionary.TryGetValue("Principle component analysis", out var pcaSettings))
            {
                fittedPcaFilter = new();
                fittedPcaFilter.Fit(transformed.GetRows(trainIndices), (int)pcaSettings["nComponents"]);
                (names, transformed) = fittedPcaFilter.Transform(transformed);
            }

            double[]? transformedOutput = null;
            DataTable dataset;
            if (predictionType == PredictionType.Regression)
            {
                if (unscaledOutputColumnForRegression == null)
                    throw new InvalidOperationException("Process the regression targets before splitting the data.");
                transformedOutput = unscaledOutputColumnForRegression;
                if (outputScalersDictionary.TryGetValue(outputColumnName, out var settings))
                    transformedOutput = FitColumn(transformedOutput, outputColumnName, settings);
                dataset = transformed.Concatenate(transformedOutput.ToJagged()).ToTable(names.Concatenate(outputColumnName));
            }
            else
            {
                if (processedOutputColumnForClassification == null)
                    throw new InvalidOperationException("Process the class labels before splitting the data.");
                dataset = transformed.ToTable(names);
                dataset.Columns.Add(outputColumnName, typeof(string));
                for (int rowIndex = 0; rowIndex < transformed.Length; rowIndex++)
                    dataset.Rows[rowIndex][outputColumnName] = processedOutputColumnForClassification[rowIndex];
            }

            // Commit only after all fitting succeeds; a rejected split keeps the preview intact.
            standardScalers.Clear();
            foreach (var entry in fittedStandardScalers)
                standardScalers.Add(entry.Key, entry.Value);
            minMaxScalers.Clear();
            foreach (var entry in fittedMinMaxScalers)
                minMaxScalers.Add(entry.Key, entry.Value);
            varianceThresholdFilter = fittedVarianceFilter;
            pcaFilter = fittedPcaFilter;
            processedInputColumnNames = names;
            processedInputColumns = transformed;
            processedOutputColumnForRegression = transformedOutput;
            processedDataset = dataset;
            processedDataDataGridView.DataSource = dataset.Columns.Count <= 600 ? dataset : null;
        }
    }
}
