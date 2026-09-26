# Regression checks

Run on Windows with the .NET 8 (or newer) SDK:

```powershell
dotnet run --project tests/JadeChem.RegressionChecks.csproj
```

The console runner returns a nonzero exit code on failure. It uses the application's
existing dependencies without an additional test framework. WinForms checks run on
an STA thread without displaying windows.

Coverage includes CSV import and the bundled datasets, train/test sampling,
constant-column scaling, train-only preprocessing, PCA, regularized regression,
saved dialog settings, prediction column order, and workflow invalidation.

Behavioral changes to account for when comparing historical results:

- The corrected random splitter can now select row zero. A fixed seed remains
  reproducible, but may select different rows than older versions.
- Creating or changing a train/test split refits scaling and dimensionality reduction
  on training rows and updates the processed preview. Changing data or preprocessing
  settings clears dependent models and results.
- Corrected regularization and PCA can change fitted coefficients and predictions.
- CSV numbers use decimal points independently of the Windows display locale.

Remaining limitations: training still runs on the UI thread. MLP's optional internal
validation split receives data preprocessed on the entire outer training pool;
its validation curves can therefore be optimistic. Use the separate test split
for held-out evaluation. GPU execution and every model/hyperparameter combination
are outside this regression suite's coverage.
