# M36 Deck Recipe Validation Closeout

## Summary

- M36 closed: `True`
- Runtime recipe promotion allowed: `False`
- Broader slice scale-out allowed: `False`
- Recommendation: `repair_first_slice_recipe_blockers_before_runtime_or_broader_scaleout`

## M36 Results

- Review items: `31` (`1` accepted seed, `24` rejected lines, `6` manual cards)
- Recipe drafts: `25`
- Accepted seed recipes: `1`
- Runtime-ready recipes: `0`
- Invalid drafts: `1`
- Blocked-by-review recipes: `24`
- Missing-card recipes: `0`
- Copy-limit violations: `0`
- Slot-gap recipes: `16`
- Trigger-count mismatch recipes: `12`
- Combo cards present: `25`
- Promotable combo lines: `0`
- Second slice future recipe-ready: `True`
- Second slice probe candidate edges: `259`

## Blockers

- `human_review_blocked_lines`: `24` - Triage rejected combo lines by unsupported semantic gap before re-drafting.
- `unfilled_recipe_slots`: `16` - Generate source-backed slot-fill candidates, starting with the accepted seed recipe.
- `trigger_count_mismatch`: `12` - Repair trigger package candidates before any recipe can be runtime-ready.
- `invalid_draft`: `1` - Keep invalid drafts advisory until validator pass and human acceptance are both present.
- `no_promotable_combo_line`: `1` - Do not export runtime playbooks until a validated recipe contains a promotable line.

## Next Queue

`M37`: First-slice blocker resolution and recipe repair

First tasks:

- `M37-01`: Accepted seed slot-gap completion candidates
- `M37-02`: Trigger package repair proposal
- `M37-03`: Rejected-line support-gap triage
- `M37-04`: Manual semantic mapping candidates
- `M37-05`: Revised recipe validation rerun
- `M37-closeout`: First runtime-ready recipe decision

Hard gates:

- no runtime deck promotion until validator_passed and human_acceptance are both true
- no bot/playbook promotion until combo consistency promotion_allowed is true
- no automatic fill from raw card text without reviewable source evidence
- no direct GameState mutation
- no hidden-state or private deck leakage
