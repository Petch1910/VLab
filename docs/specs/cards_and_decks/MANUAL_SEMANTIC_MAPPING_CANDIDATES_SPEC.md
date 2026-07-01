# Manual Semantic Mapping Candidates Spec

Milestone: `M37-04`

## Purpose

`M37-04` turns the M37-03 support-gap triage backlog into reviewable manual
semantic mapping candidates.

These candidates are work items for future review. They are not executable
ability schema changes and must not accept rejected combo lines by themselves.

## Inputs

- `outputs/target_slice/m37_03_rejected_line_support_gap_triage.json`

## Outputs

- `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.json`
- `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.md`

## Candidate Types

- `resource_requirement_provider_mapping`
- `zone_target_requirement_provider_mapping`
- `timing_window_specificity_mapping`
- `human_acceptance_without_new_mapping`
- `false_dependency_or_acceptance_review`

## Runtime Boundary

This milestone must not:

- modify ability schema
- mutate recipe drafts
- create runtime decks
- accept rejected combo lines
- promote playbook hints
- enable bot integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_manual_semantic_mapping_candidates.py
python -m unittest tests.test_manual_semantic_mapping_candidates
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M37-04` is done when it emits reviewable mapping candidates for every M37-03
manual mapping backlog group, keeps all candidates non-executable, and points to
`M37-05`.

