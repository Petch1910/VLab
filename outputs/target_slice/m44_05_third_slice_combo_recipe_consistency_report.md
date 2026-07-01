# M44-05 Third-Slice Combo-To-Recipe Consistency

## Summary

- Consistency checks: `25`
- Pair cards present: `25`
- Missing pair-card checks: `0`
- Pair manual-review dependencies: `0`
- Recipe manual-review dependencies: `25`
- Promotion allowed: `0`
- Runtime-ready consistent: `0`
- Ready for M44-06: `True`

## Status Counts

- `blocked_by_manual_review`: `25`

## Checks

- `m44_recipe_001` edge=`EB10-007TH-B->EB06-023TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_002` edge=`EB10-007TH-B->EB06-S06TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_003` edge=`EB10-007TH-W->EB06-023TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_004` edge=`EB10-007TH-W->EB06-S06TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_005` edge=`EB10-S07TH-B->EB06-023TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_006` edge=`EB10-S07TH-B->EB06-S06TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_007` edge=`EB10-S07TH-W->EB06-023TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_008` edge=`EB10-S07TH-W->EB06-S06TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_009` edge=`EB10-026TH-B->EB06-020TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_010` edge=`EB10-026TH-B->EB10-012TH-B` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_011` edge=`EB10-026TH-B->EB10-012TH-W` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_012` edge=`EB10-026TH-W->EB06-020TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_013` edge=`EB10-026TH-W->EB10-012TH-B` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_014` edge=`EB10-026TH-W->EB10-012TH-W` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_015` edge=`EB06-017TH->EB10-012TH-B` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_016` edge=`EB06-017TH->EB10-012TH-W` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_017` edge=`EB06-022TH->EB10-012TH-B` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_018` edge=`EB06-022TH->EB10-012TH-W` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_019` edge=`EB06-023TH->EB10-012TH-B` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_020` edge=`EB06-023TH->EB10-012TH-W` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_021` edge=`EB06-S06TH->EB10-012TH-B` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_022` edge=`EB06-S06TH->EB10-012TH-W` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_023` edge=`EB10-007TH-B->EB06-008TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_024` edge=`EB10-007TH-B->EB06-011TH` present=`True` status=`blocked_by_manual_review` missing=``
- `m44_recipe_025` edge=`EB10-007TH-B->EB06-013TH` present=`True` status=`blocked_by_manual_review` missing=``

## Policy

- Offline consistency check only.
- Promotion remains blocked unless validation passes and review blockers clear.
- No saved-deck injection, UI publication, runtime deck creation, or bot integration.

## Next

`M44-06`: Third-slice blocker repair candidates.
