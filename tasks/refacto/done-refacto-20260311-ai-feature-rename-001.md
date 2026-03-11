# done-refacto-20260311-ai-feature-rename-001 -- Rename French feature file to English
**Priority** : mineure
**Files concerned** :
- `tests/Vetolib.Tests.Acceptance/Features/AI/TriageVeterinaire.feature` -> `VeterinaryTriage.feature`
- `tests/Vetolib.Tests.Acceptance/Features/AI/TriageVeterinaire.feature.cs` -> `VeterinaryTriage.feature.cs`
**Violation** : Project convention is English-only (UAE market). Filename was in French.
**Correction applied** : Renamed `TriageVeterinaire.feature` to `VeterinaryTriage.feature` and updated `#line` references in the auto-generated `.feature.cs` file.
**Status** : DONE

## Audit notes
- Feature file content was already fully in English (title: "AI Veterinary Triage")
- Step definitions in `TriageSteps.cs` already had `[Scope(Feature = "AI Veterinary Triage")]` -- no change needed
- NoShowPrediction feature file and steps were already fully in English
- The "missing steps" `WhenTheAIConfidenceIsBelow` and `WhenTheAIIsUncertainBetweenAnd` do not correspond to any scenario in the feature files -- they were never added to the `.feature` files and are therefore not missing
- All step bindings in both `.feature` files have matching implementations in their respective step definition files
