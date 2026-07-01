# Public Event Masking Delivery Spec

## Status

Implemented through `M13-05` as a public-view applier and owner-private session
delivery helper. Full transport dispatch integration remains later work.

## Purpose

Commitment-only rooms must receive opponent events as masked public events, not
true `NetworkEventEnvelope` records. Applying those events must update only a
public/opponent view and must not require real opponent card ids or hidden
instance ids.

## Runtime Model

`NetworkPublicGameEventApplier.ApplyToPublicView(...)` mutates a public
`GameState` view from a `NetworkPublicGameEvent`.

Supported event types in M13-05:

- hidden private-to-private draw/move count updates
- private-to-public reveal using `public_card_id` and
  `public_card_instance_id`
- public phase update
- public Gift marker delta

`NetworkPublicGameEventApplier.ApplyToSession(...)` applies to
`LocalOwnerPrivateSession.opponent_public_view`, appends a cloned public event
to `public_event_log`, and advances `event_cursor`.

## Boundaries

- Do not mutate `LocalOwnerPrivateSession.local_true_state`.
- Do not append public events to `GameState.event_log`.
- Do not require or synthesize real opponent card ids.
- Do not dispatch this through Photon transport in M13-05.

## Verification

EditMode tests cover:

- hidden draw count update without card identity leak
- private-to-public reveal using only public identity
- owner-private session public log/cursor update without local true-state
  mutation
- invalid actor rejection without public view mutation
