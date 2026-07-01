# Photon Lobby Quick Deck Selector Spec

Milestone: `M29-03`

Status: initial selector slice and embedded Quick Edit modal are complete.

## Purpose

Let a Windows Online Room player inspect and switch the active lobby deck before
hosting, joining, or reconnecting. This is a player-facing preparation feature,
not a transport or rules change.

## Runtime Contract

- `MultiplayerLobbyBootstrap` may load saved local decks from `DeckStorage` for
  the lobby selector.
- The lobby keeps a session deck option plus saved-deck choices.
- `Host`, `Join`, and `Reconnect` use the currently selected lobby deck.
- Once `CurrentRoom` is non-null, deck switching is locked.
- Direct button invocation while a room is active must not mutate
  `MultiplayerRoomState`.
- Quick Edit may route to the Deck Builder only before a room is active.

## Privacy And Safety

- Status text must show deck name and zone counts only.
- Deck codes and revealed deck codes must not be displayed.
- Changing the active online deck after room creation requires leaving the room
  first.
- This slice does not change deck privacy policy, deck commitment handling,
  Photon payloads, reconnect semantics, or `GameState`.

## Initial M29-03 Slice

- Add a `Quick Deck` summary in the Connection panel.
- Add `Prev Deck`, `Next Deck`, and `Quick Edit` controls.
- Cycle through the session deck and saved local decks before room creation.
- Disable deck switching and Quick Edit while a room is active.
- Quick Edit currently routes to Deck Builder before room creation.

## M29-04 Embedded Quick Edit Modal

The original UX target asks for Quick Edit inside the lobby. The first slice
used a safe Deck Builder route. `M29-04` replaces that with an embedded modal
for deck-code import:

- `Quick Edit` opens a lobby modal before room creation.
- The modal shows active deck counts and a deck-code input.
- `Apply Deck Code` imports the code into the active lobby deck for this
  session only.
- The modal does not save to `DeckStorage`.
- The modal does not display deck codes in status text.
- The room-state lock remains: once a room is active, direct apply/open
  invocation must reject and leave `MultiplayerRoomState` unchanged.

## M29-05 Follow-Up

Run an Online Room usability closeout audit against the Windows playable-loop
checklist. The audit should decide whether the Online Room pass is ready to
close or whether more Photon lobby/room UI work is needed before returning to
the deferred `M28-10` PlayTable density review.

## Verification

- Formatter test for deck counts, saved-choice text, lockout text, and no deck
  code leak.
- Runtime UI test for Quick Deck controls.
- Runtime UI test that a saved deck selected before `Host` becomes the room
  local deck, then selector/direct invocation cannot mutate active room state.
- Runtime UI test that Quick Edit deck-code import changes the pre-room online
  deck and cannot mutate active room state after hosting.
- Run Unity compile, EditMode, editor client smoke, Windows build, and Windows
  player smoke when the runtime lobby surface changes.

Initial M29-03 verification passed Unity compile, EditMode `1144/1144`, editor
client smoke `blockers=[]`, Windows build `errors=0 warnings=0`, and Windows
player smoke `blockers=[]`.

M29-04 verification passed Unity compile, EditMode `1146/1146`, editor client
smoke `blockers=[]`, Windows build `errors=0 warnings=0`, and Windows player
smoke `blockers=[]`.
