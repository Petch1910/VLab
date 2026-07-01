# Playbook Integration Spec

## Scope

`M14-06` connects archetype/rideline playbooks to the M14 bot helpers as a
deterministic bias layer. It does not execute effects and does not bypass
`RulesCore`.

## Inputs

- live or visible `GameState`
- player index
- optional `ICardRepository`
- optional `BotPlaybookLibrary`
- heuristic bot options
- playbook integration options

## Output

`PlaybookIntegratedBot` returns:

- `BotDecision` from `DecideNext`
- `PlaybookActionEvaluation` rows from `EvaluateActions`

Each action evaluation includes:

- cloned legal action
- base heuristic score
- playbook bias
- total score
- matched playbook id
- sanitized summary

## Bias Rules

- Match playbook from the player's visible public board using
  `BotPlaybookLibrary.MatchFromState`.
- If no playbook matches, use `default_balanced`.
- If an action calls a visible own hand card listed in
  `priority_call_card_ids` to rear-guard, add priority call bias.
- If an aggro playbook can move to Battle phase, add a small battle-phase bias.
- Keep tie-breaking deterministic by preserving base evaluation order when
  scores are equal.

## Hidden-State Policy

- Playbook matching uses a player view.
- Priority-call matching can inspect only the acting player's visible own hand.
- Opponent private zones remain masked.
- Summary/reason strings omit card ids and instance ids.
- The helper never mutates `GameState`.

## Verification

EditMode tests must cover:

- priority call bias prefers a playbook card
- no match falls back to default balanced with no priority bias
- evaluation does not mutate source state
- reason text does not leak opponent private ids or priority card ids
- repeated evaluation is deterministic

## Non-Goals

- No runtime playbook file loader.
- No matchup-specific opponent profile.
- No automatic ability execution.
