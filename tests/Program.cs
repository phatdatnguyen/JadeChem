using System.Reflection;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        ApplicationConfiguration.Initialize();
        foreach (Type suite in Assembly.GetExecutingAssembly().GetTypes()
                     .Where(type => type.Name.EndsWith("Checks", StringComparison.Ordinal))
                     .OrderBy(type => type.Name))
        {
            try
            {
                suite.GetMethod("Run", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!
                    .Invoke(null, null);
            }
            catch (Exception exception)
            {
                Check.Fail(suite.Name, exception.InnerException ?? exception);
            }
        }

        Console.WriteLine($"{Check.Passed} passed; {Check.Failed} failed.");
        return Check.Failed == 0 ? 0 : 1;
    }
}

internal static class Check
{
    public static int Passed { get; private set; }
    public static int Failed { get; private set; }

    public static void Run(string name, Action action)
    {
        try
        {
            action();
            Passed++;
            Console.WriteLine($"PASS {name}");
        }
        catch (Exception exception)
        {
            Fail(name, exception);
        }
    }

    public static void Fail(string name, Exception exception)
    {
        Failed++;
        Console.Error.WriteLine($"FAIL {name}: {exception}");
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"Expected {expected}; got {actual}.");
    }

    public static void Near(double expected, double actual, double tolerance = 1e-9)
    {
        if (!double.IsFinite(actual) || Math.Abs(expected - actual) > tolerance)
            throw new Exception($"Expected {expected} ± {tolerance}; got {actual}.");
    }

    public static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}.");
    }
}
