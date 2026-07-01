# Bot Legal Action / Masked State Gate Spec

Milestone: `M26-02`

## Purpose

Make the canonical bot decision boundary explicit: bots receive a masked
player-legal state view plus legal actions from `RulesCore`. They do not inspect
true opponent private zones and they do not mutate `GameState` while deciding.

## Runtime Boundary

`BotDecisionContextFactory.Create(trueState, playerIndex)` must:

- clone the true state into `GameStateViewFactory.CreatePlayerView`
- mask opponent hand, opponent deck, own unrevealed deck, and hidden event
  identities
- generate legal actions from the masked player view
- not mutate the source `GameState`

Bot decision code should choose from `BotDecisionContext.LegalActions`.
Execution still happens through `RulesCore` against the live true state.

## Current Scope

This milestone hardens the currently used heuristic/profile/playbook bot
decision boundary. It does not add new bot intelligence, ISMCTS expansion,
RL/self-play, or live card text parsing.

## Verification

- Context creation hides private information and keeps own hand visible.
- Context creation does not mutate the source state.
- Legal actions generated from the masked view remain executable on true state.
- Heuristic and simple bot decisions return legal actions without leaking
  top-deck or opponent private card ids.
- Unity compile, EditMode, Windows build, and Windows player smoke remain green.
