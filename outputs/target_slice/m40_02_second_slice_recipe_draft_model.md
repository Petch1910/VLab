# M40-02 Second-Slice Recipe Draft Model

## Summary

- Target slice: `Classic Core` / `โอราเคิล ทิงค์ แทงค์`
- Candidate edge inputs: `259`
- Recipe drafts: `25`
- Quantity-complete drafts: `25`
- Drafts with manual-review card overlap: `25`
- Fixture scaffold cards: `14`
- Ready for M40-03: `True`

## Drafts

- `m40_recipe_001` edge=`BT01-006TH->BT02-033TH` score=`13` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_002` edge=`BT01-006TH->TD04-011TH` score=`13` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_003` edge=`EB05-001TH->BT02-033TH` score=`13` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_004` edge=`EB05-001TH->TD04-011TH` score=`13` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_005` edge=`BT03-007TH->BT02-033TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_006` edge=`BT03-007TH->BT02-066TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_007` edge=`BT03-007TH->BT09-063TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_008` edge=`BT03-007TH->BT09-066TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_009` edge=`BT03-007TH->BT09-067TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_010` edge=`BT03-007TH->EB05-026TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_011` edge=`BT03-007TH->EB05-027TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_012` edge=`BT03-007TH->TD04-011TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_013` edge=`EB05-003TH->BT09-066TH` score=`12` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_014` edge=`BT03-068TH->BT02-033TH` score=`10` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_015` edge=`BT03-068TH->TD04-011TH` score=`10` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_016` edge=`BT03-070TH->BT02-033TH` score=`10` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_017` edge=`BT03-070TH->TD04-011TH` score=`10` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_018` edge=`BT09-065TH->BT02-033TH` score=`10` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_019` edge=`BT09-065TH->TD04-011TH` score=`10` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_020` edge=`EB05-003TH->BT02-033TH` score=`10` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_021` edge=`EB05-003TH->TD04-011TH` score=`10` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_022` edge=`BT03-037TH->BT02-066TH` score=`9` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_023` edge=`BT03-037TH->BT09-063TH` score=`9` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_024` edge=`BT03-037TH->BT09-067TH` score=`9` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m40_recipe_025` edge=`BT03-037TH->EB05-026TH` score=`9` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`

## Policy

- Draft quantities are advisory.
- Drafts are pair-anchored from M40-01 candidate edges.
- Drafts use the M35-E2 accepted fixture as a scaffold.
- M40-03 must validate counts, copy limits, clan identity, trigger profile, and grade profile.
- No saved-deck injection, UI publication, runtime promotion, or bot/playbook promotion.
- No live card text parsing.
- No direct `GameState` mutation.

## Next

`M40-03`: Second-slice recipe validator.
