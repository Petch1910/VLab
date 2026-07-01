# M27-06 Windows PlayMode Integration Spec

## Goal

Add the first Windows PlayMode integration test for the normal playable loop.

## Scope

The test covers:

- Home screen creation.
- In-memory smoke deck selection.
- PlayTable creation.
- Stand phase action through the runtime `Button`.
- Draw action through the runtime `Button`.
- Ride phase action through the runtime `Button`.
- End phase action through the runtime `Button`.

## Non-Goals

- No Android/mobile/APK verification.
- No full card browser click automation.
- No Photon live room test.
- No visual screenshot assertion.

## Verification

- Unity compile.
- Unity EditMode remains passing after the PlayMode assembly is added.
- Unity PlayMode:
  `WindowsPlayableLoopPlayModeTests.HomeToPlayTableDrawRideEndFlowHasNoRuntimeExceptions`.
