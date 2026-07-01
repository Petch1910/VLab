# Pending Auto Ability Manual Resolution Decision PlayTable Publish Control Spec

## Status

Implemented in `M10-80`.

## Purpose

Add an online PlayTable control that republishes the latest stored pending AUTO
ability manual resolution decision payload.

This control only publishes an existing payload through the session helper. It
does not create decisions from card text, resolve effects, or mutate game
state.

## Behavior

- Online PlayTable creates a `DecAuto` button.
- Local PlayTable does not create the `DecAuto` button.
- The button is disabled until the session has at least one stored manual
  resolution decision payload.
- Clicking the button calls
  `PublishLatestPendingAutoAbilityManualResolutionDecision`.
- Success/failure is surfaced through the existing selection/status message.

## Boundary

This milestone must not:

- mutate `GameState`
- resolve, skip, or defer pending abilities
- create a new decision payload from UI selection
- add decision summary/list surfaces

## Acceptance Tests

- local PlayTable omits the decision publish control
- online PlayTable disables the control when no decision payload exists
- online PlayTable publishes the latest stored decision payload
- control does not mutate `GameState`
