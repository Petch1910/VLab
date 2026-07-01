# Trigger Allocation Modifier Adapter Spec

## Status

Implemented in `M10-04`.

## Purpose

Convert an advisory `TriggerAllocationPlan` into a `CombatModifierLedger` so the
trigger automation path has a clean bridge from planning to future application.

The adapter still does not mutate `GameState`. It only produces ledger entries
that a future validated RulesCore action can apply.

## Inputs

- source id, usually the drive/damage check event id or trigger card instance id
- `TriggerAllocationPlan`
- expiration timing for generated modifiers

## Output

- accepted/manual/rejected adapter result
- `CombatModifierLedger`
- notes copied from allocation side effects and marker notes

## Rules

- each power target becomes one modifier with `power_delta`
- each critical target becomes one modifier with `critical_delta`
- side-effect notes such as draw/heal/over trigger are preserved as notes, not
  fake stat modifiers
- manual allocation plans return manual status and no modifiers

## Boundary

The adapter must not:

- mutate `GameState`
- validate target legality
- execute draw/heal side effects
- consume RNG

## Acceptance Tests

- critical split produces separate power and critical modifiers
- front plan produces one power modifier per visible target
- draw/heal/over notes do not create fake stat modifiers
- unknown/manual plan returns manual status
- repeated conversion is deterministic

## Future Extensions

- link modifier ids to real event ids
- apply modifiers through a legal RulesCore command
- cleanup expired modifiers at battle/end phase
