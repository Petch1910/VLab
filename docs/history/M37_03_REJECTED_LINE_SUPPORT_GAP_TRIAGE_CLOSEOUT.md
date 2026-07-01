# M37-03 Rejected-Line Support-Gap Triage Closeout

## Summary

`M37-03` classified the `24` rejected combo lines from M36 into support-gap
groups for M37-04 manual semantic mapping.

The output is multi-label because many rejected lines have more than one
blocking concern. No rejected line was accepted or promoted.

## Results

- Rejected lines: `24`
- Triage items: `24`
- Gap groups: `5`
- Multi-label lines: `19`
- Manual mapping backlog items: `5`
- Runtime promotion allowed: `false`
- Ready for M37-04: `true`

## Gap Counts

- `resource_pressure_gap`: `9`
- `zone_access_gap`: `15`
- `broad_timing_review`: `10`
- `detector_gap_not_found_manual_review`: `10`
- `no_resource_dependency_manual_review`: `5`

## Priority Counts

- `P1_resource_and_zone_gap`: `7`
- `P2_single_structural_gap`: `10`
- `P3_manual_confirmation`: `7`

## Files

- Spec: `docs/specs/cards_and_decks/REJECTED_LINE_SUPPORT_GAP_TRIAGE_SPEC.md`
- Tool: `tools/deck/build_rejected_line_support_gap_triage.py`
- Tests: `tests/test_rejected_line_support_gap_triage.py`
- Output: `outputs/target_slice/m37_03_rejected_line_support_gap_triage.json`
- Output: `outputs/target_slice/m37_03_rejected_line_support_gap_triage.md`

## Verification

```powershell
python tools\deck\build_rejected_line_support_gap_triage.py
python -m unittest tests.test_rejected_line_support_gap_triage
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 311 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M37-04`: Manual semantic mapping candidates.

