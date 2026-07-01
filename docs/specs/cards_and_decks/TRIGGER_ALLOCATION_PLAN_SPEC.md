# Trigger Allocation Plan Spec

## Status

Implemented in `M10-02`.

## Purpose

Create an advisory trigger allocation plan model that maps a resolved trigger
bonus to suggested visible friendly targets.

This is not the final trigger resolver. It does not mutate game state, validate
attack windows, or move drive/damage check cards. It only produces a deterministic
plan that later automation can validate and execute through RulesCore/event logs.

## Inputs

- `GameState` or masked state view selected by caller
- player index
- `TriggerResolveResult`

## Output

Plan fields:

- accepted/manual flag
- trigger type
- power target suggestions
- critical target suggestions
- side-effect notes, for example draw/heal/over trigger
- explanation text

Target fields:

- card instance id
- card id
- zone
- zone index
- power bonus
- critical bonus

## MVP Allocation Rules

- hidden/unknown units are never selected
- critical trigger:
  - critical bonus prefers visible vanguard
  - power bonus prefers the highest-current-power visible friendly unit
- draw/heal trigger:
  - power bonus prefers the highest-current-power visible friendly unit
  - side-effect note records draw/heal intent
- front trigger:
  - front-row power bonus is assigned to all visible vanguard/rear-guard units
    until circle position data exists
- over trigger:
  - over marker note is recorded
  - large power bonus prefers visible vanguard, falling back to highest visible
    friendly unit
- none trigger:
  - accepted with no target suggestions
- unknown trigger:
  - manual resolution required

## Boundaries

The planner must not:

- mutate `GameState`
- parse Thai card text
- consume RNG
- decide that a future allocation is legal in a real battle window
- select hidden or face-down cards

## Acceptance Tests

- critical trigger can split critical and power suggestions
- front trigger targets visible units and skips hidden units
- draw/heal/over notes are emitted
- unknown trigger requires manual resolution
- repeated planning is deterministic
- state JSON is unchanged after planning

## Future Extensions

- real front/back circle position data
- target legality validation against current battle window
- user/bot target choice UI
- damage-check heal legality
- event-log integration for applied allocations
