using System.Data;
using Accord.IO;
using System.Globalization;

namespace JadeChem.Utils
{
    public static class DataFileLoader
    {
        #region Method
        public static DataTable LoadCsvFile(string filePath, bool hasHeaders)
        {
            string extension = Path.GetExtension(filePath);
            if (string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                // Load the .csv file
                using CsvReader csvReader = new(filePath, hasHeaders)
                {
                    Delimiter = ',',
                    SkipEmptyLines = true,
                    MissingFieldAction = MissingFieldAction.ReplaceByEmpty
                };
                using DataTable loadedData = csvReader.ToTable();

                // Remove null row
                for (int rowIndex = loadedData.Rows.Count - 1; rowIndex >= 0; rowIndex--)
                {
                    for (int columnIndex = 0; columnIndex < loadedData.Columns.Count; columnIndex++)
                        if (loadedData.Rows[rowIndex][columnIndex] == DBNull.Value ||
                            string.IsNullOrWhiteSpace((string)loadedData.Rows[rowIndex][columnIndex]))
                        {
                            loadedData.Rows.Remove(loadedData.Rows[rowIndex]);
                            break;
                        }
                }

                loadedData.AcceptChanges();

                // Check for invalid data
                if (loadedData.Columns.Count < 1 || loadedData.Rows.Count == 0)
                    throw new InvalidDataException("The CSV file contains no complete data rows.");

                // Convert number column to double type
                DataTable inputData = new();
                bool[] numericColumns = new bool[loadedData.Columns.Count];
                for (int columnIndex = 0; columnIndex < loadedData.Columns.Count; columnIndex++)
                {
                    string columnName = loadedData.Columns[columnIndex].ColumnName;

                    // CSV numeric values use a decimal point, independently of the UI locale.
                    numericColumns[columnIndex] = loadedData.Rows.Cast<DataRow>().All(row =>
                        double.TryParse((string)row[columnIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out _));
                    inputData.Columns.Add(columnName, numericColumns[columnIndex] ? typeof(double) : typeof(string));
                }
                for (int rowIndex = 0; rowIndex < loadedData.Rows.Count; rowIndex++)
                {
                    DataRow row = inputData.NewRow();
                    for (int columnIndex = 0; columnIndex < loadedData.Columns.Count; columnIndex++)
                    {
                        if (numericColumns[columnIndex]) // numeric data
                            row[columnIndex] = double.Parse((string)loadedData.Rows[rowIndex][columnIndex], NumberStyles.Float, CultureInfo.InvariantCulture);
                        else // string data
                            row[columnIndex] = (string)loadedData.Rows[rowIndex][columnIndex];
                    }
                    inputData.Rows.Add(row);
                }

                inputData.AcceptChanges();

                // Return the data
                return inputData;
            }
            else
            {
                throw new InvalidDataException("Cannot open the selected file. Select a CSV file.");
            }
        }
        #endregion
    }
}
