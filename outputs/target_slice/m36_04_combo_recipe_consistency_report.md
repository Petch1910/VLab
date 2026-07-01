# M36-04 Combo-Line To Recipe Consistency

## Summary

- Combo lines checked: `25`
- Combo cards present: `25`
- Missing combo-card checks: `0`
- Manual-review dependency checks: `0`
- Promotion allowed: `0`
- Runtime-ready consistent: `0`
- Ready for M36-05: `True`

## Status Counts

- `blocked_by_review`: `24`
- `invalid_recipe`: `1`

## Checks

- `line_001` recipe=`recipe_001` present=`True` status=`blocked_by_review` missing=``
- `line_002` recipe=`recipe_002` present=`True` status=`blocked_by_review` missing=``
- `line_003` recipe=`recipe_003` present=`True` status=`invalid_recipe` missing=``
- `line_004` recipe=`recipe_004` present=`True` status=`blocked_by_review` missing=``
- `line_005` recipe=`recipe_005` present=`True` status=`blocked_by_review` missing=``
- `line_006` recipe=`recipe_006` present=`True` status=`blocked_by_review` missing=``
- `line_007` recipe=`recipe_007` present=`True` status=`blocked_by_review` missing=``
- `line_008` recipe=`recipe_008` present=`True` status=`blocked_by_review` missing=``
- `line_009` recipe=`recipe_009` present=`True` status=`blocked_by_review` missing=``
- `line_010` recipe=`recipe_010` present=`True` status=`blocked_by_review` missing=``
- `line_011` recipe=`recipe_011` present=`True` status=`blocked_by_review` missing=``
- `line_012` recipe=`recipe_012` present=`True` status=`blocked_by_review` missing=``
- `line_013` recipe=`recipe_013` present=`True` status=`blocked_by_review` missing=``
- `line_014` recipe=`recipe_014` present=`True` status=`blocked_by_review` missing=``
- `line_015` recipe=`recipe_015` present=`True` status=`blocked_by_review` missing=``
- `line_016` recipe=`recipe_016` present=`True` status=`blocked_by_review` missing=``
- `line_017` recipe=`recipe_017` present=`True` status=`blocked_by_review` missing=``
- `line_018` recipe=`recipe_018` present=`True` status=`blocked_by_review` missing=``
- `line_019` recipe=`recipe_019` present=`True` status=`blocked_by_review` missing=``
- `line_020` recipe=`recipe_020` present=`True` status=`blocked_by_review` missing=``
- `line_021` recipe=`recipe_021` present=`True` status=`blocked_by_review` missing=``
- `line_022` recipe=`recipe_022` present=`True` status=`blocked_by_review` missing=``
- `line_023` recipe=`recipe_023` present=`True` status=`blocked_by_review` missing=``
- `line_024` recipe=`recipe_024` present=`True` status=`blocked_by_review` missing=``
- `line_025` recipe=`recipe_025` present=`True` status=`blocked_by_review` missing=``

## Policy

- Offline consistency check only.
- Promotion remains blocked unless recipe validation passes and review blockers clear.
- No runtime deck creation.
- No bot integration.

## Next

`M36-05`: Second-slice readiness comparison.
