# M29-04 Photon Lobby Quick Edit Modal Closeout

## Status

Done.

## Scope

This slice replaces the earlier Quick Edit Deck Builder route with an embedded
Online Room modal for deck-code import. It keeps Photon payloads, reconnect
semantics, deck privacy policy, saved deck storage, and `GameState` unchanged.

## Changes

- Added `Quick Edit Modal` to `MultiplayerLobbyBootstrap`.
- Added `Quick Edit Deck Code Input`, `Apply Deck Code`, and `Close Edit`.
- `Quick Edit` now opens the in-lobby modal before room creation.
- `Apply Deck Code` imports a deck code into the active lobby deck for this
  session only.
- Imported decks become the deck used by `Host`, `Join`, and `Reconnect`.
- Quick Edit apply/open remains locked while `CurrentRoom` is active.
- Direct apply invocation while in a room does not mutate `MultiplayerRoomState`.
- Added `MultiplayerLobbyStatusFormatter.FormatQuickEditStatus(...)`.

## Tests

- `MultiplayerLobbyStatusFormatterTests.QuickEditStatusFormatsSessionImportGuidanceWithoutDeckCode`
- `MultiplayerLobbyTests.MultiplayerLobbyQuickEditDeckCodeAppliesBeforeRoomAndLocksInsideRoom`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m29_04_quick_edit_modal.log`
  exited `0` with no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m29_04_quick_edit_modal.xml`
  passed `1146/1146`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m29_04_quick_edit_modal.log`
  passed with `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m29_04_quick_edit_modal.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m29_04_quick_edit_modal.json`
  passed with `blockers=[]`.

## Known Limitations

- Quick Edit imports deck codes only; it is not a full card-by-card editor.
- Imported lobby decks are session-local and are not saved to `DeckStorage`.

## Next Target

`M29-05`: Online Room usability closeout audit. Verify the Online Room checklist
after M29-01 through M29-04, identify remaining player-facing blockers, and
decide whether to continue Photon UI work or return to the deferred M28-10
PlayTable density review.
