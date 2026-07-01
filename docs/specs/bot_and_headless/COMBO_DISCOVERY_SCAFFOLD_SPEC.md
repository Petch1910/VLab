# Combo Discovery Scaffold Spec

## Status

Implemented in `M9-06`.

## Purpose

Create a small offline/advisory report runner that combines the M9 planning
helpers into one deterministic output. This gives future combo discovery and
bot tuning a stable artifact before adding heavy search, Monte Carlo, or
self-play.

## Inputs

- current `GameState` or masked view selected by caller
- player index
- opponent player index
- `ICardRepository`
- `BotPlaybookLibrary`
- trigger pool counts for exact probability
- battle search options

## Output

Report fields:

- report id
- player and opponent indices
- matched playbook id
- preferred bot profile
- board score
- opponent expected/max shield estimate
- probability of at least one trigger
- ranked battle sequence candidate ids
- explanation text

## Boundary

The scaffold must not:

- mutate `GameState`
- execute actions
- consume RNG
- inspect hidden opponent card ids
- claim discovered combo lines are legal beyond current advisory candidates

## Acceptance Tests

- report includes playbook, board score, guard estimate, trigger probability,
  and ranked battle candidate ids
- report JSON round-trips through Unity `JsonUtility`
- runner is deterministic
- state JSON is unchanged after running

## Future Extensions

- batch run across replay states
- export JSONL datasets for analysis
- include AbilityCore-supported structured effects
- mine repeated winning battle/action motifs
- feed self-play/RL pipelines later
