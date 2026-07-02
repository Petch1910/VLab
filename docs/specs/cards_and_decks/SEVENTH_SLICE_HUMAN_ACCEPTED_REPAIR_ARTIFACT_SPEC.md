# Seventh-Slice Human-Accepted Repair Artifact Spec

Milestone: `M61-03`

## Purpose

`M61-03` records explicit acceptance for the recipe selected in `M61-02` and
builds an in-memory repaired main-deck preview.

The seventh slice can include manual-review substitutions, grade-profile repair
previews, deferred G Zone / Stride work, and deferred Bloom/token work. This
milestone accepts only the selected manual/grade repair path. It does not make
the G Zone / Stride or Bloom/token system decision and does not validate the
repaired deck.

System decision belongs to `M61-04`; repaired validation belongs to `M61-05`.

## Inputs

- `outputs/target_slice/m61_02_seventh_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m60_03_seventh_slice_recipe_draft_model.json`
- `outputs/target_slice/m60_01_seventh_slice_fixture_scaffold.json`
- CLI/user input: `acceptance_text`

Tests may pass in-memory M60/M61 artifacts until real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m61_03_seventh_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m61_03_seventh_slice_human_accepted_repair_artifact.md`

## Acceptance Rules

- `M61-02` selected artifact must exist before CLI generation.
- `acceptance_text` must be non-empty.
- Acceptance records one selected review item and one selected recipe.
- Apply selected manual substitutions in memory only.
- Detect source grade-package conflicts after manual substitutions.
- Recompute the grade-profile repair package from post-manual quantities.
- `ready_for_m61_04=true` only when acceptance is recorded, the repaired main
  deck preview has 50 cards, the recomputed grade package reaches the target
  grade profile, and no repair-application blocker exists.
- Deferred G Zone / Stride and Bloom/token work do not block `M61-04`; they
  are the subject of `M61-04`.

## Runtime Boundary

This milestone must not:

- record acceptance without a prior M61-02 selected artifact
- record a G Zone / Stride decision
- record a Bloom/token decision
- declare the repaired recipe valid
- modify M60/M61 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable Bloom/token runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_seventh_slice_human_accepted_repair_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M61-02 output exists:

```powershell
python tools\deck\build_seventh_slice_human_accepted_repair_artifact.py `
  --acceptance-text "<explicit user acceptance>"
```

## Done Rule

`M61-03` is done when:

- explicit acceptance text is required and recorded
- selected manual repairs are applied only in memory
- grade repair is recomputed after manual substitutions
- repaired preview has 50 cards and target grade profile for valid selected
  items
- G Zone / Stride and Bloom/token decisions remain deferred
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m61_04=true` for valid accepted items
