# Photon Trusted-Client Room Policy Spec

Milestone: `M25-01`

## Purpose

Lock the Windows Online Room direction before usability work continues:

- keep Photon Realtime as the selected transport
- keep the current mode as trusted-client friend/manual rooms
- do not switch to Unity Netcode, UGS, Photon Fusion, or a custom server without
  a new ADR

## Scope

M25-01 is a policy/status milestone. It should not rewrite room flow,
reconnect, payload codecs, Photon event codes, or gameplay sync.

## Required Runtime Surface

The lobby status should keep a player-facing policy line visible:

```text
Policy: Photon Realtime / trusted-client friend room / no ranked security
```

## Boundaries

- No transport switch.
- No custom server.
- No ranked/public integrity promise.
- No mobile/Android/release packaging.
- No hidden-state behavior change.

## Verification

EditMode tests should cover:

- selected transport is `Photon Realtime`
- trust mode remains `trusted-client friend room`
- policy requires an ADR before transport switch
- lobby connection formatter includes the policy line

