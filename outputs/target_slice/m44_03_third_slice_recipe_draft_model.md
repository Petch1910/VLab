# M44-03 Third-Slice Recipe Draft Model

## Summary

- Candidate edge input count: `109`
- Recipe drafts: `25`
- Quantity-complete recipes: `25`
- Recipes with manual card overlap: `25`
- Fixture scaffold cards: `14`
- Fixture scaffold total cards: `50`
- Ready for M44-04: `True`

## Drafts

- `m44_recipe_001` edge=`EB10-007TH-B->EB06-023TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_002` edge=`EB10-007TH-B->EB06-S06TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_003` edge=`EB10-007TH-W->EB06-023TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_004` edge=`EB10-007TH-W->EB06-S06TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_005` edge=`EB10-S07TH-B->EB06-023TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_006` edge=`EB10-S07TH-B->EB06-S06TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_007` edge=`EB10-S07TH-W->EB06-023TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_008` edge=`EB10-S07TH-W->EB06-S06TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_009` edge=`EB10-026TH-B->EB06-020TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_010` edge=`EB10-026TH-B->EB10-012TH-B` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_011` edge=`EB10-026TH-B->EB10-012TH-W` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_012` edge=`EB10-026TH-W->EB06-020TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_013` edge=`EB10-026TH-W->EB10-012TH-B` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_014` edge=`EB10-026TH-W->EB10-012TH-W` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_015` edge=`EB06-017TH->EB10-012TH-B` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_016` edge=`EB06-017TH->EB10-012TH-W` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_017` edge=`EB06-022TH->EB10-012TH-B` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_018` edge=`EB06-022TH->EB10-012TH-W` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_019` edge=`EB06-023TH->EB10-012TH-B` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_020` edge=`EB06-023TH->EB10-012TH-W` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_021` edge=`EB06-S06TH->EB10-012TH-B` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_022` edge=`EB06-S06TH->EB10-012TH-W` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_023` edge=`EB10-007TH-B->EB06-008TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_024` edge=`EB10-007TH-B->EB06-011TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`
- `m44_recipe_025` edge=`EB10-007TH-B->EB06-013TH` cards=`50` triggers=`16` status=`advisory_pair_draft_manual_card_overlap`

## Policy

- Draft quantities are advisory.
- M44-04 must validate deck legality and manual-review blockers.
- No runtime deck creation.
- No saved deck or UI publication.
- No bot playbook promotion.
- No automatic deck injection.

## Next

`M44-04`: Third-slice recipe validator.
