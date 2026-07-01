# Fourth-Slice Human Repair and G Zone Review Packet Spec

Milestone: `M49-01`

## Purpose

`M49-01` exports the `M48-06` repair candidates for human/team review and
adds the G Zone / Stride decision context required by `M48-closeout`.

This packet is not acceptance and is not a G Zone boundary decision. It only
prepares review data for `M49-02`.

## Inputs

- `outputs/target_slice/m48_closeout_fourth_slice_runtime_readiness.json`
- `outputs/target_slice/m48_06_fourth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m48_03_fourth_slice_recipe_draft_model.json`

## Outputs

- `outputs/target_slice/m49_01_fourth_slice_human_repair_review_packet.json`
- `outputs/target_slice/m49_01_fourth_slice_human_repair_review_packet.md`
- `outputs/target_slice/m49_01_fourth_slice_human_repair_review_packet.csv`

## Review Rules

- Include every M48-06 repair item.
- Include the candidate edge pair that anchors each recipe.
- Include complete manual repair package evidence.
- Include grade-profile repair package evidence or explicit `not needed`
  status.
- Include G Zone deferred package evidence for every item.
- Include explicit human decision options.
- Include explicit G Zone decision options.
- Keep runtime promotion disabled.

## Runtime Boundary

This milestone must not:

- record human acceptance
- record a G Zone boundary decision
- modify M48-03 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fourth_slice_human_repair_review_packet.py
python -m unittest tests.test_fourth_slice_human_repair_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M49-01` is done when:

- all `25` M48-06 repair items are exported
- all review items require human and G Zone decisions
- packet scope says acceptance and G Zone decisions are not recorded
- runtime promotion remains disabled
- `ready_for_m49_02=true`
