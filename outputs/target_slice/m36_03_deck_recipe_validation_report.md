# M36-03 Deck Recipe Validation Report

## Summary

- Recipes validated: `25`
- Runtime-ready recipes: `0`
- Validator passed: `0`
- Passed pending human acceptance: `0`
- Invalid drafts: `1`
- Blocked by review: `24`
- Slot-gap recipes: `16`
- Trigger-count mismatch recipes: `12`
- Missing-card recipes: `0`
- Copy-limit violation recipes: `0`
- Ready for M36-04: `True`

## Recipe Status

- `recipe_001` line=`line_001` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_002` line=`line_002` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_003` line=`line_003` status=`invalid_draft` blockers=`3` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch`
- `recipe_004` line=`line_004` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_005` line=`line_005` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_006` line=`line_006` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_007` line=`line_007` status=`blocked_by_review` blockers=`3` codes=`main_deck_size_mismatch,unfilled_slots,review_status_blocked`
- `recipe_008` line=`line_008` status=`blocked_by_review` blockers=`3` codes=`main_deck_size_mismatch,unfilled_slots,review_status_blocked`
- `recipe_009` line=`line_009` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_010` line=`line_010` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_011` line=`line_011` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_012` line=`line_012` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_013` line=`line_013` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_014` line=`line_014` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_015` line=`line_015` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_016` line=`line_016` status=`blocked_by_review` blockers=`3` codes=`main_deck_size_mismatch,unfilled_slots,review_status_blocked`
- `recipe_017` line=`line_017` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_018` line=`line_018` status=`blocked_by_review` blockers=`3` codes=`main_deck_size_mismatch,unfilled_slots,review_status_blocked`
- `recipe_019` line=`line_019` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_020` line=`line_020` status=`blocked_by_review` blockers=`1` codes=`review_status_blocked`
- `recipe_021` line=`line_021` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_022` line=`line_022` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_023` line=`line_023` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_024` line=`line_024` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`
- `recipe_025` line=`line_025` status=`blocked_by_review` blockers=`4` codes=`main_deck_size_mismatch,unfilled_slots,trigger_count_mismatch,review_status_blocked`

## Policy

- Offline validator only.
- Runtime-ready count must stay separate from validator output availability.
- Review blockers remain blockers even when card counts are numeric.
- No runtime deck creation.
- No bot integration.

## Next

`M36-04`: Combo-line to recipe consistency check.
