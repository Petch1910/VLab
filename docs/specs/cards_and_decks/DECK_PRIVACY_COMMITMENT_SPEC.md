# Deck Privacy And Commitment Spec

## Status

Partially implemented. Current gameplay still uses `RoomPlayerInfo.deck_code`
inside the room JSON for friend/manual rooms only.

Implemented:

- optional room/player privacy fields
- canonical deck commitment hashing
- room validation guard that blocks public/ranked rooms from using
  `shared_deck_code`
- commitment-mode metadata validation
- deck reveal verification service for checking a revealed deck plus nonce
  against the stored commitment
- private-event payload boundary spec and game-start guard for unsupported
  privacy modes
- public event model, masking conversion, public replay log support, and
  public-event delivery surface for future non-shared privacy modes
- reveal proof metadata for public reveal event audit/tamper checks
- conservative gameplay policy that keeps commitment-only rooms blocked until
  client-trust UX and owner-private initialization exist
- deck reveal request/response transport flow with response verification against
  the stored commitment
- lobby reveal request/response UI and verification status surface
- owner-private commitment-room initialization spec

Not implemented:

- owner-private commitment-room initialization implementation and client-trust
  UX
- authoritative validator/server-held hidden state

## Goal

Prevent the current convenient friend-room deck exchange from becoming the
integrity model for public, ranked, or tournament play.

This spec defines the future path for hiding deck lists from opponents, proving
that a player did not swap decks after room start, and leaving room for a later
authoritative validator without replacing the M8 event-envelope protocol.

## Current Mode: Friend Shared Deck Code

Mode id: `shared_deck_code`

Current behavior:

- room JSON includes each player's `deck_code`
- both clients import both decks
- both clients build the same true `GameState`
- each client can apply every `NetworkEventEnvelope` through the existing
  reducer

Allowed use:

- friend rooms
- local/manual testing
- development smoke tests

Not allowed:

- ranked
- public matchmaking
- tournament results
- any mode that claims opponent deck privacy

Reason: every client can inspect the opponent deck list and hidden deck order if
they look below the UI layer.

## Future Mode: Commitment Only

Mode id: `deck_commitment`

Purpose:

- hide the opponent deck list at room start
- prove after the match that the deck list and seed material were not swapped
- keep the transport custom-server-free for casual public rooms

Room state should expose commitment metadata instead of `deck_code`:

```json
{
  "deck_privacy_mode": "deck_commitment",
  "players": [
    {
      "player_id": "p1",
      "deck_id": "my-deck",
      "deck_hash": "canonical-deck-hash",
      "deck_commitment": "sha256(canonical_deck_json|player_nonce|room_id|pack_definition_hash)",
      "deck_commitment_algorithm": "sha256-canonical-deck-v1",
      "deck_reveal_policy": "end_of_match"
    }
  ]
}
```

Important limitation:

- commitment alone is privacy, not full anti-cheat
- if both clients remain fully client-authoritative, a malicious client can still
  lie about legal hidden-zone events
- ranked still needs an authoritative validator or signed adjudication layer

## Future Mode: Authoritative Validator

Mode id: `server_held_deck`

Purpose:

- support ranked/tournament integrity
- keep true deck lists, shuffle state, and hidden zone state outside opponent
  clients
- validate every event before forwarding it

The server or validator owns:

- submitted deck lists
- deck commitments
- initial shuffle seed or committed shuffle transcript
- true hidden-zone state
- event cursor and previous-event chain

Clients receive only player/spectator masked views plus public reveal events.

## Commitment Inputs

Canonical deck JSON must be deterministic:

- stable field order
- sorted card entries inside each zone
- normalized pack id/source version
- no user display-only fields

Commitment formula:

```text
deck_commitment =
  SHA256(canonical_deck_json + "|" + player_nonce + "|" + room_id + "|" + pack_definition_hash)
```

The `player_nonce` is private until reveal. At reveal time, the client can prove
the deck matched the room commitment by publishing:

- canonical deck JSON
- player nonce
- pack definition hash
- computed commitment

## Hidden Event Direction

The current reducer expects concrete `card_instance_id` values in every event.
That is correct for `shared_deck_code`, but not enough for `deck_commitment`.

Future privacy modes need one of these approaches:

1. Public event stream plus local private owner state
   - owner client mutates its own hidden zones
   - opponent receives masked events for private draws/searches
   - when a card enters a public zone, the event includes a reveal proof

2. Authoritative validator
   - server applies the true event
   - server sends masked state deltas to clients
   - client reducers stop being the source of truth for opponent hidden zones

Do not fake privacy by keeping `deck_code` in room JSON and hiding it only in UI.

Use `PRIVATE_EVENT_PAYLOAD_SPEC.md` as the source of truth for the public/private
event split before enabling `deck_commitment` gameplay.

## Proposed Room Schema Additions

Backward-compatible optional fields:

```text
MultiplayerRoomState.deck_privacy_mode
RoomPlayerInfo.deck_commitment
RoomPlayerInfo.deck_commitment_algorithm
RoomPlayerInfo.deck_reveal_policy
RoomPlayerInfo.deck_reveal_nonce
```

Rules:

- `shared_deck_code` requires `deck_code`
- `deck_commitment` requires `deck_commitment` and forbids opponent-visible
  `deck_code`
- `server_held_deck` requires a validator/server contract before game start

## Implementation Steps

1. Add data models only: privacy mode fields and commitment info.
2. Add canonical deck serialization and SHA-256 commitment tests.
3. Add room validation rules that reject ranked/public rooms using
   `shared_deck_code`.
4. Add end-of-match reveal verification for commitment-only rooms.
5. Done: Add public/private event payload surface, masking conversion, public
   replay logs, and public-event transport delivery.
6. Done: Add reveal proof metadata for public reveals.
7. Done: Keep commitment-only normal gameplay blocked until explicit
   client-trust UX and owner-private initialization exist.
8. Done: Add end-of-match reveal request/response transport flow.
9. Done: Add end-of-match reveal UI surface.
10. Done: Design owner-private commitment-room initialization before gameplay
    unlock.
11. Implement owner-private initialization and client-trust UX before enabling
    `deck_commitment` gameplay.
12. Add a separate ADR before introducing a custom server or validator backend.

## Test Requirements

- same deck plus same nonce/room/pack creates the same commitment
- changing any card, nonce, room id, or pack hash changes the commitment
- public/ranked room validation rejects `shared_deck_code`
- `deck_commitment` room validation rejects missing commitment metadata
- `deck_commitment` room JSON does not include opponent `deck_code`
- reveal verification accepts the original deck and rejects a swapped deck
- public reveal proof verification rejects tampered public card ids
- commitment-only start is rejected with
  `DECK_COMMITMENT_CLIENT_TRUST_POLICY_REQUIRED`
- deck reveal response verification accepts deck code plus nonce matching the
  stored commitment

## Product Rule

Until this spec is implemented, the app may offer online friend/manual rooms but
must not label them ranked, public competitive, or tournament-authoritative.
