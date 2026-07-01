# Photon Lobby Room UI Polish Spec

## Status

Implemented as a post-M19 player-experience slice.

`M29-01` follow-up is complete: the Online Room now has a navigation lockout
while a Photon room is active.

`M29-03` follow-up is complete: the Connection panel has the first Quick Deck
Selector slice.

`M29-04` follow-up is complete: Quick Edit now opens an embedded lobby modal for
session-local deck-code import.

## Purpose

The Photon lobby already had transport, room, reconnect, start-table, and deck
reveal controls, but the runtime surface still looked like a debug dialog. This
slice reorganizes that same behavior into a player-facing Online Room screen
without changing Photon payloads, reconnect semantics, deck privacy policy, or
`GameState`.

## Runtime Surface

`MultiplayerLobbyBootstrap` now separates Online Room into three visible areas:

- `Connection Panel`
  - Photon transport status
  - AppId setup status
  - selected deck and local card-pack summary
  - Quick Deck summary and saved-deck cycle controls
  - Quick Edit deck-code modal before room creation
  - player name, room id, connect, host, join, leave room
- `Room Panel`
  - room id/state/player summary
  - reconnect request/batch summary
  - reconnect cursor input
  - reconnect and start-table actions
- `Safety Reveal Panel`
  - trusted-client friend-room warning
  - acknowledgement button for commitment-mode warning
  - deck reveal request/response summary
  - reveal target/nonce inputs and request/send controls

The screen also has a `Back Home` action. Closing the lobby no longer leaves a
blank runtime screen when launched from the Home Lobby.

## M29-01 Navigation Lockout

When `MultiplayerLobbyController.CurrentRoom` is non-null:

- `Back Home` is disabled.
- `BackHome()` also rejects direct invocation and keeps the lobby open.
- The player must use the dedicated `Leave Room` button first.
- The lobby shows a compact navigation guidance line explaining the lockout.

When no room is active:

- `Back Home` is available.
- `Leave Room` is disabled.

This lockout prevents accidental Photon room disconnects through normal back
navigation and keeps the room exit action explicit.

## Formatter Boundary

`MultiplayerLobbyStatusFormatter` owns read-only text formatting for:

- connection/AppId/last-message status
- selected deck and pack status
- room player summary
- reconnect request/batch summary
- trusted-client safety summary
- deck reveal request/response summary
- navigation lockout summary

The formatter intentionally does not expose deck codes or revealed deck codes.
Deck hash readiness is shown only as a readiness label.

## Non-Goals

- no Photon event code changes
- no reconnect protocol changes
- no owner-private commitment-room gameplay unlock
- no ranked/public anti-cheat promise
- no mutation of `GameState`
- no direct lobby UI mutation of `MultiplayerRoomState`
- no copied VangPro, Vanguard Area, Dear Days, or official assets

## Verification

- `MultiplayerLobbyStatusFormatterTests`
  - connection/AppId formatting
  - selected deck/pack formatting
  - room summary without deck-code leak
  - reconnect request/batch formatting
  - trusted-client warning visibility
  - reveal summary without revealed deck-code leak
  - navigation lockout text without deck-code leak
- `MultiplayerLobbyTests.MultiplayerLobbyBootstrapCreatesRuntimeUiWithInjectedTransport`
  - runtime UI creates connection, room, safety/reveal, reconnect, start-table,
    back-home, and leave-room controls with injected fake transport
- `MultiplayerLobbyTests.MultiplayerLobbyLocksBackHomeWhileRoomIsActive`
  - host-room flow disables `Back Home`, direct `BackHome()` invocation is
    rejected, and `Leave Room` clears the room before `Back Home` unlocks

Run Unity compile and EditMode tests after changing this surface.
