# M37-04 Manual Semantic Mapping Candidates

## Summary

- Mapping candidates: `5`
- Structural candidates: `2`
- Timing candidates: `1`
- Review-only candidates: `2`
- Line mapping links: `49`
- Runtime promotion allowed: `False`
- Ready for M37-05: `True`

## Candidates

- `m37_04_map_01_broad_timing_review` `timing_window_specificity_mapping` kind=`timing_semantic_mapping` lines=`10` target=`timing_window_tags`
- `m37_04_map_02_detector_gap_not_found_manual_review` `human_acceptance_without_new_mapping` kind=`review_only_gate` lines=`10` target=`human_review_status`
- `m37_04_map_03_no_resource_dependency_manual_review` `false_dependency_or_acceptance_review` kind=`review_only_gate` lines=`5` target=`dependency_review_status`
- `m37_04_map_04_resource_pressure_gap` `resource_requirement_provider_mapping` kind=`structural_semantic_mapping` lines=`9` target=`resource_requirements_and_providers`
- `m37_04_map_05_zone_access_gap` `zone_target_requirement_provider_mapping` kind=`structural_semantic_mapping` lines=`15` target=`zone_target_requirements_and_providers`

## Policy

- Candidates are review work items, not executable schema changes.
- Rejected combo lines remain rejected.
- Runtime deck and bot/playbook promotion remain disabled.

## Next

`M37-05`: Revised recipe validation rerun.
