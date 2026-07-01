# M37-04 Manual Semantic Mapping Candidates Closeout

## Summary

`M37-04` converted the M37-03 support-gap triage backlog into reviewable manual
semantic mapping candidates.

The candidates are non-executable work items. No rejected line was accepted, no
ability schema changed, and runtime promotion remains disabled.

## Results

- Mapping candidates: `5`
- Structural candidates: `2`
- Timing candidates: `1`
- Review-only candidates: `2`
- Line mapping links: `49`
- Runtime promotion allowed: `false`
- Ready for M37-05: `true`

## Candidate Types

- `resource_requirement_provider_mapping`
- `zone_target_requirement_provider_mapping`
- `timing_window_specificity_mapping`
- `human_acceptance_without_new_mapping`
- `false_dependency_or_acceptance_review`

## Files

- Spec: `docs/specs/cards_and_decks/MANUAL_SEMANTIC_MAPPING_CANDIDATES_SPEC.md`
- Tool: `tools/deck/build_manual_semantic_mapping_candidates.py`
- Tests: `tests/test_manual_semantic_mapping_candidates.py`
- Output: `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.json`
- Output: `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.md`

## Verification

```powershell
python tools\deck\build_manual_semantic_mapping_candidates.py
python -m unittest tests.test_manual_semantic_mapping_candidates
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 320 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M37-05`: Revised recipe validation rerun.

