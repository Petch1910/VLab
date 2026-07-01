# M35-E4 Bot Integration Gate

## Summary

- Reviewed hint candidates: `1`
- Blocked sources: `1`
- Gate passed: `True`
- Runtime bot integration enabled: `False`

## Allowed Future Hint Candidates

- `seed_001` anchor=`BT04-078TH` allowed=`True`

## Blocked Sources

- `M35-E3 semantic/compatibility probe` edges=`2660` candidates=`259` blocked=`True`

## Required Runtime Gates

- `bot_may_consume_only_reviewed_hint_candidates`: `True`
- `legal_action_mask_required`: `True`
- `masked_state_view_required`: `True`
- `RulesCore_command_validation_required`: `True`
- `direct_GameState_mutation_forbidden`: `True`
- `true_hidden_state_access_forbidden`: `True`
- `live_card_text_parsing_forbidden`: `True`
- `unreviewed_e3_probe_edges_forbidden`: `True`
- `auto_publish_to_runtime_or_bot_forbidden`: `True`

## Next

`M35-closeout`: close the Hybrid Vertical-Slice Strategy status and choose
the next implementation queue. Runtime bot wiring remains disabled until a
separate Unity/C# milestone opens with legal-action and masked-state tests.
