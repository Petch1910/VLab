# M44-01 Third-Slice Fixture Scaffold

## Selected Target

- Group: `เบอร์มิวด้า ไทรแองเกิล`
- Era preset: `link_joker_legion_mate`

## Scaffold

- Policy level: `third_slice_source_backed_fixture_scaffold_not_full_official_legality`
- Main deck exact: `50`
- Trigger target: `16`
- Required trigger types: `['Critical', 'Draw', 'Heal', 'Stand']`
- Recommended trigger profile: `{'Critical': 4, 'Draw': 4, 'Heal': 4, 'Stand': 4}`
- Required setup grades: `[0, 1, 2, 3]`
- Preferred grade profile: `{'0': 17, '1': 14, '2': 11, '3': 8}`
- Copy limit source: `runtime SQLite cards.deck_limit`

## Evidence

- Source cards: `127`
- Series counts: `{'EB06': 41, 'EB10': 86}`
- Grade counts: `{'0': 31, '1': 35, '2': 30, '3': 31}`
- Trigger counts: `{'': 106, 'Critical': 6, 'Draw': 6, 'Heal': 3, 'Stand': 6}`
- Candidate edges: `109`
- Manual-review cards: `61`

## Summary

- Scaffold ready: `True`
- Blocking issues: `0`
- Ready for M44-02: `True`

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

`M44-02`: Third-slice review packet.
