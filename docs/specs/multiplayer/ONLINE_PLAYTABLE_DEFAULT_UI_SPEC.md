# Online PlayTable Default UI Spec

Milestone: `M25-05`

## Purpose

Keep the online PlayTable primary UI player-facing. Network payload/debug counts
must not appear in the default toolbar or main table surface.

## Scope

This milestone only moves online debug detail out of the primary PlayTable
summary. It does not change multiplayer transport, payload publishing, pending
AUTO handling, trigger-check transport, reconnect protocol, or RulesCore.

## Primary UI Contract

The visible toolbar mode summary should show only:

```text
Online | Status: InRoom | Transport: Photon | Cursor: 12
```

It must not show:

- trigger replay payload counts
- pending AUTO payload ids
- reconnect batch counters
- raw network payload text

## Advanced Drawer Contract

Debug counts may still exist inside the Advanced drawer:

- event cursor
- trigger log count
- last reconnect applied count/source cursor

## Verification

- Formatter tests prove primary summary hides debug counts.
- Formatter tests prove advanced detail still reports debug counters.
- Windows-only verification: Unity compile, EditMode, Windows build, and player
  smoke.
