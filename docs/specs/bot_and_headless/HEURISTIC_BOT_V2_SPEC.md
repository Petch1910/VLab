# Heuristic Bot V2 Spec

## Scope

`M14-01` adds a deterministic bot chooser that ranks the current
`RulesCore.GetLegalActions` output with the existing board/resource evaluator.
It is still a one-action chooser, not an advanced planner, ISMCTS searcher, or
RL policy.

## Contract

- Input: live `GameState`, acting player index, optional `ICardRepository`, and
  optional heuristic weights.
- Output: `BotDecision` containing one legal action and a short sanitized
  reason.
- The bot must not mutate the input `GameState`.
- The bot must execute candidate actions only on cloned branch states.
- The selected action must still be accepted by `RulesCore.CanExecute` on the
  original state.
- Ranking must be deterministic. Equal scores keep original legal-action order.

## Hidden-State Policy

- Candidate actions are generated from the true state only because current legal
  actions need real local instance ids.
- Candidate scoring must use a player-view evaluation state.
- Opponent hand, ride deck, and deck identities must remain masked during
  scoring.
- Owner deck identity must not leak through hypothetical draw scoring. When a
  candidate draw is simulated, the drawn card is masked before
  `BoardResourceEvaluator` sees it, so top-deck card stats cannot affect the
  draw score.
- Decision reason strings must not include card ids or card instance ids.

## Scoring

For each executable candidate:

```text
score = own_board_score - opponent_board_score + action_bias
```

`own_board_score` and `opponent_board_score` come from
`BoardResourceEvaluator`. Action bias is intentionally small and rule-agnostic:

- draw: slight positive bias
- hand to empty vanguard: strong setup bias
- hand to vanguard after a vanguard exists: small positive bias
- hand to rear-guard: small pressure bias
- moves to drop or damage: penalty
- phase changes: small phase-specific bias
- gift marker: small penalty until format-specific gift rules are automated

## Verification

EditMode tests must cover:

- returned decision is executable through `RulesCore`
- input state is unchanged after thinking
- empty-vanguard setup prefers hand-to-vanguard over hand-to-rear-guard
- top-deck card stats do not change draw scoring
- reason text does not leak top-deck or opponent private card ids
- repeated evaluation is deterministic

## Non-Goals

- No guard bot decisions. That starts at `M14-02`.
- No trigger-risk battle search integration. That starts at `M14-03` and
  `M14-04`.
- No advanced search/ISMCTS/RL until the `M14-09` readiness gate passes.
