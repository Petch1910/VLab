# Multiplayer Room Spec

## Status

Milestone 8 foundation is implemented as an event-log protocol, an in-memory
mock relay, Photon Realtime payload/transport scaffolding, and hidden-state
views for player/spectator/replay consumers. The important contract is that
online play must reuse the same `GameEvent` stream used by local replay, so
network sync does not create a second game engine. The current live path has a
game-session controller that can publish local `RulesCore` actions and apply
remote envelopes over Photon Realtime. `PlayTableBootstrap` can now run in an
online-session mode that publishes manual table actions through that controller.
The lobby can now exchange deck codes, build the agreed initial true state from
room seed, apply compatible pending reconnect batches, and hand the transport to
the online PlayTable. The table shows online event cursor and reconnect sync
status. This is suitable for friend/manual rooms; ranked or public rooms need a
deck privacy/commitment upgrade before use.

## Implementation Files

- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/MultiplayerProtocol.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/IMultiplayerTransport.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/MockMultiplayerRoom.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/MultiplayerLobbyController.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/MultiplayerGameSessionController.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/NetworkReconnectRequest.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/NetworkPublicGameEvent.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/NetworkPublicGameEventFactory.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/NetworkPublicGameReplay.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/NetworkPublicGameReplayPlayer.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/NetworkDeckRevealRequest.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/NetworkDeckRevealResponse.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Multiplayer/DeckRevealResponseVerifier.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/PlayTableBootstrap.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/MultiplayerLobbyBootstrap.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Game/GameStateViewFactory.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Game/GameStateViewPerspective.cs`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Game/GameReplayPlayer.cs`
- `client/unity/VanguardThaiSim/Assets/Tests/EditMode/MultiplayerProtocolTests.cs`
- `client/unity/VanguardThaiSim/Assets/Tests/EditMode/MultiplayerGameSessionTests.cs`
- `client/unity/VanguardThaiSim/Assets/Tests/EditMode/GameStateViewTests.cs`

## Room State

```json
{
  "protocol_version": 1,
  "room_id": "ABCD",
  "format": "D",
  "state": "waiting",
  "host_player_id": "p1",
  "random_seed": 9001,
  "pack": {
    "pack_id": "vanguard_th",
    "source_version": "2026-06-19",
    "definition_hash": "...",
    "image_manifest_hash": "...",
    "image_content_hash": "..."
  },
  "players": [
    {
      "player_id": "p1",
      "display_name": "Host",
      "deck_id": "host-deck",
      "deck_hash": "...",
      "deck_code": "VGTH1....",
      "connected": true,
      "event_cursor": 0
    }
  ]
}
```

## Event Envelope

```json
{
  "protocol_version": 1,
  "room_id": "ABCD",
  "game_id": "game-...",
  "player_id": "p1",
  "event_index": 12,
  "previous_event_id": "event-000012",
  "sent_at_utc": "2026-06-19T00:00:00.0000000Z",
  "game_event": {
    "event_id": "event-000013",
    "action_type": "Draw",
    "actor_index": 0
  }
}
```

## Sync Model

The transport sends append-only `NetworkEventEnvelope` records. Receivers apply
them through `MultiplayerProtocol.TryApplyEnvelope`, which validates protocol
version, game id, event index, previous event id, and then calls
`GameEventReducer.Apply`.

Do not sync arbitrary UI state. Rebuild table state from:

1. agreed initial `GameState`
2. deterministic random seed
3. ordered `GameEvent` envelopes

`MultiplayerGameSessionController` is the runtime bridge between `RulesCore`
and `IMultiplayerTransport`:

- `ExecuteLocalAction` validates and mutates the local true state through
  `RulesCore.TryExecute`, then publishes the emitted event as a
  `NetworkEventEnvelope`.
- incoming game events are applied through `MultiplayerProtocol.TryApplyEnvelope`
  so game id, event index, previous event id, and reducer execution stay
  identical to replay.
- reconnect requests are answered by creating a `NetworkEventBatch` from the
  local event log.
- reconnect batches are applied envelope by envelope through the same protocol
  validation path.

Initial online state creation:

- `RoomPlayerInfo.deck_code` stores the existing compressed deck-code format.
- `MultiplayerLobbyController.TryCreateInitialGameState()` imports host and
  guest deck codes, uses `room.random_seed`, creates a matching `GameState`,
  and assigns a deterministic room `game_id` so host and guest envelopes agree.
- host is always player index 0; the first connected non-host player is player
  index 1.
- `MultiplayerGameSessionController.LocalPlayerIndex` tells PlayTable which
  player the local UI controls.
- `MultiplayerLobbyController.TryCreateGameSession()` creates the online session
  from the room state and applies a pending reconnect batch when its cursor
  matches the session event cursor.
- `MultiplayerLobbyBootstrap` Start Table hands the existing transport/session
  to `PlayTableBootstrap.ShowOnline()` without disconnecting.

Current privacy tradeoff:

- The first friend-room version shares deck codes inside room state so both
  clients can apply deterministic hidden-zone events.
- Do not treat this as ranked-ready. Public/ranked rooms should use deck
  commitments, server validation, or another hidden-information protocol before
  exposing competitive play.
- Use `DECK_PRIVACY_COMMITMENT_SPEC.md` and
  `ADR/ADR-0004-deck-privacy-before-ranked.md` before implementing public or
  ranked rooms.
- Privacy fields and validation guards now exist, but only `shared_deck_code`
  friend/manual gameplay is implemented. `deck_commitment` still needs private
  event payloads or an authoritative validator before it is playable.
- `MultiplayerLobbyController.TryCreateInitialGameState()` blocks non-shared
  privacy modes from gameplay so unsupported privacy rooms cannot fall through
  to the shared deck-code path.
- `NetworkPublicGameEventFactory` can now convert true `GameEvent` records into
  opponent-safe public events, and `NetworkPublicGameReplay` can store/step
  masked public event logs. `IMultiplayerTransport` and Photon Realtime can now
  carry public event payloads separately from true envelopes. Runtime gameplay
  still only starts from `shared_deck_code` rooms. `DeckPrivacyGameplayPolicy`
  blocks commitment-only normal start until explicit client-trust UX and
  owner-private initialization exist; reveal proof metadata now exists for
  public reveal audit/tamper checks but does not provide ranked-grade
  hidden-state validation by itself. Deck reveal request/response transport now
  exists for end-of-match commitment checks, and the lobby exposes reveal
  request/response controls plus verification status. The owner-private
  initialization design now lives in `OWNER_PRIVATE_ROOM_INITIALIZATION_SPEC.md`,
  but `deck_commitment` gameplay stays blocked until that design is implemented
  with explicit client-trust UX.

## Hidden-State View Contract

`GameState` remains the true source of truth inside the local core, reducer, and
replay engine. UI, bot observation, remote player state, spectator mode, and
replay sharing must use a derived view from `GameStateViewFactory` instead of
handing true state directly to consumers.

Supported perspectives:

- `TrueState`: deep clone of the full state for trusted local core/debug code.
- `Player`: shows the viewer's own hand and ride deck, but hides opponent
  private zones.
- `Spectator`: hides private zones for both players.

Masking policy:

- Deck contents are always hidden, including from the deck owner.
- Hand and ride deck are visible only to the owning player in `Player` view.
- Public zones keep face-up cards visible: vanguard, rear-guard, drop, damage,
  bind, trigger, and order.
- Any face-down card in a public zone is hidden.
- Hidden cards use `card_id = "__hidden_card__"` and stable synthetic
  `instance_id` values so card ids cannot leak through instance ids.
- Event log `card_instance_id` is also masked for private-zone events that the
  viewer should not know. This matters because generated instance ids can
  contain card ids.

Masked views are read-only observation objects. Do not run authoritative
reducers or legal-action execution against a masked view.

Current UI wiring:

- `PlayTableBootstrap` keeps true state for command execution.
- `PlayTableBootstrap.CreateDisplayStateView()` returns a player masked view.
- `RefreshUi()` renders zones and event log from that masked display view.
- `PlayTableBootstrap.ShowOnline()` accepts a `MultiplayerGameSessionController`.
- in online-session mode, Draw/Move/Phase/Gift marker buttons execute through
  the session so the emitted event is published to transport.
- online status displays transport status, event cursor, and reconnect sync
  count after a reconnect batch applies.
- Undo is disabled in online-session mode to avoid divergent event logs.
- Future online room/replay UI must follow the same split: true state for local
  reducer execution, masked state for any user-facing or shareable surface.
- Post-M19 Online Room UI now groups runtime controls into Connection, Room, and
  Safety/Reveal panels. `MultiplayerLobbyStatusFormatter` owns player-facing
  lobby status text and must not display deck codes or revealed deck codes.

## Reconnect Model

Each `RoomPlayerInfo` stores `event_cursor`. A reconnecting client sends a
`NetworkReconnectRequest` from that cursor. The current transport surface can
carry request and `NetworkEventBatch` payloads over Photon event codes 3 and 4.
The receiving gameplay surface must apply each batch envelope through
`MultiplayerProtocol.TryApplyEnvelope`, then advance its cursor to the room
event count.

Current UI wiring:

- `MultiplayerLobbyBootstrap` has a reconnect cursor input and Reconnect button.
- `MultiplayerLobbyController.ReconnectRoom` joins the room with the requested
  cursor.
- after room state is received, the controller sends `NetworkReconnectRequest`
  automatically when the cursor is greater than zero.
- `LastReconnectBatch` records the latest batch received for the next gameplay
  surface to apply.
- lobby reconnect request/batch state is cleared on host, join, disconnect, and
  room-id changes so stale room data cannot be handed to a new table.
- reconnect batches received before a room state exists, or for a different
  room id, are ignored.
- `MultiplayerGameSessionController` can auto-respond to reconnect requests and
  apply received batches once a true `GameState` is active for the room.
- Start Table applies a pending lobby reconnect batch only when
  `batch.from_event_index` equals the newly created session event cursor. A
  batch starting later than the available state is rejected to avoid an event-log
  gap.

## Before Game Start

Check:

- both players are connected
- both decks pass `DeckValidator`
- pack id/source version/definition hash/image manifest hash match
- image content hash matches when the room requires strict asset parity
- random seed is non-zero and agreed

## Current Mock Relay

`MockMultiplayerRoom` is intentionally local and deterministic. It verifies pack
hashes on connect, stores a room event log, publishes host/client actions, and
can create missing-event reconnect batches into a second `GameState`.

## Selected Transport

Photon Realtime is the first selected live transport. Use
`PHOTON_REALTIME_TRANSPORT_SPEC.md` as the detailed integration spec. The current
repo has a compile-safe payload codec, SDK-gated adapter scaffold, imported
Photon Realtime SDK, local AppId config path, editor live smoke-test runner,
lobby UI/controller, reconnect request/batch transport surface, and a live
lobby/controller smoke runner. It also has a live game-session smoke runner
that verifies Photon-delivered game-event sync and reconnect batch delivery.

## Future Options

- Add deck privacy/commitment for public or ranked rooms
- Firebase/Supabase profiles/decks around Photon rooms
- Custom WebSocket or authoritative server later using the same envelope schema
