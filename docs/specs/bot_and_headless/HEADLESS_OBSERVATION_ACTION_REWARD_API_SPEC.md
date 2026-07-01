# Headless Observation / Action / Reward API Spec

## Milestone

`M17-06`

## Goal

Define a bounded research-facing contract for observation, legal-action mask,
and reward signals. This is a data boundary for future bot/research work, not a
training loop and not a packed-state optimization.

## API

`HeadlessObservationActionRewardApi.CreateObservation(state, playerIndex, ruleset, seed)`

- creates a player-view observation from `GameState`
- uses `GameStateViewFactory.CreatePlayerView`
- uses `RulesCore.GetLegalActions` on the masked player view
- does not mutate source `GameState`
- returns zone counts, phase, turn metadata, event count, and sanitized legal
  action entries

`HeadlessObservationActionRewardApi.CreateReward(result)`

- returns reward model `m17_06_smoke_acceptance_v1`
- accepted smoke result: non-terminal reward `0`
- rejected/missing result: terminal reward `-1`
- this is not a win/loss reward model

`HeadlessObservationActionRewardApi.CreateSample(...)`

- combines one observation with one reward signal for fixture/export tests

## Hidden-State Policy

The observation/action mask must not include:

- `card_instance_id`
- `card_id`
- card names or labels from legal actions
- opponent hand contents
- deck order
- full replay events

Action mask entries use sanitized action metadata:

- `index`
- `action_id`
- `action_type`
- `actor_index`
- `requires_card_selection`
- `from_zone`
- `to_zone`
- `phase`
- `gift_marker_type`
- `resource_operation_type`

The action mask is a read-only contract. Future execution mapping must keep
private selectors inside the engine, not in public dataset JSON.

## Non-Goals

- No RL framework integration.
- No self-play learning.
- No packed/flat state migration.
- No network payload publishing.
- No UI surface.
- No win/loss reward shaping yet.

## Verification

- Observation creation is deterministic for the same state.
- Observation creation does not mutate source state.
- Observation/sample JSON does not contain card ids or card instance ids.
- Reward output clearly reports the smoke acceptance model.
