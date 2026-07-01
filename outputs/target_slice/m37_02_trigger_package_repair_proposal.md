# M37-02 Trigger Package Repair Proposal

## Summary

- Recipe: `recipe_003`
- Packages simulated: `5`
- Packages resolving trigger blockers: `5`
- Recommended package: `m37_01_pkg_001` / `balanced_classic`
- Runtime promotion allowed: `False`
- Ready for M37-03: `True`

## Recommended Repair

- Reason: `highest_scoring_complete_candidate_that_resolves_trigger_blockers`
- Final trigger counts: `{'Critical': 4, 'Draw': 4, 'Heal': 4, 'Stand': 4}`
- Resolved blockers: `['main_deck_size_mismatch', 'trigger_count_mismatch', 'unfilled_slots']`
- Remaining review issues: `['grade_profile_review', 'human_acceptance_pending']`

Quantity delta:

- `4x` `BT04-077TH` (Critical, BT04)
- `4x` `BT02-073TH` (Draw, BT02)
- `4x` `BT01-065TH` (Heal, BT01)

## Package Simulations

- `m37_01_pkg_001` `balanced_classic` status=`trigger_blockers_resolved_pending_review` resolved=['main_deck_size_mismatch', 'trigger_count_mismatch', 'unfilled_slots'] remaining=[] final={'Critical': 4, 'Draw': 4, 'Heal': 4, 'Stand': 4}
- `m37_01_pkg_002` `eb04_local_balanced` status=`trigger_blockers_resolved_pending_review` resolved=['main_deck_size_mismatch', 'trigger_count_mismatch', 'unfilled_slots'] remaining=[] final={'Critical': 4, 'Draw': 4, 'Heal': 4, 'Stand': 4}
- `m37_01_pkg_003` `critical_pressure` status=`trigger_blockers_resolved_pending_review` resolved=['main_deck_size_mismatch', 'trigger_count_mismatch', 'unfilled_slots'] remaining=[] final={'Critical': 8, 'Heal': 4, 'Stand': 4}
- `m37_01_pkg_004` `stand_pressure` status=`trigger_blockers_resolved_pending_review` resolved=['main_deck_size_mismatch', 'trigger_count_mismatch', 'unfilled_slots'] remaining=[] final={'Critical': 4, 'Heal': 4, 'Stand': 8}
- `m37_01_pkg_005` `draw_stand_guarded` status=`trigger_blockers_resolved_pending_review` resolved=['main_deck_size_mismatch', 'trigger_count_mismatch', 'unfilled_slots'] remaining=[] final={'Draw': 4, 'Heal': 4, 'Stand': 8}

## Policy

- This is a repair proposal only.
- The recipe draft is not modified.
- Runtime deck promotion remains disabled.
- Human review is required before accepting the quantity delta.

## Next

`M37-03`: Rejected-line support-gap triage.
