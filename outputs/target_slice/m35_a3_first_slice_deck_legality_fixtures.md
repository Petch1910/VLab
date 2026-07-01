# M35-A3 First Slice Deck Legality Fixtures

## Selected Target

- Slice: `Classic Core`
- Era preset: `classic_part1`
- Group: `โนว่า เกรปเปอร์`

## Fixtures

- `classic_core_selected_group_valid_minimal` expected `pass` -> `accepted`; reasons: `none`
- `classic_core_selected_group_short_main` expected `fail` -> `rejected`; reasons: `main_count:49!=50`
- `classic_core_selected_group_bad_trigger_count` expected `fail` -> `rejected`; reasons: `trigger_count:15!=16`
- `classic_core_selected_group_missing_grade_3_setup` expected `fail` -> `rejected`; reasons: `missing_setup_grade:3`
- `classic_core_selected_group_copy_limit_exceeded` expected `fail` -> `rejected`; reasons: `copy_limit_exceeded:BT01-063TH:5>4`
- `classic_core_selected_group_identity_mismatch` expected `fail` -> `rejected`; reasons: `identity_mismatch:BT01-003TH`

## Deferred Limits

- Official heal trigger maximum is noted but not broadly enforced in this fixture slice.
- Runtime per-card `deck_limit` is enforced for the selected fixtures.

## Next

`M35-A4`: Refresh first-slice feasibility with legality-readiness and missing-rule gates.
