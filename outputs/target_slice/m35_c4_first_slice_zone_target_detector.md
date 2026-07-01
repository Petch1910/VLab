# M35-C4 Zone / Target Compatibility Detector

## Selected Target

- Slice: `Classic Core`
- Era preset: `classic_part1`
- Group: `โนว่า เกรปเปอร์`

## Summary

- Nodes: `112`
- Source graph edges: `3919`
- Zone-relevant edges: `3704`
- Manual-review zone edges: `331`
- Unsupported required zone types: `1`
- Ready for M35-C5: `True`

## Verdict Counts

- `missing_zone_support_in_slice`: `52`
- `mixed_zone_support_and_slot_pressure`: `339`
- `rear_guard_slot_pressure`: `436`
- `source_zone_profile_only`: `766`
- `target_zone_need_not_supported_by_source`: `1641`
- `vanguard_role_conflict`: `402`
- `zone_support`: `68`

## Zone Requirement Counts

- `damage_zone` requirement: `16`
- `drop_zone` requirement: `1`
- `rear_guard_circle` requirement: `43`
- `vanguard_circle` requirement: `28`

## Zone Provider Counts

- `damage_zone` provider: `12`
- `deck` provider: `13`
- `hand` provider: `15`
- `rear_guard_circle` provider: `8`
- `soul` provider: `13`

## Zone Findings To Review First

- `BT09-071TH->BT09-070TH` verdict=`vanguard_role_conflict` shared=`rear_guard_circle, vanguard_circle` supported=`rear_guard_circle`
- `BT09-071TH->BT09-073TH` verdict=`vanguard_role_conflict` shared=`rear_guard_circle, vanguard_circle` supported=`rear_guard_circle`
- `BT01-028TH->BT09-070TH` verdict=`vanguard_role_conflict` shared=`damage_zone, vanguard_circle` supported=`damage_zone`
- `BT01-028TH->BT09-073TH` verdict=`vanguard_role_conflict` shared=`damage_zone, vanguard_circle` supported=`damage_zone`
- `BT02-008TH->BT06-008TH` verdict=`vanguard_role_conflict` shared=`rear_guard_circle, vanguard_circle` supported=`none`
- `BT02-008TH->EB04-003TH` verdict=`vanguard_role_conflict` shared=`rear_guard_circle, vanguard_circle` supported=`none`
- `BT04-019TH->BT09-070TH` verdict=`vanguard_role_conflict` shared=`damage_zone, vanguard_circle` supported=`damage_zone`
- `BT04-019TH->BT09-073TH` verdict=`vanguard_role_conflict` shared=`damage_zone, vanguard_circle` supported=`damage_zone`
- `BT04-075TH->BT02-008TH` verdict=`vanguard_role_conflict` shared=`rear_guard_circle, vanguard_circle` supported=`none` review-required
- `BT04-075TH->BT06-008TH` verdict=`vanguard_role_conflict` shared=`rear_guard_circle, vanguard_circle` supported=`none` review-required
- `BT04-075TH->EB04-003TH` verdict=`vanguard_role_conflict` shared=`rear_guard_circle, vanguard_circle` supported=`none` review-required
- `BT06-008TH->BT02-008TH` verdict=`vanguard_role_conflict` shared=`rear_guard_circle, vanguard_circle` supported=`none`

## Scope

- Advisory zone/target detector only.
- Vanguard-circle conflicts are archetype-review signals, not final rejection.
- Rear-guard slot pressure is coarse, not exact board-capacity calculation.
- No deck skeleton or bot playbook promotion.
