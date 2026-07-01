# ADR-0004: Require Deck Privacy Before Ranked Or Public Rooms

## Status

Accepted

## Context

The first Photon Realtime room implementation shares `RoomPlayerInfo.deck_code`
inside room JSON. This is practical for friend/manual rooms because both clients
can build the same true `GameState` and apply the same event log without a
custom server.

That model is not safe for public or ranked play. It exposes the opponent deck
list and gives a modified client access to hidden information below the UI
layer.

## Decision

Keep `deck_code` room exchange for friend/manual rooms only.

Before public, ranked, tournament, or integrity-sensitive rooms are exposed, add
a deck privacy layer based on `docs/DECK_PRIVACY_COMMITMENT_SPEC.md`.

The minimum acceptable next step is a deck commitment protocol that proves the
deck was not changed after room start. True ranked integrity still requires an
authoritative validator, signed event adjudication, or equivalent server-held
hidden-state design.

## Consequences

Positive:

- Friend rooms remain simple and usable without paying for a custom server.
- The product does not accidentally claim ranked integrity before the protocol
  can support it.
- The existing M8 event-envelope protocol remains reusable for future server
  validation.

Tradeoffs:

- Public/ranked features are blocked until privacy validation exists.
- Commitment-only rooms add complexity but still do not fully solve cheating.
- A future authoritative backend may be required for serious tournaments.

