# M40-04 Second-Slice Combo-To-Recipe Consistency

## Summary

- Consistency checks: `25`
- Pair cards present: `25`
- Missing pair-card checks: `0`
- Pair manual-review dependencies: `0`
- Recipe manual-review dependencies: `25`
- Promotion allowed: `0`
- Runtime-ready consistent: `0`
- Ready for M40-05: `True`

## Status Counts

- `blocked_by_manual_review`: `25`

## Checks

- `m40_recipe_001` edge=`BT01-006TH->BT02-033TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_002` edge=`BT01-006TH->TD04-011TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_003` edge=`EB05-001TH->BT02-033TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_004` edge=`EB05-001TH->TD04-011TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_005` edge=`BT03-007TH->BT02-033TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_006` edge=`BT03-007TH->BT02-066TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_007` edge=`BT03-007TH->BT09-063TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_008` edge=`BT03-007TH->BT09-066TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_009` edge=`BT03-007TH->BT09-067TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_010` edge=`BT03-007TH->EB05-026TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_011` edge=`BT03-007TH->EB05-027TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_012` edge=`BT03-007TH->TD04-011TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_013` edge=`EB05-003TH->BT09-066TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_014` edge=`BT03-068TH->BT02-033TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_015` edge=`BT03-068TH->TD04-011TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_016` edge=`BT03-070TH->BT02-033TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_017` edge=`BT03-070TH->TD04-011TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_018` edge=`BT09-065TH->BT02-033TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_019` edge=`BT09-065TH->TD04-011TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_020` edge=`EB05-003TH->BT02-033TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_021` edge=`EB05-003TH->TD04-011TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_022` edge=`BT03-037TH->BT02-066TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_023` edge=`BT03-037TH->BT09-063TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_024` edge=`BT03-037TH->BT09-067TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m40_recipe_025` edge=`BT03-037TH->EB05-026TH` present=`True` status=`blocked_by_manual_review` missing=``

## Policy

- Offline consistency check only.
- Promotion remains blocked unless validation passes and review blockers clear.
- No saved-deck injection, UI publication, runtime deck creation, or bot integration.

## Next

`M40-05`: Second-slice blocker repair candidates.
