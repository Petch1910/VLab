# Deck Builder Windows Landscape Spec

Milestone: `M24-01`

## Purpose

Make the Windows Deck Builder read as a usable landscape workspace rather than
a debug/card-browser panel.

## Required First-Pass Surfaces

- Card preview panel.
- Card grid.
- Deck list panel.
- Deck counters for Main, Ride, and G.
- Rule badge showing the deck format and playable state.

## Scope

M24-01 is a UI/readability pass only. It must not change:

- Deck legality rules.
- Card pack loading.
- Runtime card data.
- Deck save/load/import/export behavior.
- RulesCore state.

## Acceptance

- Deck panel title is player-facing.
- Rule badge is visible in Deck Builder mode.
- Counters are visible in Deck Builder mode.
- Existing deck validation behavior is unchanged.
- Unity compile and EditMode pass.
- Windows build/smoke pass when runtime UI changes.
