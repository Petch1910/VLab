# Ninth-Slice Human-Accepted Repair Artifact Spec

Milestone: `M69-03`

## Purpose

`M69-03` records an explicit repair decision for the recipe selected in
`M69-02` and builds an in-memory repaired main-deck preview.

The ninth slice can include manual-review substitutions, grade-profile repair
previews, deferred G Zone work, deferred Stride work, and deferred Aqua Force
battle-order work. This milestone accepts or rejects only the selected
manual/grade repair path. It does not make the G Zone, Stride, or Aqua Force
system decision and does not validate the repaired deck.

System decision belongs to `M69-04`; repaired validation belongs to `M69-05`.

## Inputs

- `outputs/target_slice/m69_02_ninth_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m68_03_ninth_slice_recipe_draft_model.json`
- `outputs/target_slice/m68_01_ninth_slice_fixture_scaffold.json`
- CLI/user input: `decision_text`
- CLI/user input: `repair_decision` (`accepted` or `rejected`)

Tests may pass in-memory M68/M69 artifacts until real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m69_03_ninth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m69_03_ninth_slice_human_accepted_repair_artifact.md`

## Decision Rules

- `M69-02` selected artifact must exist before CLI generation.
- `decision_text` must be non-empty.
- `repair_decision` must be `accepted` or `rejected`.
- Accepted decisions apply selected manual substitutions in memory only.
- Accepted decisions recompute the grade-profile repair package from
  post-manual quantities.
- Rejected decisions are recorded but keep `ready_for_m69_04=false`.
- `ready_for_m69_04=true` only when the repair decision is accepted, the
  selected artifact is ready for M69-03, the repaired main deck preview has 50
  cards, the recomputed grade package reaches the target grade profile, and no
  repair-application blocker exists.
- Deferred G Zone, Stride, and Aqua Force battle-order work do not block
  `M69-04`; they are the subject of `M69-04`.

## Runtime Boundary

This milestone must not:

- record a repair decision without a prior M69-02 selected artifact
- record a G Zone decision
- record a Stride decision
- record an Aqua Force battle-order decision
- declare the repaired recipe valid
- modify M68/M69 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_ninth_slice_human_accepted_repair_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M69-02 output exists:

```powershell
python tools\deck\build_ninth_slice_human_accepted_repair_artifact.py `
  --repair-decision accepted `
  --decision-text "<explicit user repair decision>"
```

## Done Rule

`M69-03` is done when:

- explicit decision text is required and recorded
- accepted repairs are applied only in memory
- rejected repair decisions are recorded but block M69-04 readiness
- grade repair is recomputed after manual substitutions
- repaired preview has 50 cards and target grade profile for valid accepted
  items
- G Zone, Stride, and Aqua Force battle-order decisions remain deferred
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m69_04=true` for valid accepted items
