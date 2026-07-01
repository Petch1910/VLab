# M48-01 Fourth-Slice Fixture Scaffold

## Selected Target

- Group: `รอยัล พาลาดิน`
- Era preset: `g_series_first`
- Applied expansion: `g_era_heal_expansion`

## Scaffold

- Policy level: `fourth_slice_g_era_expanded_scope_fixture_scaffold_not_full_official_legality`
- Main deck exact: `50`
- Trigger target: `16`
- Required trigger types: `['Critical', 'Draw', 'Heal', 'Stand']`
- Recommended trigger profile: `{'Critical': 4, 'Draw': 4, 'Heal': 4, 'Stand': 4}`
- Required setup grades: `[0, 1, 2, 3]`
- Preferred grade profile: `{'0': 17, '1': 14, '2': 11, '3': 8}`
- Copy limit source: `runtime SQLite cards.deck_limit`

## Evidence

- Source cards: `190`
- Series counts: `{'G-BT01': 14, 'G-BT02': 6, 'G-BT04': 8, 'G-BT06': 15, 'G-BT08': 12, 'G-BT14': 13, 'G-CHB01': 26, 'G-CMB01': 22, 'G-FC04': 3, 'G-LD03': 18, 'G-MT01': 19, 'G-TD02': 16, 'G-TD11': 18}`
- Grade counts: `{'0': 47, '1': 48, '2': 46, '3': 27, '4': 22}`
- Trigger counts: `{'': 156, 'Critical': 15, 'Draw': 6, 'Heal': 7, 'Stand': 6}`
- Candidate edges: `785`
- Manual-review cards: `15`

## Summary

- Scaffold ready: `True`
- Blocking issues: `0`
- Ready for M48-02: `True`

## Boundary

- `scaffold_artifact_only`: `True`
- `does_not_create_deck`: `True`
- `does_not_create_recipe_draft`: `True`
- `does_not_create_runtime_fixture`: `True`
- `does_not_mutate_runtime_pack`: `True`
- `does_not_publish_to_ui`: `True`
- `does_not_publish_to_bot`: `True`
- `GameState_mutation`: `False`

## Next

`M48-02`: Fourth-slice review packet.
