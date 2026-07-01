# M29-05 Online Room Usability Closeout Audit

## Status

Done.

## Scope

This is a docs-only audit of the Windows Online Room checklist after the
M29-01 through M29-04 Photon lobby reopen pass.

## Checklist Review

| Requirement | Status | Evidence |
| --- | --- | --- |
| Navigation lockout while connected to a Photon room | Pass | `M29-01` disables `Back Home`, requires `Leave Room`, and guards direct `BackHome()` invocation. |
| Quick Deck Selector in lobby | Pass | `M29-03` adds session/saved-deck cycling before room creation. |
| Quick Edit Modal in lobby | Pass with limited scope | `M29-04` adds embedded deck-code import modal. It is session-local and not a full card-by-card editor. |
| Photon connection, room id, players, pack hash, cursor status | Pass | Existing M25 room status surface remains in `MultiplayerLobbyStatusFormatter.FormatRoomStatus(...)`. |
| Reconnect request/batch/cursor handoff | Pass | `M29-02` expands reconnect summary and guidance. |
| No deck-code leaks in status text | Pass | M25/M29 formatter tests cover room/reveal/quick-deck status without deck-code display. |
| Deck readiness before Host/Join/Reconnect | Gap | The lobby shows deck counts/hash readiness, but `Host`, `Join`, and `Reconnect` do not block obviously unready local decks before creating/sending room state. |

## Decision

Continue one more Photon lobby slice before closing the online-room pass:

- Add a lightweight online deck readiness guard before `Host`, `Join`, and
  `Reconnect`.
- The guard should be count-based and local-only for now:
  - deck exists
  - main deck is exactly `50`
  - ride deck is at most `4`
  - G deck is at most `16`
- The guard should not require a card repository and should not perform
  pack-aware card-id validation in this slice.

## Guardrails Preserved

- No Photon transport switch.
- No Photon payload schema changes.
- No deck privacy policy changes.
- No `GameState` mutation.
- Windows-only verification remains the active path.

## Verification

Docs-only audit. No Unity tests were required for this audit document.

## Next Target

`M29-06`: Online deck readiness guard before Host/Join/Reconnect.
