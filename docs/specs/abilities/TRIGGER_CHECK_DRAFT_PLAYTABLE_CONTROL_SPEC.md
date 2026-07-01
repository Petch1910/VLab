# Trigger Check Draft PlayTable Control Spec

## Status

Implemented in `M10-19`.

## Purpose

Expose a narrow online PlayTable control for publishing a manual trigger-check
draft payload from the currently selected card.

This is a UI scaffold for future manual trigger entry. It uses explicit selected
card context and does not perform a real deck reveal, card movement, or trigger
effect application.

## Inputs

- selected card instance id and card id from the local PlayTable selection
- online `MultiplayerGameSessionController`
- current local player index

## Output

- online-only `DraftTrig` button
- `DraftTrig` disabled until a non-hidden selected card exists
- manual draft payload sent through
  `MultiplayerGameSessionController.PublishManualTriggerCheckDraft`
- sent payload stored in the session trigger-check payload list

## Boundary

The control must not:

- appear in local mode
- draft when no card is selected
- draft from a hidden card identity
- inspect deck order
- move cards
- apply trigger effects or modifiers to `GameState`
- append to `GameState.event_log`

## Acceptance Tests

- online `DraftTrig` exists but is disabled until a card is selected
- invoking with no selection does not send or store payloads
- selecting a card enables `DraftTrig`
- publishing creates a masked manual-resolution payload with `TriggerType.Unknown`
- local table does not create `DraftTrig`
- `GameState` is unchanged after draft publishing
- Unity compile and EditMode tests pass

## Future Extensions

- trigger type selector for manual draft publishing
- manual check source/index controls
- real RulesCore trigger-check command after reveal/move/apply rules are owned
  by the core
