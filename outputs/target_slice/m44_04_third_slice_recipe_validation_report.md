# M44-04 Third-Slice Recipe Validation Report

## Summary

- Recipes validated: `25`
- Runtime-ready recipes: `0`
- Validator passed: `0`
- Passed pending human selection: `0`
- Invalid drafts: `0`
- Blocked by manual review: `25`
- Slot-gap recipes: `0`
- Trigger-count mismatch recipes: `0`
- Missing-card recipes: `0`
- Copy-limit violation recipes: `0`
- Grade-profile review recipes: `25`
- Ready for M44-05: `True`

## Recipe Status

- `m44_recipe_001` edge=`EB10-007TH-B->EB06-023TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_002` edge=`EB10-007TH-B->EB06-S06TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_003` edge=`EB10-007TH-W->EB06-023TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_004` edge=`EB10-007TH-W->EB06-S06TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_005` edge=`EB10-S07TH-B->EB06-023TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_006` edge=`EB10-S07TH-B->EB06-S06TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_007` edge=`EB10-S07TH-W->EB06-023TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_008` edge=`EB10-S07TH-W->EB06-S06TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_009` edge=`EB10-026TH-B->EB06-020TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_010` edge=`EB10-026TH-B->EB10-012TH-B` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_011` edge=`EB10-026TH-B->EB10-012TH-W` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_012` edge=`EB10-026TH-W->EB06-020TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_013` edge=`EB10-026TH-W->EB10-012TH-B` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_014` edge=`EB10-026TH-W->EB10-012TH-W` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_015` edge=`EB06-017TH->EB10-012TH-B` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_016` edge=`EB06-017TH->EB10-012TH-W` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_017` edge=`EB06-022TH->EB10-012TH-B` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_018` edge=`EB06-022TH->EB10-012TH-W` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_019` edge=`EB06-023TH->EB10-012TH-B` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_020` edge=`EB06-023TH->EB10-012TH-W` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_021` edge=`EB06-S06TH->EB10-012TH-B` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_022` edge=`EB06-S06TH->EB10-012TH-W` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_023` edge=`EB10-007TH-B->EB06-008TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_024` edge=`EB10-007TH-B->EB06-011TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m44_recipe_025` edge=`EB10-007TH-B->EB06-013TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`

## Policy

- Offline validator only.
- Manual-review card overlap blocks runtime readiness.
- Grade-profile mismatch is review evidence, not a blocker.
- No saved-deck injection, UI publication, runtime deck creation, or bot integration.

## Next

`M44-05`: Third-slice combo-to-recipe consistency.
