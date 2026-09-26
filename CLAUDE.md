# CLAUDE.md

This file provides guidance to Claude Code when working with this repository.

## What this is

JadeChem is a .NET 8 Windows Forms desktop app for building and evaluating ML models on molecular-property datasets. Its MDI shell (`MainForm`) hosts `PredictionTaskForm` children, each with an independent ML workflow. Classical models use Accord.NET, the MLP uses TorchSharp, and molecular features use RDKit through `RDKit2DotNetStandard`.

`JadeChem.sln` and `JadeChem.csproj` are in this repository's root directory. Sibling installer and bundle projects are packaging projects; build the application project directly for normal development.

## Build, run, and verify

Run these commands from the directory containing `JadeChem.csproj`:

```powershell
dotnet build JadeChem.csproj
dotnet build JadeChem.csproj -c Release -p:Platform=x64
dotnet run --project JadeChem.csproj
dotnet run --project tests/JadeChem.RegressionChecks.csproj
```

`tests/` contains a standalone Windows/STA regression-check executable with a project reference to the app. Its runner discovers static `Run()` methods on classes whose names end in `Checks`; use the existing `Check` assertions for new checks. It exercises preprocessing, CSV import, regularized regression, WinForms workflow state, culture handling, and native RDKit/TorchSharp paths. The app project excludes test source files from its own compilation.

Also verify affected UI workflows manually using the CSVs in `SampleDatasets/`. The native RDKit dependency produces MSB3246 "PE image does not have metadata" build warnings; those warnings are expected. Other warnings or failed checks still need review.

## Pipeline architecture

`PredictionTaskForm` owns workflow state. `WorkflowDiagramControl` visualizes the steps and raises navigation events; the form's handlers perform the work.

1. Load input data and select molecular, numeric-input, and output columns.
2. Configure feature extraction, scaling, and optional variance/PCA filters.
3. Process data: extract features and create a processed preview. Preserve unscaled feature rows and raw regression targets in `unscaledInputColumns` and `unscaledOutputColumnForRegression`.
4. Split data: `PredictionTaskForm.Preprocessing.cs` chooses the training indices, fits scalers and filters using only those rows, transforms all rows, and rebuilds the processed, training, and test data. Each new split refits from the cached unscaled data. The preview prepared before splitting is not the final fitted preprocessing state used for training or evaluation.
5. Select and train a model, evaluate against the test split, and predict new rows using the fitted training preprocessor.

Input, feature, processing, and split changes invalidate the dependent model and results. `InvalidateProcessedData`, `ClearModelResults`, and `ClearPredictionResults` keep fields, controls, plots, and workflow indicators consistent. Preserve these invalidation boundaries when changing handlers. Replacing model controls also disposes them; an MLP control owns its native model.

The feature/processing dialog reset flags track whether the selected columns and settings still apply. Classification training splits must contain every retained class so that model output indices agree with the form's class labels. Batch predictions map columns by name into the training feature order; headerless input uses the order displayed in the prediction grid.

**Remaining evaluation limitation:** MLP validation splits are created within the outer training dataset after preprocessing has already been fitted on that outer training dataset. Consequently, the inner validation rows influence scaler/PCA/variance statistics. Outer held-out test rows are excluded from fitting, but the MLP's validation curve is not an independently preprocessed validation estimate. Addressing that limitation requires fitting preprocessing separately for the inner split.

## ModelControl contract

Each model has a paired control in `CustomControls/ModelControls/`. The control owns its hyperparameter UI; `PredictionTaskForm.TrainModel` reads its `Hyperparameters` dictionary and dispatches to the corresponding learner.

Dictionary value types vary: regularized regressors and RandomForest use `double`, KNN uses `int`, LogisticRegression uses `object`, and MultinomialLogisticRegression uses `string`. Inspect both the producer and consumer before changing a type or numeric formatting. String keys such as `"lambda"`, `"k"`, `"max iterations"`, and `"learning rate"` are part of this internal contract.

Adding a model requires its control, training dispatch, evaluation and prediction branches, and any model-specific display update. Check the existing event contract: classical controls raise `TrainButtonClicked`; the MLP control manages its own training and raises model lifecycle events.

## Native-resource lifecycle

- RDKit wrappers (`RWMol`, `ROMol`, `ExplicitBitVect`, `MolDraw2DCairo`) own native resources. Use `using` for local owning instances; parse each molecule once per input row/column and reuse it across features.
- TorchSharp tensors and modules wrap native CPU/GPU allocations. Use explicit disposal or `torch.NewDisposeScope()`, and use `torch.no_grad()` for inference/validation. Do not dispose a module while its owner is still using it.
- Dispose replaced WinForms controls, modal dialogs, and explicitly owned images. Modeless visualization dialogs close with their owner; preserve that ownership when changing their creation.
- Dispose transient GDI objects such as `StringFormat`, and dispose cached pens, brushes, paths, regions, and fonts with their owning control. Do not dispose shared framework brushes such as `Brushes.Black`.
- Molecule rendering uses a unique temporary file and copies the image before deleting that file. Do not restore a fixed image filename in the working directory.

## Locale and culture

CSV numbers and the text-based parameter/optimizer dialogs use a decimal point and `CultureInfo.InvariantCulture`. Format defaults with the same culture used to parse edits; storing a `double` in a grid and later calling its parameterless `ToString()` before invariant parsing breaks on comma-decimal locales.

```csharp
double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value);
value.ToString(CultureInfo.InvariantCulture);
```

Read `NumericUpDown.Value` directly when a numeric control provides a typed value. For other UI text and string-valued hyperparameters, keep the formatting and parsing conventions paired. Use de-DE/fr-FR regression checks when changing this boundary. Validate finiteness where values feed numerical algorithms; successful parsing alone can accept NaN or infinity.

## Where things live

- `Models/`: custom Ridge/Lasso/ElasticNet regressors, their shared numerical helpers, and the TorchSharp MLP. Regularization excludes the intercept; Lasso/ElasticNet use proximal soft-thresholding.
- `Utils/`: scalers, encoders, PCA/variance filtering, train/test splitting, regression metrics, CSV loading, and RDKit conversion helpers.
- `CustomControls/ModelControls/`: hyperparameter and model-display controls.
- `CustomControls/DiagramControls/`: workflow diagram and decision-tree viewer.
- `CustomControls/EvaluationControls/`: classification and regression metrics panels.
- `Dialogs/`: feature, processing, hyperparameter, molecule, and visualization dialogs.
- `SampleDatasets/`: chemistry CSV examples for smoke tests.
- `tests/`: executable regression checks.
- `bin/` and `obj/`: build outputs, including native dependencies.

## Citation

If a change affects published results, the README cites <https://doi.org/10.1002/vjch.70121>.
