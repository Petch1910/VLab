# Eighth-Slice Human-Accepted Grade Repair Artifact Spec

Milestone: `M65-03`

## Purpose

`M65-03` records the explicit human grade-repair decision for the recipe
selected in `M65-02` and builds an in-memory repaired main-deck preview.

The eighth slice has no manual-overlap repair path. This milestone applies the
selected grade-profile repair package directly when the human decision is
`accepted`. It may also record a `rejected` grade decision, but rejected
decisions are not ready for `M65-04`.

Lock and Legion system decisions belong to `M65-04`; repaired validation
belongs to `M65-05`.

## Inputs

- `outputs/target_slice/m65_02_eighth_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m64_03_eighth_slice_recipe_draft_model.json`
- CLI/user input: `decision_text`
- CLI/user input: `grade_decision` (`accepted` or `rejected`)

Tests may pass in-memory M64/M65 artifacts until real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m65_03_eighth_slice_human_accepted_grade_repair_artifact.json`
- `outputs/target_slice/m65_03_eighth_slice_human_accepted_grade_repair_artifact.md`

## Decision Rules

- `M65-02` selected artifact must exist before CLI generation.
- `decision_text` must be non-empty.
- `grade_decision` must be `accepted` or `rejected`.
- Accepted decisions apply the selected grade removals/additions in memory only.
- Rejected decisions are recorded but keep `ready_for_m65_04=false`.
- `ready_for_m65_04=true` only when the grade decision is accepted, the
  selected artifact is ready for M65-03, the repaired main deck preview has 50
  cards, the grade profile reaches the target counts, and no repair-application
  blocker exists.
- Deferred Lock and Legion work do not block accepted M65-03 output; they are
  the subject of `M65-04`.

## Runtime Boundary

This milestone must not:

- record a grade decision without a prior M65-02 selected artifact
- record a Lock decision
- record a Legion decision
- declare the repaired recipe valid
- modify M64/M65 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable Lock runtime
- enable Legion runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_eighth_slice_human_accepted_grade_repair_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M65-02 output exists:

```powershell
python tools\deck\build_eighth_slice_human_accepted_grade_repair_artifact.py `
  --grade-decision accepted `
  --decision-text "<explicit user grade repair decision>"
```

## Done Rule

`M65-03` is done when:

- explicit decision text is required and recorded
- accepted grade repairs are applied only in memory
- rejected grade decisions are recorded but block M65-04 readiness
- repaired preview has 50 cards and target grade profile for valid accepted
  items
- Lock and Legion decisions remain deferred
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m65_04=true` for valid accepted items
