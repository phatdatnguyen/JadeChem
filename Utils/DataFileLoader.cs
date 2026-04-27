using System.Data;
using System.Globalization;
using Accord.IO;
using Accord.Math;

namespace JadeChem.Utils
{
    public static class DataFileLoader
    {
        #region Method
        public static DataTable LoadCsvFile(string filePath, bool hasHeaders)
        {
            string extension = Path.GetExtension(filePath);
            if (extension != ".csv")
                throw new Exception("Cannot open the selected file!");

            // Load the .csv file
            CsvReader csvReader = new(filePath, hasHeaders)
            {
                Delimiter = ',',
                SkipEmptyLines = true,
                MissingFieldAction = MissingFieldAction.ReplaceByEmpty
            };
            DataTable loadedData = csvReader.ToTable();

            // Identify rows containing any null/empty cell, then remove them in reverse order.
            // The previous in-place forward loop with rowIndex-- could skip rows because the inner
            // column loop kept going against shifted rows after a removal.
            List<int> rowsToRemove = new();
            for (int rowIndex = 0; rowIndex < loadedData.Rows.Count; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < loadedData.Columns.Count; columnIndex++)
                {
                    object cell = loadedData.Rows[rowIndex][columnIndex];
                    if (cell == DBNull.Value || (cell is string s && s.Length == 0))
                    {
                        rowsToRemove.Add(rowIndex);
                        break;
                    }
                }
            }
            for (int i = rowsToRemove.Count - 1; i >= 0; i--)
                loadedData.Rows.RemoveAt(rowsToRemove[i]);

            loadedData.AcceptChanges();

            // Check for invalid data
            if (loadedData.Columns.Count < 1 || loadedData.Rows.Count == 0)
                throw new Exception("Invalid data!");

            // Convert number column to double type
            DataTable inputData = new();
            HashSet<int> numericColumnIndices = new();
            for (int columnIndex = 0; columnIndex < loadedData.Columns.Count; columnIndex++)
            {
                string columnName = loadedData.Columns[columnIndex].ColumnName;

                try
                {
                    loadedData.Columns[columnIndex].ToArray(); // This will throw an exception if any data is not numeric
                    numericColumnIndices.Add(columnIndex);
                    inputData.Columns.Add(columnName, typeof(double));
                }
                catch
                {
                    inputData.Columns.Add(columnName, typeof(string));
                }
            }
            for (int rowIndex = 0; rowIndex < loadedData.Rows.Count; rowIndex++)
            {
                DataRow row = inputData.NewRow();
                for (int columnIndex = 0; columnIndex < loadedData.Columns.Count; columnIndex++)
                {
                    object cell = loadedData.Rows[rowIndex][columnIndex];
                    if (numericColumnIndices.Contains(columnIndex) && cell != DBNull.Value)
                        // Use invariant culture so "3.14" parses correctly on locales where ',' is the decimal separator.
                        row[columnIndex] = double.Parse((string)cell, NumberStyles.Float, CultureInfo.InvariantCulture);
                    else
                        row[columnIndex] = (string)cell;
                }
                inputData.Rows.Add(row);
            }

            loadedData.AcceptChanges();

            // Return the data
            return inputData;
        }
        #endregion
    }
}
