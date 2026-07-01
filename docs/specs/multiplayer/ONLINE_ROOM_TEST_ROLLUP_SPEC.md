# Online Room Test Rollup Spec

Milestone: `M25-07`

## Purpose

Keep a compact inventory of the core online-room tests that must stay green
before closing Windows Online Room usability.

## Required Coverage

The rollup must track:

- no deck-code leak in player-facing lobby/reveal text
- stale cursor rejection without state mutation
- reconnect display and failure reason coverage
- masked public event delivery / spectator replay sync

## Scope

This milestone adds coverage inventory and tests only. It must not change
runtime multiplayer behavior, transport payloads, hidden-state rules, or room
lifecycle.

## Verification

- Rollup report test checks all required categories exist.
- Rollup report text must not contain `deck_code` or revealed deck-code values.
- Full Unity EditMode suite remains green.
