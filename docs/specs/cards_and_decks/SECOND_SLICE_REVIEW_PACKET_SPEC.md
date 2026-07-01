# Second-Slice Review Packet Spec

Milestone: `M40-01`

## Purpose

`M40-01` creates the first review packet for the second selected slice:
`Classic Core / Oracle Think Tank`.

The packet gathers fixture notes, manual-review cards, and candidate
compatibility edges before any second-slice recipe drafts are created. It is a
review artifact only.

## Inputs

- `outputs/target_slice/m35_e1_second_target_slice_report.json`
- `outputs/target_slice/m35_e2_second_slice_fixture_readiness.json`
- `outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.json`
- `outputs/target_slice/m39_04_second_slice_recipe_scale_decision.json`

The tool also rebuilds the selected-slice semantic/compatibility pipeline in
memory so the packet contains actual manual-review cards and full candidate
edge rows rather than only the M35-E3 summary.

## Outputs

- `outputs/target_slice/m40_01_second_slice_review_packet.json`
- `outputs/target_slice/m40_01_second_slice_review_packet.md`
- `outputs/target_slice/m40_01_second_slice_review_packet.csv`

## Required Contents

- fixture note items from second-slice readiness fixtures
- manual-review card items from the rebuilt semantic queue
- candidate compatibility edge review items from the rebuilt compatibility
  output
- blocked-until reasons for every item
- recommended next action for every item
- evidence that M39-04 still blocks saved deck, UI, runtime, and bot promotion

## Runtime Boundary

This milestone must not:

- create a deck recipe draft
- inject a saved deck
- publish a deck to UI deck lists
- promote a runtime fixture
- enable bot/playbook integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_second_slice_review_packet.py
python -m unittest tests.test_second_slice_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M40-01` is done when the review packet reports:

- `6` fixture note items
- `7` manual-review card items
- `259` candidate edge items
- `272` total review items
- `ready_for_m40_02=true`

