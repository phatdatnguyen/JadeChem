# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

JadeChem is a .NET 8 Windows Forms desktop app for building and evaluating ML models on molecular-property datasets. It is an MDI shell (`MainForm`) that hosts one or more `PredictionTaskForm` MDI children — each child is a self-contained ML pipeline. Models come from Accord.NET (classical ML) and TorchSharp (MLP); molecular features come from RDKit via `RDKit2DotNetStandard`.

The solution lives at `JadeChem.sln` (root of the parent directory). The `JadeChemInstaller`, `JadeChemInstaller_x64`, `JadeChemWixBundle`, and `JadeChem_InstallShield` siblings are packaging projects only — never build them as part of normal development.

## Build & run

```bash
# from the JadeChem repo root (the directory containing JadeChem.sln)
dotnet build JadeChem.sln                     # default Debug, AnyCPU
dotnet build JadeChem.sln -c Release -p:Platform=x64
dotnet run --project JadeChem/JadeChem.csproj
```

There are **no automated tests** in this repo. Verification is manual: run the app, load one of the CSVs in `JadeChem/SampleDatasets/` (BBBP, HIV, Lipophilicity, Solubility, HOMO-LUMO_Gap, BuchwaldHartwigReactionYield), and walk the workflow.

The MSB3246 "PE image does not have metadata" warnings during build are from RDKit's native DLLs being copied to the output — they are expected and do not affect the build.

## Pipeline architecture

`PredictionTaskForm` (one per MDI child) drives the user through a fixed sequence of steps, visualized by `CustomControls/DiagramControls/WorkflowDiagramControl`. Click handlers on the form, **not** the diagram, mutate state:

```
LoadDataButton  →  EditFeatureExtractionButton  →  EditProcessingStepsButton
              ↓                ↓                          ↓
         inputData    featuresDictionary       (scaler/PCA/variance config)
                                                      ↓
                                  ProcessButton ── runs feature extraction
                                                  + fits scalers/filters
                                                      ↓
                                              processedDataset
                                                      ↓
                                        SplitDataButton (train/test split)
                                                      ↓
                                  trainDataset / testDataset
                                                      ↓
                                     [pick a ModelControl] → TrainButton
                                                      ↓
                                                   model
                                                      ↓
                                        EvaluateButton → metrics
                                                      ↓
                                  PredictButton / PredictDatasetButton
```

State for each step is held as form fields on `PredictionTaskForm`: `inputData`, `processedDataset`, `trainDataset`, `testDataset`, `model`, `featuresDictionary`, `inputScalersDictionary`, `featureScalersDictionary`, `outputScalersDictionary`, `varianceThresholdFilter`, `pcaFilter`. Two flags (`isFeatureExtractionDialogResetNeeded`, `isDataProcessingDialogResetNeeded`) try to keep dialogs in sync — but **downstream state is not invalidated** when an upstream step is re-run, so editing features after training leaves a stale `model` field. Be careful when touching the workflow handlers.

> **Known correctness caveat**: the scalers, variance threshold, and PCA are fit during `ProcessButton_Click`, which runs *before* `SplitDataButton_Click` — so test-set statistics leak into the preprocessor. Fixing this requires reordering the UX. See the in-progress notes in `~/.claude/plans/this-is-my-c-ethereal-shell.md`.

## ModelControl contract (read this before adding a model)

Every classifier/regressor has a paired UserControl in `CustomControls/ModelControls/` (e.g. `KNNModelControl.cs`, `RidgeRegressionModelControl.cs`, `MLPModelControl.cs`). The control owns the hyperparameter UI; `PredictionTaskForm.TrainModel` reaches in and reads values out of a public `Hyperparameters` dictionary by string key.

The dictionary's value type **is not consistent**:

- Most controls expose `Dictionary<string, double>` (Ridge, Lasso, ElasticNet, RandomForest, KNN — k is stored as int promoted to double).
- A few expose `Dictionary<string, object>` so they can carry `string`/`bool` settings (e.g. LogisticRegression's `"learning method"` is a string, `"stochastic"` is a bool; MultinomialLogisticRegression stores numerics as strings and the call site uses `int.Parse(...)` — note: pending fix to `TryParse` + `InvariantCulture`).

The string keys (`"lambda"`, `"k"`, `"max iterations"`, `"learning rate"`, `"learning method"`, `"Set intercept = 0"`, etc.) are the API. Renaming a key requires updating both the control's setter and `PredictionTaskForm.TrainModel`.

When the train button is clicked the control raises a custom `TrainButtonClicked` event; `PredictionTaskForm` subscribes and dispatches to a per-model branch in `TrainModel(...)`. There's no polymorphism — adding a model means: new `ModelControl`, new branch in `TrainModel`, new branch in `EvaluateButton_Click` and the prediction methods, and (for regressors with weights) a `UpdateFittingEquation` call.

## Native-resource lifecycle (don't skip this)

Several dependencies wrap native (C++) memory and **must** be disposed promptly:

- **RDKit (`GraphMolWrap.RWMol`, `ROMol`, `ExplicitBitVect`, `MolDraw2DCairo`)** — wrap libRDKit. Always `using` them. The feature-extraction loop processes thousands of molecules; a missing `using` here leaks tens of thousands of native allocations per Process click. The molecule is parsed once per (row, column) and reused across features — do not move parsing back inside the per-feature loop.
- **TorchSharp tensors and modules** (`MLPModelControl`, `Models/MLP.cs`) — wrap libtorch. Either dispose explicitly or wrap the work in `torch.NewDisposeScope()`. Tensors created in inference loops accumulate fast on the GPU.
- **OxyPlot `PlotModel` / `PlotView`** in the visualization dialogs (`Dialogs/Visualize*Dialog.cs`) — they own bitmaps and series; the dialogs are currently shown modeless via `.Show(this)` without being tracked, which can leak handles when many are opened.
- **GDI objects** in `WorkflowDiagramControl` and `DecisionTreeViewControl` — `Pen`, `Brush`, `StringFormat`, `GraphicsPath`, `Font`. The diagram's `OnPaint` and `OnMouseMove` allocate per call; use `using` for transients and dispose cached fields in `Dispose(bool)`.

If you add new code that touches any of the above, treat disposal as a correctness concern, not a hygiene concern.

## Locale & culture

User-entered numerics (in dialogs and CSV data) must be parsed with `CultureInfo.InvariantCulture`. The app shipped originally with bare `double.Parse` / `double.TryParse`, which silently mis-parses on non-English Windows (de-DE, fr-FR, vi-VN). New code should use:

```csharp
double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double v)
value.ToString(CultureInfo.InvariantCulture)
```

`Utils/DataFileLoader.cs`, `Dialogs/EditParametersDialog.cs`, and `Dialogs/ConfigureOptimizerAndSchedulerDialog.cs` already follow this pattern; match it.

## Where things live

- `Models/` — custom regularized regressors (`LassoRegression`, `RidgeRegression`, `ElasticNetRegression`) and the TorchSharp `MLP`. Lasso/ElasticNet use proximal soft-thresholding; the intercept (last appended column) is excluded from regularization.
- `Utils/` — preprocessing primitives (`StandardScaler`, `MinMaxScaler`, `OneHotEncoder`, `PCAFilter`, `VarianceThresholdFilter`, `TrainTestSpliter`), `RegressionMetrics`, `DataFileLoader`, `Conversion` (RDKit ↔ array helpers).
- `CustomControls/DiagramControls/` — the workflow diagram and decision-tree viewer.
- `CustomControls/EvaluationControls/` — per-task-type evaluation panels (binary/multiclass/regression).
- `Dialogs/` — modal/modeless dialogs for feature extraction, processing steps, hyperparameter editing, visualization.
- `SampleDatasets/` — CSV inputs for manual testing; `BBBP.csv` / `HIV.csv` are classification, `Lipophilicity.csv` / `Solubility.csv` / `HOMO-LUMO_Gap.csv` are regression.
- `bin/` and `obj/` — build outputs; the RDKit native DLLs land here on build.

## Citation

If you change anything that affects published results, the README cites: <https://doi.org/10.1002/vjch.70121>.
