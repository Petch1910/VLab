# M37-03 Rejected-Line Support-Gap Triage

## Summary

- Rejected lines: `24`
- Triage items: `24`
- Gap groups: `5`
- Multi-label lines: `19`
- Runtime promotion allowed: `False`
- Ready for M37-04: `True`

## Gap Groups

- `broad_timing_review`: `10` lines -> Tighten timing-window mapping or keep manual review.
- `detector_gap_not_found_manual_review`: `10` lines -> Confirm whether line can be accepted after human review.
- `no_resource_dependency_manual_review`: `5` lines -> Confirm this is not a false dependency before accepting the line.
- `resource_pressure_gap`: `9` lines -> Map resource requirements/providers or keep line rejected.
- `zone_access_gap`: `15` lines -> Map zone/target requirements/providers or require normal setup note.

## Priority Counts

- `P1_resource_and_zone_gap`: `7`
- `P2_single_structural_gap`: `10`
- `P3_manual_confirmation`: `7`

## Manual Mapping Backlog

- `m37_04_01_broad_timing_review` `timing_window_specificity_mapping` lines=`10`
- `m37_04_02_detector_gap_not_found_manual_review` `human_acceptance_without_new_mapping` lines=`10`
- `m37_04_03_no_resource_dependency_manual_review` `false_dependency_or_acceptance_review` lines=`5`
- `m37_04_04_resource_pressure_gap` `resource_requirement_provider_mapping` lines=`9`
- `m37_04_05_zone_access_gap` `zone_target_requirement_provider_mapping` lines=`15`

## Policy

- Triage is multi-label and advisory.
- No rejected combo line is promoted by this report.
- Manual semantic mapping candidates are handled in M37-04.

## Next

`M37-04`: Manual semantic mapping candidates.
