# Fifth-Slice Human-Accepted Repair Artifact Spec

Milestone: `M53-03`

## Purpose

`M53-03` records explicit acceptance for the recipe selected in `M53-02` and
applies the selected grade-profile repair preview in memory.

This milestone does not validate the repaired deck and does not promote runtime
fixtures. Validation belongs to `M53-04`.

## Inputs

- `outputs/target_slice/m53_02_fifth_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m52_03_fifth_slice_recipe_draft_model.json`
- CLI/user input: `acceptance_text`

## Outputs

- `outputs/target_slice/m53_03_fifth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m53_03_fifth_slice_human_accepted_repair_artifact.md`

## Acceptance Rules

- `M53-02` selection artifact must exist before CLI generation.
- `acceptance_text` must be non-empty.
- Acceptance records one selected review item and one selected recipe.
- Apply grade-profile removals/additions in memory only.
- `ready_for_m53_04=true` only when repair application produces a 50-card
  main deck with no application issues.

## Runtime Boundary

This milestone must not:

- record acceptance without a prior M53-02 selected artifact
- declare the repaired recipe valid
- modify M52/M53 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_fifth_slice_human_accepted_repair_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M53-02 output exists:

```powershell
python tools\deck\build_fifth_slice_human_accepted_repair_artifact.py `
  --acceptance-text "<explicit user acceptance>"
```
