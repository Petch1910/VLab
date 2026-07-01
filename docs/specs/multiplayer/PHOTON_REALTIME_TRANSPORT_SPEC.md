# Photon Realtime Transport Spec

## Decision

Use Photon Realtime as the first production online transport.

Reason:

- The game sends turn-based `GameEvent` records, not frame-by-frame movement.
- Photon Realtime rooms, matchmaking, reliable events, and custom properties map
  directly to the existing M8 event-log protocol.
- It avoids running a custom server while the project is unfunded.
- The same protocol can move to an authoritative custom server later if ranked,
  tournaments, or anti-cheat need it.

Do not use Photon Fusion for the first online version. Fusion is better for
state sync, prediction, and lag compensation. That is useful for action games
but unnecessary overhead for this card game.

## Current Repo Status

Implemented:

- `IMultiplayerTransport`
- `MultiplayerTransportResult`
- `PhotonRealtimeTransportConfig`
- `PhotonRealtimePayload`
- `PhotonRealtimePayloadCodec`
- `PhotonRealtimeTransportAdapter` SDK-gated live bridge
- Photon Realtime Unity SDK 5.1.15 imported into `Assets/Photon`
- scripting define `VANGUARD_PHOTON_REALTIME` enabled for Standalone and Android
- live Photon API usage for `RealtimeClient`, `OpCreateRoom`, `OpJoinRoom`,
  `OpSetCustomPropertiesOfRoom`, and `OpRaiseEvent`
- EditMode tests for room-property round trip, event-payload round trip, and
  SDK bridge availability
- Editor command-line live smoke runner:
  `VanguardThaiSim.EditorTools.PhotonRealtimeSmokeTestRunner.RunFromCommandLine`
- ignored local AppId config path:
  `ProjectSettings/VanguardPhotonLocal.json`
- verified live smoke test using a local AppId: two clients connected, created
  and joined a room, read room state, and delivered one reliable event
- runtime lobby/room UI and controller on top of `IMultiplayerTransport`
- Post-M19 lobby/room UI polish with Connection, Room, and Safety/Reveal panels
  plus player-facing formatter tests that avoid deck-code/revealed-code leaks
- reconnect request and reconnect batch payload surface on Photon event codes 3
  and 4
- editor command-line lobby/controller smoke runner:
  `VanguardThaiSim.EditorTools.PhotonLobbySmokeTestRunner.RunFromCommandLine`
- verified live lobby/controller smoke test: two clients connected, hosted and
  joined a room, propagated room state, sent reconnect request, and delivered a
  reconnect batch
- game-session sync controller that publishes `RulesCore` actions as Photon
  game events and applies remote/reconnect envelopes through the shared
  multiplayer protocol
- editor command-line game-session smoke runner:
  `VanguardThaiSim.EditorTools.PhotonGameSessionSmokeTestRunner.RunFromCommandLine`
- verified live game-session smoke test: two clients connected, hosted and
  joined a room, lifecycle reached `playing`, host draw synced into guest
  state, reconnect batch delivery completed, and lifecycle reached `ended`
- PlayTable online-session mode that routes manual table actions through
  `MultiplayerGameSessionController` and refreshes after session state changes
- lobby deck exchange using `RoomPlayerInfo.deck_code`, agreed initial
  `GameState` creation from room seed, and Start Table handoff to online
  PlayTable
- deterministic room `game_id` creation for lobby-started host/guest sessions
- online PlayTable event cursor/sync status and pending reconnect batch handoff
  from lobby to game session
- deck privacy mode fields, canonical commitment hashing, and validation guards
  that block public/ranked rooms from using shared deck-code room JSON
- deck reveal verification service for checking a revealed deck and nonce
  against the stored commitment
- private-event payload boundary spec and lobby game-start guard for unsupported
  privacy modes
- `NetworkPublicGameEvent` model, Photon payload encode/decode helpers, runtime
  send/receive API, and adapter dispatch for public-safe event delivery on
  event code 5
- `MultiplayerGameSessionController` keeps shared friend rooms on true event
  envelopes and routes non-shared privacy sessions to public events without
  applying public payloads to true hidden state
- reveal proof metadata for public events that expose cards from private zones
- `DeckPrivacyGameplayPolicy` keeps commitment-only normal lobby start blocked
  until explicit client-trust UX and owner-private initialization exist
- deck reveal request/response payloads, Photon event codes 6/7, transport
  hooks, and lobby verification status for end-of-match commitment checks
- lobby reveal request/response UI surface
- owner-private commitment-room initialization spec for the future
  `deck_commitment` casual privacy path

Not implemented yet:

- owner-private commitment-room initialization implementation and client-trust UX
- authoritative validator/server-held hidden-state model for ranked rooms

The adapter returns `PHOTON_APP_ID_MISSING` when neither
`VANGUARD_PHOTON_APP_ID` nor the ignored local config file provides an AppId. If
the SDK or scripting define is removed in another environment, the same adapter
falls back to `PHOTON_SDK_NOT_ENABLED` instead of breaking the Unity project
compile.

## Event Codes

Keep Photon event codes below 200.

| Code | Name | Payload |
| --- | --- | --- |
| `1` | Room state | `PhotonRealtimePayload.json = MultiplayerRoomState` |
| `2` | Game event | `PhotonRealtimePayload.json = NetworkEventEnvelope` |
| `3` | Reconnect request | `PhotonRealtimePayload.json = NetworkReconnectRequest` |
| `4` | Reconnect batch | `PhotonRealtimePayload.json = NetworkEventBatch` |
| `5` | Public game event | `PhotonRealtimePayload.json = NetworkPublicGameEvent` |
| `6` | Deck reveal request | `PhotonRealtimePayload.json = NetworkDeckRevealRequest` |
| `7` | Deck reveal response | `PhotonRealtimePayload.json = NetworkDeckRevealResponse` |

Game events must be sent reliable and ordered.

## Room Custom Properties

Photon room properties should include searchable metadata plus a full room JSON
snapshot.

| Key | Meaning |
| --- | --- |
| `vg_room_json` | Full `MultiplayerRoomState` JSON |
| `vg_protocol` | Multiplayer protocol version |
| `vg_format` | Vanguard format, for example `D` |
| `vg_state` | `waiting`, `in_game`, `finished` |
| `vg_host` | host player id |
| `vg_seed` | agreed deterministic seed |
| `vg_pack_id` | card pack id |
| `vg_pack_version` | pack source version |
| `vg_pack_def` | definition hash |
| `vg_pack_img_manifest` | image manifest hash |
| `vg_pack_img_content` | image content hash |

Only the small searchable fields should be exposed to lobby filtering. The full
JSON snapshot exists for exact room reconstruction after joining. Current friend
rooms include player deck codes inside that JSON so both clients can build the
same initial true `GameState`.

## Connection Flow

1. Client loads local pack manifest and creates `PackSyncInfo`.
2. Client connects to Photon Realtime using AppId, app version, and fixed or
   auto-selected region.
3. Host creates a Photon room with max clients `2`.
4. Host writes room custom properties from `PhotonRealtimePayloadCodec.RoomToProperties`.
5. Guest joins by room id or matchmaking filter.
6. Guest checks local pack against room pack before accepting.
7. Both players validate decks and agree on initial `GameState` and random seed.
8. Each action is converted to `NetworkEventEnvelope`.
9. Sender raises Photon event code `2` reliable.
10. Receiver decodes payload and calls `MultiplayerProtocol.TryApplyEnvelope`.

## Reconnect Flow

Use the existing cursor model:

1. Reconnecting player joins the same Photon room.
2. Client sends event code `3` with current cursor.
3. Host/master client sends event code `4` with missing `NetworkEventBatch`.
4. Client applies every envelope through `TryApplyEnvelope`.

If the event gap cannot be filled, reject the reconnect and ask the player to
restart from replay/spectator state.

The implemented `MultiplayerGameSessionController` can create reconnect batches
from its local event log, respond to reconnect requests, and apply received
batches. `PlayTableBootstrap` can render the session state, show the session
event cursor/reconnect sync status, and publish local manual actions through the
controller. The lobby can create an initial state from room deck codes, assign a
deterministic room game id, apply a compatible pending reconnect batch, and hand
the existing transport to the online PlayTable. Lobby reconnect request/batch
state is cleared when hosting, joining, disconnecting, or switching room ids, and
batches received before a room state or for a different room id are ignored.
Non-shared deck privacy modes are still blocked before normal lobby game-state
creation. A lower-level public event delivery surface exists for the future
private-room path, but commitment-only normal start remains blocked until
owner-private initialization and client-trust UX exist. The design for that
future path lives in `OWNER_PRIVATE_ROOM_INITIALIZATION_SPEC.md`.

## Anti-Cheat Boundary

First version is client-authoritative/manual, like the current local table. This
is acceptable for friend matches.

Current room JSON shares deck codes to let both clients apply deterministic
hidden-zone events. Do not use that model for ranked/public integrity; add a
deck commitment or authoritative validator first. Use
`DECK_PRIVACY_COMMITMENT_SPEC.md` and
`ADR/ADR-0004-deck-privacy-before-ranked.md` as the source of truth for that
work.

Do not promise ranked integrity until one of these exists:

- authoritative hosted room server
- deterministic server-side validator
- signed event log with server adjudication

## Photon Setup Later

When ready to connect live:

1. Create a Photon Realtime AppId.
   Photon Dashboard may label compatible load-balancing apps as `Realtime` or
   `PUN`; the smoke test is the source of truth for whether the AppId works with
   `RealtimeClient`.
2. Put the AppId in environment variable `VANGUARD_PHOTON_APP_ID`, or copy
   `ProjectSettings/VanguardPhotonLocal.example.json` to
   `ProjectSettings/VanguardPhotonLocal.json` and fill `app_id`.
3. Run the existing EditMode tests plus the live smoke runner.
4. Keep `VANGUARD_PHOTON_REALTIME` enabled for Standalone and Android builds.

Do not commit `ProjectSettings/VanguardPhotonLocal.json`. It is ignored by
`.gitignore` and exists only for local machine testing.

## Live Smoke Test Command

```powershell
$env:VANGUARD_PHOTON_APP_ID = "<your photon realtime app id>"
& "C:\Users\Phet\AppData\Local\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\Phet\Documents\Codex\2026-06-19\d-cardfight-area-full-version-4\client\unity\VanguardThaiSim" `
  -executeMethod VanguardThaiSim.EditorTools.PhotonRealtimeSmokeTestRunner.RunFromCommandLine `
  -logFile "C:\Users\Phet\Documents\Codex\2026-06-19\d-cardfight-area-full-version-4\work\photon_live_smoke.log"
```

The runner creates two Photon clients in one editor process, connects both,
creates a unique room, joins the guest, verifies the guest can read room state,
then sends one reliable `NetworkEventEnvelope` from host to guest.

## Live Lobby Smoke Test Command

```powershell
& "C:\Users\Phet\AppData\Local\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\Phet\Documents\Codex\2026-06-19\d-cardfight-area-full-version-4\client\unity\VanguardThaiSim" `
  -executeMethod VanguardThaiSim.EditorTools.PhotonLobbySmokeTestRunner.RunFromCommandLine `
  -logFile "C:\Users\Phet\Documents\Codex\2026-06-19\d-cardfight-area-full-version-4\work\photon_lobby_smoke.log"
```

The runner uses the same AppId loading rules as the runtime lobby and verifies
connect, host, join, propagated room state, reconnect request, and reconnect
batch delivery.

## Live Game Session Smoke Test Command

```powershell
& "C:\Users\Phet\AppData\Local\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\Phet\Documents\Codex\2026-06-19\d-cardfight-area-full-version-4\client\unity\VanguardThaiSim" `
  -executeMethod VanguardThaiSim.EditorTools.PhotonGameSessionSmokeTestRunner.RunFromCommandLine `
  -logFile "C:\Users\Phet\Documents\Codex\2026-06-19\d-cardfight-area-full-version-4\work\photon_game_session_smoke.log"
```

The runner uses two live Photon transports, two lobby controllers, and two
game-session controllers. It verifies host/guest room setup, lifecycle
`playing` propagation, host `Draw` action delivery into guest state, reconnect
request/batch delivery, and lifecycle `ended` propagation.
