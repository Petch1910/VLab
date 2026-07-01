# M35-E2 Second Slice Fixture Readiness

## Selected Target

- Slice: `Classic Core`
- Era preset: `classic_part1`
- Group: `โอราเคิล ทิงค์ แทงค์`
- M34-03 rank: `10`

## Readiness

- All fixture expectations met: `True`
- Classic Core policy reusable: `True`
- Semantic scale-out ready: `True`
- Runtime/bot promotion allowed: `False`

## Fixtures

- `classic_core_second_slice_valid_minimal` expected `pass` -> `accepted`; reasons: `none`
- `classic_core_second_slice_short_main` expected `fail` -> `rejected`; reasons: `main_count:49!=50`
- `classic_core_second_slice_bad_trigger_count` expected `fail` -> `rejected`; reasons: `trigger_count:15!=16`
- `classic_core_second_slice_missing_grade_3_setup` expected `fail` -> `rejected`; reasons: `missing_setup_grade:3`
- `classic_core_second_slice_copy_limit_exceeded` expected `fail` -> `rejected`; reasons: `copy_limit_exceeded:BT03-038TH:5>4`
- `classic_core_second_slice_identity_mismatch` expected `fail` -> `rejected`; reasons: `identity_mismatch:BT01-003TH`

## Decision

`M35-E2` reuses the Classic Core policy for the second slice because the
selected group uses the same era preset and all fixture expectations pass.

## Next

`M35-E3`: generalize semantic/compatibility tooling around a selected-report
input contract before scaling more groups.
