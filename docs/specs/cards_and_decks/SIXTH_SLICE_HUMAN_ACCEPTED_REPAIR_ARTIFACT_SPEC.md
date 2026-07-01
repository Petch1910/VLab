# Sixth-Slice Human-Accepted Repair Artifact Spec

Milestone: `M57-03`

## Purpose

`M57-03` records explicit acceptance for the recipe selected in `M57-02` and
builds an in-memory repaired main-deck preview.

The sixth slice can include both manual-review substitutions and grade-profile
repair previews. Because those two repair sources can target overlapping cards,
the accepted artifact must apply manual substitutions first, then recompute the
grade-profile repair package from the post-manual quantities.

This milestone does not validate the repaired deck, does not record a G Zone /
Stride decision, and does not promote runtime fixtures. G Zone / Stride
decision belongs to `M57-04`; validation belongs to `M57-05`.

## Inputs

- `outputs/target_slice/m57_02_sixth_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m56_03_sixth_slice_recipe_draft_model.json`
- `outputs/target_slice/m56_01_sixth_slice_fixture_scaffold.json`
- CLI/user input: `acceptance_text`

## Outputs

- `outputs/target_slice/m57_03_sixth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m57_03_sixth_slice_human_accepted_repair_artifact.md`

## Acceptance Rules

- `M57-02` selection artifact must exist before CLI generation.
- `acceptance_text` must be non-empty.
- Acceptance records one selected review item and one selected recipe.
- Apply selected manual substitutions in memory only.
- Detect source grade-package conflicts after manual substitutions.
- Recompute the grade-profile repair package from post-manual quantities.
- `ready_for_m57_04=true` only when acceptance is recorded, the repaired main
  deck preview has 50 cards, the recomputed grade package reaches the target
  grade profile, and no repair-application blocker exists.

## Runtime Boundary

This milestone must not:

- record acceptance without a prior M57-02 selected artifact
- record a G Zone / Stride decision
- declare the repaired recipe valid
- modify M56/M57 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_slice_human_accepted_repair_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M57-02 output exists:

```powershell
python tools\deck\build_sixth_slice_human_accepted_repair_artifact.py `
  --acceptance-text "<explicit user acceptance>"
```
