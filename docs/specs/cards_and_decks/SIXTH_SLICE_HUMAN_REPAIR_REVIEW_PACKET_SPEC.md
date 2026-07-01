# Sixth-Slice Human Repair Review Packet Spec

Milestone: `M57-01`

## Purpose

`M57-01` converts `M56-06` repair candidates and the `M56-closeout` decision
into a compact human-review packet. The packet lets a reviewer choose one
sixth-slice recipe/repair candidate for later selection, acceptance, and G
Zone/Stride decision.

This milestone does not record selection, does not record acceptance, does not
record a G Zone decision, and does not promote runtime decks.

## Inputs

- `outputs/target_slice/m56_closeout_sixth_slice_runtime_readiness.json`
- `outputs/target_slice/m56_06_sixth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m56_03_sixth_slice_recipe_draft_model.json`

## Outputs

- `outputs/target_slice/m57_01_sixth_slice_human_repair_review_packet.json`
- `outputs/target_slice/m57_01_sixth_slice_human_repair_review_packet.md`
- `outputs/target_slice/m57_01_sixth_slice_human_repair_review_packet.csv`

## Review Rules

- Include all repair candidates that are ready for human repair review.
- Preserve pair context from the recipe draft.
- Preserve manual substitution previews from M56-06.
- Preserve grade-profile additions/removals from M56-06.
- Preserve G Zone deferred system-work context from M56-06.
- Provide decision options only; do not choose an option automatically.
- Mark `ready_for_m57_02=true` only if `M56-closeout` allows human selection
  review and every review item is ready.

## Runtime Boundary

This milestone must not:

- modify M56 recipe drafts
- record human selection
- record human acceptance
- record G Zone/Stride decision
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_sixth_slice_human_repair_review_packet.py
python -m unittest tests.test_sixth_slice_human_repair_review_packet
python -m unittest discover -s tests -p "test_*.py"
```
