# Owner-Private Room Initialization Spec

## Status

Implemented through `M13-01` as a local owner-private initialization model.
`M13-02` explicit client-trust UX is implemented. Normal `deck_commitment`
gameplay remains blocked until later public-event state application/reconnect
milestones are complete.

## Purpose

Define how a client-authoritative commitment-only room can initialize gameplay
without giving either player the opponent's deck list or hidden card instance
ids.

This is not ranked-grade anti-cheat. It is a casual privacy mode that still
requires a client-trust warning and end-of-match deck reveal verification.

## Current Blocker

The current playable online path builds one shared true `GameState` from both
players' deck codes. That works for friend rooms, but it is not valid for
`deck_commitment` rooms because the opponent deck code is intentionally hidden.

The public-event payload, public replay, public delivery, reveal proof, and deck
reveal flow now exist. What is still missing is the initial local state model.

## Required State Split

Each client needs two state surfaces:

- owner true state: full local deck, hand, ride deck, and private choices
- opponent public state: public zones plus private-zone counts and synthetic
  hidden cards only

Do not create a fake full opponent deck with real card ids.

## Proposed Runtime Model

```text
LocalOwnerPrivateSession
  local_true_state
  opponent_public_view
  public_event_log
  local_private_event_log
```

The local player executes legal actions against `local_true_state`.

The opponent's actions are received as `NetworkPublicGameEvent` records and
update only public counts, public zones, and replay/audit logs.

## Initialization Inputs

From room state:

- room id
- random seed
- pack definition hash
- player ids
- each player's deck commitment metadata
- public count metadata:
  - `main_deck_count`
  - `ride_deck_count`
  - `g_deck_count`
  - `opening_hand_count`

From local user:

- local `VanguardDeck`
- local private reveal nonce

The opponent contributes no deck code during initialization.

## M13-01 Implementation

Runtime types:

- `LocalOwnerPrivateSession`
- `OwnerPrivateRoomInitializationResult`
- `OwnerPrivateRoomInitializer`

`OwnerPrivateRoomInitializer.Create(...)` accepts:

- `MultiplayerRoomState`
- local `VanguardDeck`
- local player id
- local reveal nonce

It validates that:

- room privacy mode is `deck_commitment`
- room id, random seed, and pack definition hash exist
- local and opponent players are connected and use
  `DeckCommitmentService.Algorithm`
- neither player exposes `deck_code`
- the local deck plus local reveal nonce matches the announced commitment
- opponent public count metadata is present

It creates:

- `local_true_state`: true local deck, hand, ride deck, and synthetic hidden
  opponent placeholders
- `opponent_public_view`: masked local player view produced from the local true
  state
- empty `public_event_log`
- empty `local_private_event_log`
- `event_cursor = 0`
- `reconnect_enabled = false`

The initializer does not mutate `GameState`, `MultiplayerRoomState`, or
`VanguardDeck` inputs. It does not unblock normal commitment-only gameplay.

## Local Player Setup

1. Verify local deck creates the announced commitment.
2. Create true local zones from the local deck and room seed.
3. Create opponent public placeholder zones:
   - deck count only
   - hand count only after public draw/mulligan info exists
   - no real opponent `card_id`
   - no real opponent `card_instance_id`
4. Start event cursor at zero.

## Public Event Application

Private-to-private opponent events:

- update only public count deltas
- do not create real card identities

Private-to-public opponent events:

- add public card identity from `NetworkPublicGameEvent.public_card_id`
- use `public_card_instance_id`
- verify `reveal_proof` when commitment context exists

Public-to-public opponent events:

- move public synthetic/public instances between public zones

## Local Event Publishing

When local true actions resolve:

1. apply through `RulesCore` to local true state
2. generate `NetworkPublicGameEvent` from pre-event true state
3. attach reveal proof when a local private card becomes public
4. send public event to opponent/spectator
5. keep local private event in local audit log

Do not send true `NetworkEventEnvelope` to the opponent in this mode.

## Reconnect

Commitment-only reconnect cannot use true `NetworkEventBatch`.

It needs:

- public event replay from event cursor
- local owner-private restore from local save/snapshot
- end-of-match reveal verification for audit

Until this exists, commitment-only reconnect must stay blocked.

## Acceptance Criteria Before Gameplay Unlock

- local player can initialize from own deck plus both commitments
- opponent starts as public/hidden placeholders only
- local actions publish public events without true card id leakage
- incoming public events update public view without requiring true opponent ids
- public replay can rebuild public view
- reconnect has a public-event path or is explicitly disabled
- UI shows client-trust warning before starting commitment-only rooms
- end-of-match deck reveal verifies both decks or reports a clear failure

## Non-Goals

- ranked integrity
- server-held hidden-state validation
- automatic trust in opponent hidden actions
- LLM/rule text resolution

## Decision

Keep normal `deck_commitment` gameplay blocked until this owner-private
initialization model is paired with explicit client-trust UX and public-event
state application/reconnect paths.
