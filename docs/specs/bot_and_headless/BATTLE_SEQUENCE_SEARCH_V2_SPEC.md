# Battle Sequence Search V2 Spec

## Scope

`M14-04` adds a v2 advisory wrapper around the existing battle sequence search.
It keeps the M9 prototype intact and layers guard-pressure and trigger-risk
signals on top of visible attacker ordering.

This is still not full battle execution and not multi-turn planning.

## Inputs

- visible `GameState`
- player index
- optional `ICardRepository` for visible attacker power
- optional `OpponentGuardEstimate`
- optional `TriggerProbabilityResult`
- optional v2 options

## Output

Each `BattleSequenceV2Candidate` includes:

- candidate id
- base M9 battle search score
- guard pressure contribution
- trigger pressure contribution
- trigger risk
- total score
- sanitized explanation
- cloned visible attackers

## Scoring

```text
total_score = base_battle_score + guard_pressure_contribution
```

The base score already includes trigger pressure from
`BattleSequenceSearch`. V2 adds guard pressure:

- estimate shield needed for each visible attacker against the configured
  opponent vanguard power
- weight earlier attacks higher so front-loaded pressure can matter
- reward expected shield drain
- reward demand that exceeds expected shield as guard-break pressure

## Hidden-State And Mutation Policy

- Hidden attackers are skipped by the base visible-attacker collector.
- V2 clones attacker summaries before returning them.
- V2 never mutates `GameState`.
- V2 never appends events or applies trigger outcomes.
- Explanation strings avoid card ids and instance ids.

## Verification

EditMode tests must cover:

- guard estimate adds positive guard pressure
- trigger risk and trigger pressure are carried into v2 output
- hidden attackers are skipped
- source `GameState` is unchanged
- repeated search output is deterministic

## Non-Goals

- No guard response simulation.
- No attack event execution.
- No snapshot branch execution. That starts at `M14-05`.
