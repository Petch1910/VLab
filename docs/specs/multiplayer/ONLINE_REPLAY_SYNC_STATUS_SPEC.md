# Online Replay Sync Status Spec

Milestone: `M25-06`

## Purpose

Show online replay/sync status in player-facing language on the Windows
PlayTable.

## Scope

This milestone only formats and surfaces status from existing online session
state. It must not change public-event replay application, reconnect protocol,
payload formats, Photon transport, or hidden-state masking.

## Required Status

Online PlayTable match log should include:

```text
Replay Sync
Online cursor: 3
Public replay events: 3
Status: synced
Reconnect: none applied
```

When counts differ:

- public replay behind local event cursor
- public replay ahead of local event cursor

Reconnect result text must be action-oriented and must not include raw payload
JSON, deck codes, card instance ids, or private ids.

## Verification

- Formatter tests cover synced, behind, ahead, accepted reconnect, and rejected
  reconnect status.
- Existing event log privacy tests continue to pass.
- Windows-only verification: Unity compile, EditMode, Windows build, and player
  smoke.
