# Third-Slice Human Repair Review Packet Spec

Milestone: `M45-01`

## Purpose

`M45-01` exports the `M44-06` repair candidates for human/team review.

This packet is not acceptance. It only prepares the review data needed before
`M45-02` can record an explicit decision.

## Inputs

- `outputs/target_slice/m44_closeout_third_slice_runtime_readiness.json`
- `outputs/target_slice/m44_06_third_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m44_03_third_slice_recipe_draft_model.json`

## Outputs

- `outputs/target_slice/m45_01_third_slice_human_repair_review_packet.json`
- `outputs/target_slice/m45_01_third_slice_human_repair_review_packet.md`
- `outputs/target_slice/m45_01_third_slice_human_repair_review_packet.csv`

## Review Rules

- Include every M44-06 repair item.
- Include the candidate edge pair that anchors each recipe.
- Include complete manual repair package evidence.
- Include complete grade-profile repair package evidence.
- Include explicit decision options.
- Keep runtime promotion disabled.

## Runtime Boundary

This milestone must not:

- record human acceptance
- modify M44-03 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_third_slice_human_repair_review_packet.py
python -m unittest tests.test_third_slice_human_repair_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M45-01` is done when:

- all `25` M44-06 repair items are exported
- all review items require a human decision
- packet scope says acceptance is not recorded
- runtime promotion remains disabled
- `ready_for_m45_02=true`
