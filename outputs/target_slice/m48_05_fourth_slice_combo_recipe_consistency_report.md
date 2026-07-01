# M48-05 Fourth-Slice Combo-To-Recipe Consistency

## Summary

- Consistency checks: `25`
- Pair cards present: `25`
- Missing pair-card checks: `0`
- Pair manual-review dependencies: `0`
- Recipe manual-review dependencies: `25`
- G Zone deferred checks: `25`
- Promotion allowed: `0`
- Runtime-ready consistent: `0`
- Ready for M48-06: `True`

## Status Counts

- `blocked_by_manual_review`: `25`

## Checks

- `m48_recipe_001` edge=`G-CMB01-003TH->G-TD02-004TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_002` edge=`G-CMB01-003TH->G-TD11-002TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_003` edge=`G-CMB01-028TH->G-BT01-045TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_004` edge=`G-BT14-006TH->G-BT01-045TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_005` edge=`G-CHB01-004TH->G-BT02-044TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_006` edge=`G-CMB01-003TH->G-BT01-045TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_007` edge=`G-BT08-025TH->G-BT01-048TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_008` edge=`G-BT08-025TH->G-BT06-024TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_009` edge=`G-BT08-025TH->G-BT08-048TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_010` edge=`G-BT08-025TH->G-CMB01-013TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_011` edge=`G-BT08-025TH->G-CMB01-014TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_012` edge=`G-BT08-025TH->G-TD02-006TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_013` edge=`G-BT08-025TH->G-TD02-012TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_014` edge=`G-LD03-011TH->G-BT01-045TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_015` edge=`G-BT01-010TH->G-BT02-044TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_016` edge=`G-BT01-011TH->G-BT01-046TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_017` edge=`G-BT01-011TH->G-BT06-003TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_018` edge=`G-BT01-011TH->G-BT06-023TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_019` edge=`G-BT01-011TH->G-BT06-024TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_020` edge=`G-BT01-011TH->G-BT06-045TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_021` edge=`G-BT01-011TH->G-BT08-048TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_022` edge=`G-BT01-011TH->G-BT14-015TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_023` edge=`G-BT01-011TH->G-CHB01-006TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_024` edge=`G-BT01-011TH->G-CHB01-047TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m48_recipe_025` edge=`G-BT01-011TH->G-CMB01-004TH` present=`True` status=`blocked_by_manual_review` missing=``

## Policy

- Offline consistency check only.
- Promotion remains blocked unless validation passes and review blockers clear.
- G Zone deferred remains visible as a review dependency.
- No saved-deck injection, UI publication, runtime deck creation, or bot integration.

## Next

`M48-06`: Fourth-slice blocker repair candidates.
