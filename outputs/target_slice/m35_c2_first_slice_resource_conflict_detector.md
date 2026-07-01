# M35-C2 Resource Conflict Detector

## Selected Target

- Slice: `Classic Core`
- Era preset: `classic_part1`
- Group: `โนว่า เกรปเปอร์`

## Summary

- Nodes: `112`
- Source graph edges: `3919`
- Resource-relevant edges: `2826`
- Manual-review resource edges: `224`
- Missing recovery resource types: `0`
- Ready for M35-C3: `True`

## Verdict Counts

- `mixed_support_and_shared_pressure`: `12`
- `resource_support`: `357`
- `shared_resource_pressure`: `286`
- `source_resource_profile_only`: `1293`
- `target_resource_need_not_supported_by_source`: `878`

## Resource Demand Counts

- `counter_blast` demand: `27`
- `soul` demand: `5`

## Resource Provider Counts

- `counter_blast` provider: `12`
- `hand_cards` provider: `15`
- `soul` provider: `13`

## Top Resource Findings

- `BT01-029TH->BT01-028TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT01-029TH->EB04-016TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT01-029TH->TD03-003TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT02-008TH->BT01-028TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT02-008TH->EB04-016TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT02-008TH->TD03-003TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT04-008TH->BT01-028TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT04-008TH->EB04-016TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT04-008TH->TD03-003TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT05-072TH->BT01-028TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT05-072TH->EB04-016TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`
- `BT05-072TH->TD03-003TH` verdict=`shared_resource_pressure` risk=`9` supported=`none`

## Scope

- Advisory resource detector only.
- No exact resource amount claim until structured cost amounts exist.
- No timing compatibility verdict yet.
- No zone/target compatibility verdict yet.
- No deck skeleton or bot playbook promotion.
