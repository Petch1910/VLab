# M52-01 Fifth-Slice Fixture Scaffold

## Selected Target

- Group: `โกลด์ พาลาดิน`
- Era preset: `link_joker_legion_mate`

## Scaffold

- Policy level: `fifth_slice_link_joker_legion_mate_fixture_scaffold_not_full_official_legality`
- Main deck exact: `50`
- Trigger target: `16`
- Required trigger types: `['Critical', 'Draw', 'Heal', 'Stand']`
- Recommended trigger profile: `{'Critical': 4, 'Draw': 4, 'Heal': 4, 'Stand': 4}`
- Required setup grades: `[0, 1, 2, 3]`
- Preferred grade profile: `{'0': 17, '1': 14, '2': 11, '3': 8}`
- Copy limit source: `runtime SQLite cards.deck_limit`

## Evidence

- Source cards: `106`
- Series counts: `{'BT10': 13, 'BT12': 11, 'BT14': 20, 'BT15': 16, 'BT17': 17, 'TD08': 12, 'TD16': 17}`
- Grade counts: `{'0': 20, '1': 29, '2': 30, '3': 27}`
- Trigger counts: `{'': 97, 'Critical': 3, 'Draw': 1, 'Heal': 1, 'Stand': 4}`
- Candidate edges: `142`
- Manual-review cards: `4`

## Summary

- Scaffold ready: `True`
- Blocking issues: `0`
- Ready for M52-02: `True`

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

`M52-02`: Fifth-slice review packet.
