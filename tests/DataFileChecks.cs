using JadeChem.Utils;
using System.Globalization;

internal static class DataFileChecks
{
    public static void Run()
    {
        Check.Run("CSV drops incomplete first, consecutive, and last rows", () =>
            WithCsv("x,y,z\n,2,3\n4,,6\n7,8,9\n10,11,\n", path =>
            {
                using var table = DataFileLoader.LoadCsvFile(path, true);
                Check.Equal(1, table.Rows.Count);
                Check.Near(7, (double)table.Rows[0][0]);
            }));
        Check.Run("CSV accepts uppercase extension and releases file handle", () =>
            WithCsv("x,y\n1,2\n", path =>
            {
                using var table = DataFileLoader.LoadCsvFile(path, true);
                using var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                Check.Equal(1, table.Rows.Count);
            }, ".CSV"));
        Check.Run("CSV preserves quoted text and numeric precision across cultures", () =>
        {
            var previousCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
                WithCsv("smiles,value\n\"C,C\",1.25\nCC,-2.5e-3\n", path =>
                {
                    using var table = DataFileLoader.LoadCsvFile(path, true);
                    Check.Equal("C,C", (string)table.Rows[0][0]);
                    Check.Equal(typeof(double), table.Columns[1].DataType);
                    Check.Near(1.25, (double)table.Rows[0][1]);
                    Check.Near(-0.0025, (double)table.Rows[1][1]);
                });
            }
            finally { CultureInfo.CurrentCulture = previousCulture; }
        });
        Check.Run("CSV with no complete data reports invalid data", () =>
            WithCsv("x,y\n,1\n2,\n", path =>
                Check.Throws<InvalidDataException>(() => DataFileLoader.LoadCsvFile(path, true))));
        Check.Run("CSV without headers retains the first record", () =>
            WithCsv("1,2\n3,4\n", path =>
            {
                using var table = DataFileLoader.LoadCsvFile(path, false);
                Check.Equal(2, table.Rows.Count);
                Check.Near(1, (double)table.Rows[0][0]);
            }));
        Check.Run("All bundled chemistry datasets import successfully", () =>
        {
            string[] paths = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "SampleDatasets"), "*.csv");
            Check.Equal(6, paths.Length);
            foreach (string path in paths)
            {
                using var table = DataFileLoader.LoadCsvFile(path, true);
                Check.Equal(true, table.Rows.Count > 0 && table.Columns.Count > 1);
            }
        });
    }

    private static void WithCsv(string contents, Action<string> action, string extension = ".csv")
    {
        string path = Path.Combine(Path.GetTempPath(), $"jadechem-{Guid.NewGuid():N}{extension}");
        try
        {
            File.WriteAllText(path, contents);
            action(path);
        }
        finally { File.Delete(path); }
    }
}
