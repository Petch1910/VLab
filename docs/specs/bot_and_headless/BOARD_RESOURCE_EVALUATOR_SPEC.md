# Board Resource Evaluator Spec

## Status

Implemented in `M9-02`.

## Purpose

Provide deterministic board/resource scores for bot planning without mutating
game state or reading hidden opponent information.

The evaluator is a heuristic tool. It does not decide legality and does not
execute actions.

## Inputs

- a `GameState` or masked `GameState` view already chosen by the caller
- player index to evaluate
- optional `ICardRepository` lookup for visible card stats such as shield
- scoring weights

The caller is responsible for passing a legal observation surface. For opponent
evaluation, use a masked public/player view instead of true private state.

## Metrics

MVP metrics:

- hand count
- visible hand shield value
- visible hand unknown count
- vanguard count
- rear-guard count
- damage count
- available counter-blast count from face-up damage
- estimated soul count from stacked vanguard cards until a dedicated Soul zone
  exists
- deck count

## Output

Return:

- total score
- metric values
- per-term weighted score entries
- explanation text suitable for bot decision logs

## Boundaries

The evaluator must not:

- mutate `GameState`
- call `RulesCore.TryExecute`
- use live RNG
- inspect card order for hidden information
- infer hidden opponent card ids from instance ids

## Acceptance Tests

- visible hand shield and hand count increase score
- high damage lowers score
- face-up damage increases available counter-blast score
- hidden/missing card stats are counted as unknown instead of throwing
- evaluating the same state twice returns the same score and terms

## Future Extensions

- pressure score from current attack lines
- grade/rideline quality
- perfect guard and sentinel feature tags
- trigger density from `TriggerProbabilityEngine`
- opponent public shield estimate from revealed information only
