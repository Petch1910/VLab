# First Slice Feasibility Refresh Spec

Milestone: `M35-A4`

## Purpose

Refresh the first selected slice after `M35-A2` target selection and `M35-A3`
minimal legality fixtures. This decides whether Phase A is ready to move into
Phase B semantic tagging.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_a2_first_target_slice_report.json
outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.json
```

## Readiness Gates

The refresh report must state:

- capacity ready: selected group has enough card capacity for the minimal
  deck policy
- taxonomy ready: no required Classic Core taxonomy terms are missing
- legality fixture ready: minimal pass/fail fixtures match expectations
- blocking gaps for Phase B
- deferred limits before any full official deck legality claim

## Non-Goals

- no semantic ability tagging yet
- no combo compatibility graph yet
- no bot/playbook promotion
- no Unity/runtime mutation
- no full official legality claim

## Outputs

```text
outputs/target_slice/m35_a4_first_slice_feasibility_refresh.json
outputs/target_slice/m35_a4_first_slice_feasibility_refresh.md
```

## Verification

```powershell
python tools\deck\refresh_first_slice_feasibility.py
python -m unittest tests.test_first_slice_feasibility_refresh
python -m unittest discover -s tests -p "test_*.py"
```
