# Second-Slice Human Repair Review Packet Spec

Milestone: `M41-01`

## Purpose

`M41-01` exports a human/team review packet for the Oracle Think Tank repair
candidates created in `M40-05`.

This packet is for review only. It does not record acceptance, mutate recipe
drafts, create runtime fixtures, publish saved decks, or enable bot/playbook
use.

## Inputs

- `outputs/target_slice/m40_closeout_second_slice_runtime_readiness.json`
- `outputs/target_slice/m40_05_second_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json`

## Outputs

- `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.json`
- `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.md`
- `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.csv`

## Packet Contents

Each review item must include:

- recipe id
- source candidate edge
- pair card ids and Thai names from the M40-02 draft model
- validation and consistency status
- manual-review card ids
- grade-profile repair package id
- compact additions/removals from the repair package
- grade counts after repair
- explicit decision options

The allowed reviewer options are:

- accept repair for validation rerun
- request different repair
- reject recipe runtime candidate

None of these options are recorded by `M41-01`; recording acceptance or
rejection is `M41-02`.

## Runtime Boundary

This milestone must not:

- record human acceptance
- modify M40-02 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_second_slice_human_repair_review_packet.py
python -m unittest tests.test_second_slice_human_repair_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M41-01` is done when:

- all `25` M40-05 repair candidates are represented
- all represented candidates remain non-runtime
- review policy states that the packet is not acceptance
- JSON, Markdown, and CSV artifacts are written
- project status docs point the active queue to `M41-02`
