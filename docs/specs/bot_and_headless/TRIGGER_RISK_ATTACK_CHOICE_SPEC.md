# Trigger-Risk Attack Choice Spec

## Scope

`M14-03` adds an advisory attack-choice helper that uses exact trigger
probability as a planning signal. It does not perform a drive check, does not
modify power, and does not replace real RNG outcomes.

## Inputs

- visible `GameState`
- player index
- optional `ICardRepository` for visible attacker power
- optional `TriggerProbabilityResult`
- optional battle search options

## Output

`TriggerRiskAttackChoiceResult` includes:

- whether a choice exists
- selected battle sequence candidate id
- total score
- trigger risk
- trigger pressure contribution
- `UsesProbabilityAsPlanningSignal`
- `AppliesTriggerOutcome` which must remain `false`
- sanitized reason text
- cloned visible attackers for display/debug

## Decision Rules

- The helper delegates visible-attacker ordering to `BattleSequenceSearch`.
- Valid `TriggerProbabilityResult` contributes only to candidate scoring.
- Invalid or missing probability is treated as zero trigger risk.
- The helper returns the highest-scoring candidate from the current battle
  search output.

## Hidden-State And RNG Policy

- The caller must pass a visible state appropriate for the bot.
- Hidden attackers are skipped by `BattleSequenceSearch`.
- The helper never mutates `GameState`.
- The helper never appends events.
- The helper never applies trigger power/critical outcomes.
- Reason strings avoid card ids and instance ids.

## Verification

EditMode tests must cover:

- high trigger risk can change the best attack sequence to vanguard-first
- probability is marked as planning-only and never as an applied outcome
- source `GameState` and event log are unchanged
- invalid probability falls back to zero trigger risk
- hidden attackers are skipped and hidden ids do not leak
- repeated choices are deterministic

## Non-Goals

- No full battle execution.
- No guard-response simulation.
- No multi-turn planning. That remains gated until later M14 readiness tasks.
