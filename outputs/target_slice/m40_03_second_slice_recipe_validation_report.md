# M40-03 Second-Slice Recipe Validation Report

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
- Ready for M40-04: `True`

## Recipe Status

- `m40_recipe_001` edge=`BT01-006TH->BT02-033TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_002` edge=`BT01-006TH->TD04-011TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_003` edge=`EB05-001TH->BT02-033TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_004` edge=`EB05-001TH->TD04-011TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_005` edge=`BT03-007TH->BT02-033TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_006` edge=`BT03-007TH->BT02-066TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_007` edge=`BT03-007TH->BT09-063TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_008` edge=`BT03-007TH->BT09-066TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_009` edge=`BT03-007TH->BT09-067TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_010` edge=`BT03-007TH->EB05-026TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_011` edge=`BT03-007TH->EB05-027TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_012` edge=`BT03-007TH->TD04-011TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_013` edge=`EB05-003TH->BT09-066TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_014` edge=`BT03-068TH->BT02-033TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_015` edge=`BT03-068TH->TD04-011TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_016` edge=`BT03-070TH->BT02-033TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_017` edge=`BT03-070TH->TD04-011TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_018` edge=`BT09-065TH->BT02-033TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_019` edge=`BT09-065TH->TD04-011TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_020` edge=`EB05-003TH->BT02-033TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_021` edge=`EB05-003TH->TD04-011TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_022` edge=`BT03-037TH->BT02-066TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_023` edge=`BT03-037TH->BT09-063TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_024` edge=`BT03-037TH->BT09-067TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`
- `m40_recipe_025` edge=`BT03-037TH->EB05-026TH` status=`blocked_by_manual_review` blockers=`1` codes=`manual_review_card_overlap`

## Policy

- Offline validator only.
- Manual-review card overlap blocks runtime readiness.
- Grade-profile mismatch is review evidence, not a blocker.
- No saved-deck injection, UI publication, runtime deck creation, or bot integration.

## Next

`M40-04`: Second-slice combo-to-recipe consistency.
