using JadeChem.Dialogs;
using System.Globalization;
using System.Reflection;

internal static class MergeChecks
{
    public static void Run()
    {
        foreach (string cultureName in new[] { "de-DE", "fr-FR" })
        {
            Check.Run($"Parameter dialog round-trips invariant defaults and edits under {cultureName}", () =>
                WithCulture(cultureName, () =>
                {
                    using var dialog = new EditParametersDialog
                    {
                        Parameters = new Dictionary<string, double> { ["threshold"] = 0.125 }
                    };
                    Invoke(dialog, "EditParametersDialog_Load", dialog, EventArgs.Empty);
                    var grid = GetGrid(dialog, "parametersDataGridView");
                    // Check before invoking FormClosing: a regression must fail without opening an error message box.
                    Check.Equal("0.125", grid.Rows[0].Cells[1].Value as string);
                    Accept(dialog);
                    Check.Near(0.125, dialog.Parameters["threshold"]);

                    grid.Rows[0].Cells[1].Value = "0.375";
                    Accept(dialog);
                    Check.Near(0.375, dialog.Parameters["threshold"]);
                }));

            Check.Run($"Optimizer dialog round-trips invariant defaults and edits under {cultureName}", () =>
                WithCulture(cultureName, () =>
                {
                    using var dialog = new ConfigureOptimizerAndSchedulerDialog(
                        "Adam", new() { ["learning rate"] = 0.001 },
                        "ExponentialLR", new() { ["gamma"] = 0.95 });
                    var optimizerGrid = GetGrid(dialog, "optimizerParametersDataGridView");
                    var schedulerGrid = GetGrid(dialog, "lrSchedulerParametersDataGridView");
                    Check.Equal("0.001", optimizerGrid.Rows[0].Cells[1].Value as string);
                    Check.Equal("0.95", schedulerGrid.Rows[0].Cells[1].Value as string);
                    Accept(dialog);
                    Check.Near(0.001, dialog.OptimizerParameters["learning rate"]);
                    Check.Near(0.95, dialog.LRSchedulerParameters["gamma"]);

                    optimizerGrid.Rows[0].Cells[1].Value = "0.0025";
                    schedulerGrid.Rows[0].Cells[1].Value = "0.875";
                    Accept(dialog);
                    Check.Near(0.0025, dialog.OptimizerParameters["learning rate"]);
                    Check.Near(0.875, dialog.LRSchedulerParameters["gamma"]);
                }));
        }
    }

    private static void Accept(Form dialog)
    {
        dialog.DialogResult = DialogResult.OK;
        var args = new FormClosingEventArgs(CloseReason.UserClosing, false);
        Invoke(dialog, "EditParametersDialog_FormClosing", dialog, args);
        Check.Equal(false, args.Cancel);
    }

    private static void WithCulture(string name, Action action)
    {
        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static DataGridView GetGrid(object instance, string field) =>
        (DataGridView)instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;

    private static void Invoke(object instance, string method, params object[] arguments) =>
        instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, arguments);
}
