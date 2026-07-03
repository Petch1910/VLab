# M57-02 Sixth-Slice Human Selection Candidate Digest

## Summary

- Candidates: `12`
- Ready candidates: `12`
- Source groups: `2`
- Target groups: `7`
- Unique structural profiles: `1`
- Same structural profile: `True`
- Human selection recorded: `False`
- Blocking issues: `0`

## Guidance

- Auto-selection: `disabled`
- Tie breaker: All ready candidates have the same structural readiness profile; choose by desired source/target combo preference.

```powershell
python tools\deck\build_sixth_slice_human_selection_preflight.py --review-item-id <review_item_id> --selection-text "<explicit user/team selection reason>"
```

## Comparison Rows

- `1` `m57_01_m56_recipe_001_repair_review` recipe=`m56_recipe_001` edge=`G-BT12-062TH->G-BT12-066TH` source=`G-BT12-062TH` target=`G-BT12-066TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `2` `m57_01_m56_recipe_002_repair_review` recipe=`m56_recipe_002` edge=`G-BT10-026TH->G-BT09-058TH` source=`G-BT10-026TH` target=`G-BT09-058TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `3` `m57_01_m56_recipe_003_repair_review` recipe=`m56_recipe_003` edge=`G-BT10-026TH->G-BT09-060TH` source=`G-BT10-026TH` target=`G-BT09-060TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `4` `m57_01_m56_recipe_004_repair_review` recipe=`m56_recipe_004` edge=`G-BT10-026TH->G-BT09-062TH` source=`G-BT10-026TH` target=`G-BT09-062TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `5` `m57_01_m56_recipe_005_repair_review` recipe=`m56_recipe_005` edge=`G-BT10-026TH->G-BT12-031TH` source=`G-BT10-026TH` target=`G-BT12-031TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `6` `m57_01_m56_recipe_006_repair_review` recipe=`m56_recipe_006` edge=`G-BT10-026TH->G-BT12-065TH` source=`G-BT10-026TH` target=`G-BT12-065TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `7` `m57_01_m56_recipe_007_repair_review` recipe=`m56_recipe_007` edge=`G-BT10-026TH->G-TD10-010TH` source=`G-BT10-026TH` target=`G-TD10-010TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `8` `m57_01_m56_recipe_008_repair_review` recipe=`m56_recipe_008` edge=`G-BT12-062TH->G-BT09-058TH` source=`G-BT12-062TH` target=`G-BT09-058TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `9` `m57_01_m56_recipe_009_repair_review` recipe=`m56_recipe_009` edge=`G-BT12-062TH->G-BT09-060TH` source=`G-BT12-062TH` target=`G-BT09-060TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `10` `m57_01_m56_recipe_010_repair_review` recipe=`m56_recipe_010` edge=`G-BT12-062TH->G-BT09-062TH` source=`G-BT12-062TH` target=`G-BT09-062TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `11` `m57_01_m56_recipe_011_repair_review` recipe=`m56_recipe_011` edge=`G-BT12-062TH->G-BT12-065TH` source=`G-BT12-062TH` target=`G-BT12-065TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`
- `12` `m57_01_m56_recipe_012_repair_review` recipe=`m56_recipe_012` edge=`G-BT12-062TH->G-TD10-010TH` source=`G-BT12-062TH` target=`G-TD10-010TH` manual_subs=`7` grades=`{'0': 17, '1': 14, '2': 11, '3': 8}` g_zone_deferred=`True`

## Source Groups

- `G-BT10-026TH` count=`6` recipes=`['m56_recipe_002', 'm56_recipe_003', 'm56_recipe_004', 'm56_recipe_005', 'm56_recipe_006', 'm56_recipe_007']`
- `G-BT12-062TH` count=`6` recipes=`['m56_recipe_001', 'm56_recipe_008', 'm56_recipe_009', 'm56_recipe_010', 'm56_recipe_011', 'm56_recipe_012']`

## Target Groups

- `G-BT09-058TH` count=`2` recipes=`['m56_recipe_002', 'm56_recipe_008']`
- `G-BT09-060TH` count=`2` recipes=`['m56_recipe_003', 'm56_recipe_009']`
- `G-BT09-062TH` count=`2` recipes=`['m56_recipe_004', 'm56_recipe_010']`
- `G-BT12-065TH` count=`2` recipes=`['m56_recipe_006', 'm56_recipe_011']`
- `G-TD10-010TH` count=`2` recipes=`['m56_recipe_007', 'm56_recipe_012']`
- `G-BT12-031TH` count=`1` recipes=`['m56_recipe_005']`
- `G-BT12-066TH` count=`1` recipes=`['m56_recipe_001']`

## Boundary

- This digest does not choose a review item.
- This digest does not record human selection or acceptance.
- This digest does not record a G Zone / Stride decision.
- This digest does not create the real M57-02 selected artifact.
- This digest does not create a runtime fixture, publish decks, enable bot playbooks, or mutate GameState.

## Next

`M57-02-user-selection`: Choose one ready review_item_id, provide selection_text, then run preflight.
