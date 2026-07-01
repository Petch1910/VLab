# M56-01 Sixth-Slice Fixture Scaffold

## Selected Target

- Group: `ชาโดว์ พาลาดิน`
- Era preset: `g_next_z`

## Scaffold

- Policy level: `sixth_slice_g_next_z_fixture_scaffold_not_full_official_legality`
- Main deck exact: `50`
- Trigger target: `16`
- Required trigger types: `['Critical', 'Draw', 'Heal', 'Stand']`
- Recommended trigger profile: `{'Critical': 4, 'Draw': 4, 'Heal': 4, 'Stand': 4}`
- Required setup grades: `[0, 1, 2, 3]`
- Preferred grade profile: `{'0': 17, '1': 14, '2': 11, '3': 8}`
- Copy limit source: `runtime SQLite cards.deck_limit`

## Mechanic Scope

- `format_family`: `G NEXT / G Z Stride era`
- `runtime_stride_enabled`: `False`
- `g_zone_recipe_validation_deferred`: `True`
- `grade4_cards_present`: `True`
- `grade4_cards_advisory_only_until_g_zone_support`: `True`
- `stride_or_generation_break_text_requires_manual_review`: `True`
- `ritual_or_retire_text_requires_manual_review`: `True`
- `imaginary_gift_enabled`: `False`
- `ride_deck_enabled`: `False`
- `over_trigger_enabled`: `False`
- `front_trigger_enabled`: `False`
- `order_cards_enabled`: `False`

## Evidence

- Source cards: `77`
- Series counts: `{'G-BT09': 19, 'G-BT10': 9, 'G-BT12': 17, 'G-BT14': 14, 'G-TD10': 18}`
- Grade counts: `{'0': 19, '1': 20, '2': 16, '3': 11, '4': 11}`
- Trigger counts: `{'': 65, 'Critical': 4, 'Draw': 4, 'Heal': 2, 'Stand': 2}`
- Candidate edges: `70`
- Manual-review cards: `11`

## Summary

- Scaffold ready: `True`
- Blocking issues: `0`
- Ready for M56-02: `True`

## Boundary

- `scaffold_artifact_only`: `True`
- `does_not_create_deck`: `True`
- `does_not_create_recipe_draft`: `True`
- `does_not_create_runtime_fixture`: `True`
- `does_not_mutate_runtime_pack`: `True`
- `does_not_publish_to_ui`: `True`
- `does_not_publish_to_bot`: `True`
- `g_zone_runtime_enabled`: `False`
- `stride_runtime_enabled`: `False`
- `GameState_mutation`: `False`

## Next

`M56-02`: Sixth-slice review packet.
