# M41-03 Second-Slice Repaired Recipe Validation Rerun

## Summary

- Recipe: `m40_recipe_001`
- Validation status: `invalid_repaired_recipe`
- Runtime ready: `False`
- Promotion allowed: `False`
- Blocking issues: `1`
- Main deck count: `50`
- Trigger count: `2`
- Grade counts: `{'0': 17, '1': 14, '2': 11, '3': 8}`
- Manual-review overlap cleared: `True`
- Ready for M41-04: `False`
- Ready for repair loop: `True`

## Issues

- `blocker` `trigger_count_mismatch`: Classic trigger count must be 16 before fixture promotion. `{'expected': 16, 'actual': 2, 'by_trigger': {'Draw': 2}}`

## Policy

- Offline validation only.
- No runtime fixture, saved deck, UI deck list, bot playbook, or GameState mutation.
- Failed validation routes to a repair loop before any promotion gate.

## Next

`M41-repair`: Second-slice trigger/profile repair loop.
