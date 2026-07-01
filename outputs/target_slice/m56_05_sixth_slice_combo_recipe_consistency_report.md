# M56-05 Sixth-Slice Combo-To-Recipe Consistency

## Summary

- Consistency checks: `12`
- Pair cards present: `12`
- Missing pair-card checks: `0`
- Pair manual-review dependencies: `0`
- Recipe manual-review dependencies: `12`
- G Zone deferred checks: `12`
- Promotion allowed: `0`
- Runtime-ready consistent: `0`
- Ready for M56-06: `True`

## Status Counts

- `blocked_by_manual_review`: `12`

## Checks

- `m56_recipe_001` edge=`G-BT12-062TH->G-BT12-066TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_002` edge=`G-BT10-026TH->G-BT09-058TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_003` edge=`G-BT10-026TH->G-BT09-060TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_004` edge=`G-BT10-026TH->G-BT09-062TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_005` edge=`G-BT10-026TH->G-BT12-031TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_006` edge=`G-BT10-026TH->G-BT12-065TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_007` edge=`G-BT10-026TH->G-TD10-010TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_008` edge=`G-BT12-062TH->G-BT09-058TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_009` edge=`G-BT12-062TH->G-BT09-060TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_010` edge=`G-BT12-062TH->G-BT09-062TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_011` edge=`G-BT12-062TH->G-BT12-065TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m56_recipe_012` edge=`G-BT12-062TH->G-TD10-010TH` present=`True` status=`blocked_by_manual_review` missing=``

## Policy

- Offline consistency check only.
- Promotion remains blocked unless validation passes and review blockers clear.
- G Zone deferred remains visible as a review dependency.
- No saved-deck injection, UI publication, runtime deck creation, or bot integration.

## Next

`M56-06`: Sixth-slice blocker repair candidates.
