# Fifth-Slice Human Repair Review Packet Spec

Milestone: `M53-01`

## Purpose

`M53-01` converts `M52-06` repair candidates and the `M52-closeout` decision
into a compact human-review packet. The packet lets a reviewer choose one
fifth-slice recipe/repair candidate for later selection and acceptance.

This milestone does not record selection, does not record acceptance, and does
not promote runtime decks.

## Inputs

- `outputs/target_slice/m52_closeout_fifth_slice_runtime_readiness.json`
- `outputs/target_slice/m52_06_fifth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m52_03_fifth_slice_recipe_draft_model.json`

## Outputs

- `outputs/target_slice/m53_01_fifth_slice_human_repair_review_packet.json`
- `outputs/target_slice/m53_01_fifth_slice_human_repair_review_packet.md`
- `outputs/target_slice/m53_01_fifth_slice_human_repair_review_packet.csv`

## Review Rules

- Include all repair candidates that are ready for human repair review.
- Preserve pair context from the recipe draft.
- Preserve grade-profile additions/removals from M52-06.
- Provide decision options only; do not choose an option automatically.
- Mark `ready_for_m53_02=true` only if `M52-closeout` allows human selection
  review and every review item is ready.

## Runtime Boundary

This milestone must not:

- modify M52 recipe drafts
- record human selection
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fifth_slice_human_repair_review_packet.py
python -m unittest tests.test_fifth_slice_human_repair_review_packet
python -m unittest discover -s tests -p "test_*.py"
```
