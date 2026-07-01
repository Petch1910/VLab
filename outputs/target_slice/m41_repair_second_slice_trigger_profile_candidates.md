# M41 Repair Second-Slice Trigger/Profile Repair Candidates

## Summary

- Trigger blocker present: `True`
- Candidate packages: `3`
- Complete candidates: `3`
- Ready for human review: `3`
- Runtime promotion allowed: `False`
- Ready for repair acceptance: `True`

## Candidates

### `m41_repair_pkg_001`

- Profile: `balanced_classic_trigger_restore`
- Add rows: `4`
- Remove rows: `5`
- Counts after: `{'main_deck_count': 50, 'trigger_count': 16, 'trigger_counts': {'Critical': 4, 'Draw': 4, 'Heal': 4, 'Stand': 4}, 'grade_counts': {'0': 17, '1': 14, '2': 11, '3': 8}}`
- Complete candidate: `True`

### `m41_repair_pkg_002`

- Profile: `critical_pressure_trigger_restore`
- Add rows: `4`
- Remove rows: `5`
- Counts after: `{'main_deck_count': 50, 'trigger_count': 16, 'trigger_counts': {'Critical': 8, 'Draw': 4, 'Heal': 4}, 'grade_counts': {'0': 17, '1': 14, '2': 11, '3': 8}}`
- Complete candidate: `True`

### `m41_repair_pkg_003`

- Profile: `stand_pressure_trigger_restore`
- Add rows: `4`
- Remove rows: `5`
- Counts after: `{'main_deck_count': 50, 'trigger_count': 16, 'trigger_counts': {'Draw': 4, 'Heal': 4, 'Stand': 8}, 'grade_counts': {'0': 17, '1': 14, '2': 11, '3': 8}}`
- Complete candidate: `True`

## Policy

- Candidates are advisory only.
- Human acceptance is required before applying a trigger repair.
- No runtime fixture, saved deck, UI deck list, bot playbook, or GameState mutation.

## Next

`M41-repair-accept`: Second-slice trigger repair acceptance artifact.
