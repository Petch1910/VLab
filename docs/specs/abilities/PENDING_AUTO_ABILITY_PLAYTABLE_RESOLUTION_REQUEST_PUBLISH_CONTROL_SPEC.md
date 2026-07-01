# Pending Auto Ability PlayTable Resolution Request Publish Control Spec

## Goal

Add an online-only PlayTable control that publishes the currently selected
pending auto ability as a resolution request intent.

This is still a synchronization scaffold. It must not resolve the ability,
mutate `GameState`, or consume the pending queue.

## UI Contract

- Online PlayTable creates a `ReqAuto` button.
- Local PlayTable does not create the button.
- `ReqAuto` is disabled until a pending auto ability selection exists.
- `ReqAuto` sends the selected resolution request through the multiplayer
  session and refreshes PlayTable status text.

## Session Contract

- `MultiplayerGameSessionController.PublishPendingAutoAbilityResolutionRequest`
  accepts a `PendingAutoAbilityResolutionRequest`.
- The session encodes the request into
  `NetworkPendingAutoAbilityResolutionRequestPayload`.
- The session sends the payload through
  `IMultiplayerTransport.SendPendingAutoAbilityResolutionRequest`.
- Successful sends are stored in
  `PendingAutoAbilityResolutionRequestPayloads` as a local publish log.
- Missing requests are rejected with
  `PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING`.

## Safety Rules

- Do not resolve card effects.
- Do not mutate `GameState` or `event_log`.
- Do not leak hidden source card identity from masked pending queues.
- Keep the payload layer separate from true gameplay event sync.

## Verification

- Unity compile passes.
- EditMode tests cover local omission, disabled no-selection state,
  enable-after-selection, request send/storage, hidden-source masking, no queue
  send, and no `GameState` mutation.
