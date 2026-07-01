# Private Event Payload Spec

## Status

Partially implemented. The current playable online mode remains
`shared_deck_code` friend rooms. `deck_commitment` and `server_held_deck` rooms
are blocked from normal lobby gameplay until client-trust UX plus owner-private
initialization or an authoritative validator exists.

Implemented guard:

- lobby game start validates room privacy before importing shared deck codes
- public/ranked `shared_deck_code` rooms are rejected
- non-shared privacy modes return `DECK_PRIVACY_MODE_UNSUPPORTED_FOR_GAMEPLAY`
  before state creation
- `NetworkPublicGameEvent` model exists with visibility, count deltas, hidden
  identity marker, public card identity, and reveal proof fields
- Photon payload codec can round-trip public events on event code 5
- `NetworkPublicGameEventFactory` can derive public events from true
  `GameEvent` records while masking private draw/move identity and revealing
  cards that enter public zones
- `NetworkPublicGameReplay` and `NetworkPublicGameReplayPlayer` can serialize
  and step masked public event logs without reconstructing true hidden state
- `IMultiplayerTransport`, Photon Realtime adapter, and
  `MultiplayerGameSessionController` have a public-event delivery surface:
  shared friend rooms keep true envelopes, while non-shared privacy sessions
  publish public events and store received public logs without mutating true
  state
- reveal proof metadata can bind a public reveal event to the player's deck
  commitment, room id, pack hash, source event id, public card id, and public
  instance id for replay/audit tamper checks
- `DeckPrivacyGameplayPolicy` keeps commitment-only lobby gameplay blocked with
  `DECK_COMMITMENT_CLIENT_TRUST_POLICY_REQUIRED` until explicit client-trust UX
  and owner-private initialization exist
- deck reveal request/response payloads and lobby controller verification status
  exist for end-of-match commitment checks
- lobby UI exposes reveal target/nonce inputs, Request Reveal, Send Reveal, and
  verification status text
- owner-private commitment-room initialization is specified in
  `OWNER_PRIVATE_ROOM_INITIALIZATION_SPEC.md`, but gameplay unlock remains
  blocked until implementation and client-trust UX exist

## Purpose

Define the event payload direction needed before hidden deck/hand privacy can be
claimed in public rooms.

The current `NetworkEventEnvelope` carries a true `GameEvent`. That works when
both clients know both decks, but it leaks private card instance ids and cannot
support deck privacy by itself.

## Required Boundary

Future private rooms need two event surfaces:

- true event: used only by trusted owner-side state or an authoritative
  validator
- public event: delivered to the opponent/spectator and safe to store in a
  shared replay

Do not send true private-zone card ids to an opponent in a `deck_commitment` or
`server_held_deck` room.

## Proposed Payload Types

```text
NetworkEventEnvelope
  true_event: GameEvent
  public_event: NetworkPublicGameEvent
  visibility: public | owner_private | validator_private
  reveal_proof: optional card/deck proof for public reveals
```

For backward compatibility, `shared_deck_code` rooms can continue to use
`game_event` directly until the new payload is implemented.

## Public Event Rules

Private draw/search/move:

- hide `card_instance_id`
- hide `card_id`
- include source/destination zones only when those zones are public knowledge
- include count deltas when public, for example hand size changed by +1

Public reveal:

- include the revealed `card_id`
- include a stable public instance id
- include the source commitment context when needed
- update the public event log so replay viewers can see the reveal

Guard/call/play from hand:

- before reveal, opponent only knows hand count
- when card moves from hand to a public zone, public event includes the revealed
  card id and destination

Deck movement:

- top-deck identity stays hidden until checked/revealed
- shuffle/search events expose only public counts unless a card is revealed

## Commitment Interaction

Commitment-only rooms still cannot prevent a malicious owner client from lying
about hidden-zone events. Commitment proves the final deck list, not each hidden
draw.

Because of that, there are only two acceptable product paths:

1. Casual public rooms with clear "client-trust" warning plus end-of-match deck
   reveal verification.
2. Ranked/tournament rooms with `server_held_deck` and an authoritative
   validator.

Do not label commitment-only client-authoritative rooms as ranked.

## Implementation Steps

1. Done: Add masking conversion from true `GameEvent` to public event.
2. Done: Add replay support for public event logs.
3. Done: Add owner/opponent delivery rules in the transport/controller layer.
4. Done: Add reveal proof handling for cards that leave private zones.
5. Done: Keep commitment-only normal gameplay blocked until explicit
   client-trust UX and owner-private initialization exist.
6. Done: Add end-of-match deck reveal request/response transport flow.
7. Done: Add UI surface for reveal request/response and verification status.
8. Done: Design owner-private commitment-room initialization before gameplay
   unlock.
9. Implement owner-private initialization and client-trust UX before enabling
   `deck_commitment` gameplay.

## Test Requirements

- drawing from deck creates a public event that changes hand count without card
  id leakage
- playing a card from hand creates a public reveal event with card id and public
  destination
- non-card events keep phase/marker metadata without accidental card identity
- public replay does not contain private-zone `card_instance_id`
- public replay stepping does not apply true reducers to incomplete public data
- non-shared privacy sessions publish public events instead of true envelopes
- received public events are stored without mutating true hidden state
- reveal proof verification accepts unchanged public reveal events and rejects
  tampered public card ids
- `deck_commitment` game start remains blocked until public/private event
  client-trust UX and owner-private initialization exist
- deck reveal response verifies a revealed deck code plus nonce against the
  stored room commitment
- ranked rooms remain blocked without `server_held_deck` validator support
