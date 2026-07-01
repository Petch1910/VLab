# M29-03 Photon Lobby Quick Deck Selector Closeout

## Status

Done.

## Scope

This slice adds the first Quick Deck Selector implementation to the Windows
Photon lobby. It lets the player cycle from the session deck to saved local
decks before hosting, joining, or reconnecting. It does not change Photon
payloads, reconnect semantics, deck privacy policy, or `GameState`.

## Changes

- Added `Quick Deck` summary to the lobby Connection panel.
- Added `Prev Deck`, `Next Deck`, and `Quick Edit` controls.
- Loaded saved local decks from `DeckStorage`, with test injection support.
- `Host`, `Join`, and `Reconnect` now use the currently selected lobby deck.
- Deck switching and Quick Edit are disabled while a room is active.
- Direct selector/Edit invocation while in a room does not mutate
  `MultiplayerRoomState`.
- Quick Edit routes to the Deck Builder before room creation.

## Tests

- `MultiplayerLobbyStatusFormatterTests.QuickDeckSelectorFormatsCountsLockAndNoDeckCode`
- `MultiplayerLobbyTests.MultiplayerLobbyBootstrapCreatesRuntimeUiWithInjectedTransport`
- `MultiplayerLobbyTests.MultiplayerLobbyQuickDeckSelectorAppliesBeforeRoomAndLocksInsideRoom`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m29_03_quick_deck_selector.log`
  exited `0` with no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m29_03_quick_deck_selector.xml`
  passed `1144/1144`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m29_03_quick_deck_selector.log`
  passed with `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m29_03_quick_deck_selector.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m29_03_quick_deck_selector.json`
  passed with `blockers=[]`.

## Known Limitations

- Quick Edit currently routes to Deck Builder instead of opening an embedded
  modal inside the lobby.
- Returning from Deck Builder to the exact same lobby preparation state is not
  implemented yet.

## Next Target

`M29-04`: Quick Edit return-flow/modal planning. Decide whether to implement a
small embedded edit modal or a safer Deck Builder return handoff, then implement
the smallest verified slice.
