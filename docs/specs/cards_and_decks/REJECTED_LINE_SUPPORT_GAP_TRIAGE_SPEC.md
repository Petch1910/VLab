# Rejected-Line Support-Gap Triage Spec

Milestone: `M37-03`

## Purpose

`M37-03` classifies the M36 rejected combo lines into support-gap groups so
M37-04 can create focused manual semantic mapping candidates.

This is a triage report only. It does not accept rejected lines, resolve card
semantics, mutate recipe drafts, or promote runtime playbooks.

## Inputs

- `outputs/target_slice/m36_01_first_slice_review_packet.json`
- `outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json`
- `outputs/target_slice/m37_02_trigger_package_repair_proposal.json`

## Outputs

- `outputs/target_slice/m37_03_rejected_line_support_gap_triage.json`
- `outputs/target_slice/m37_03_rejected_line_support_gap_triage.md`

## Triage Labels

- `resource_pressure_gap`
- `zone_access_gap`
- `broad_timing_review`
- `detector_gap_not_found_manual_review`
- `no_resource_dependency_manual_review`

Lines may have multiple labels. Multi-label output is required because many
rejected lines are blocked by both resource and zone assumptions.

## Runtime Boundary

This milestone must not:

- accept rejected combo lines
- mutate recipe drafts
- create runtime decks
- promote playbook hints
- enable bot integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_rejected_line_support_gap_triage.py
python -m unittest tests.test_rejected_line_support_gap_triage
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M37-03` is done when all rejected lines are classified, a manual mapping
backlog is emitted, runtime promotion remains disabled, and the report points to
`M37-04`.

