# Battle Sequence Search Spec

## Status

Implemented in `M9-03`.

## Purpose

Prototype a deterministic battle-order ranking helper for bot planning.

The current core does not yet have a full attack/guard/drive/damage action
model, so this milestone must stay advisory: it ranks visible attack sequences
and explanation terms, but it does not execute attacks or emit game commands.

## Inputs

- `GameState` or masked state view selected by the caller
- player index
- `ICardRepository` for visible card power
- `BoardResourceEvaluation` from `BoardResourceEvaluator`
- optional `TriggerProbabilityResult` from `TriggerProbabilityEngine`
- search options such as opponent vanguard power and max candidate count

## Candidate Model

Each candidate contains:

- candidate id
- ordered visible attackers
- estimated total pressure score
- board base score contribution
- trigger probability contribution for vanguard attacks
- explanation text

Attackers are visible known cards from:

- vanguard zone
- rear-guard zone

Hidden or face-down cards are skipped.

## Boundary

This helper must not:

- mutate `GameState`
- call `RulesCore.TryExecute`
- emit `LegalGameAction`
- claim an attack is legal beyond current visible prototype assumptions
- inspect hidden opponent zones
- resolve drive checks or damage checks

## Acceptance Tests

- high-power-first sequence ranks above low-power-first when all else is equal
- vanguard trigger probability increases the vanguard attack candidate score
- hidden attackers are skipped
- repeated searches on the same state are deterministic
- state JSON is unchanged after search

## Future Extensions

- real `Attack` legal action once battle windows exist
- guard shield prediction
- trigger allocation policy
- multi-attack/restand support
- kill-shot probability using exact and Monte Carlo helpers
