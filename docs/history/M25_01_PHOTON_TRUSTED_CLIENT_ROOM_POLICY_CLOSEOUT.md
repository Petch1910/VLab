# M25-01 Photon Trusted-Client Room Policy Closeout

## Scope

Locked Windows Online Room direction before usability work continues.

## Changes

- Added `docs/specs/multiplayer/PHOTON_TRUSTED_CLIENT_ROOM_POLICY_SPEC.md`.
- Added `OnlineRoomTransportPolicy`.
- Added `OnlineRoomTransportPolicyFormatter`.
- Added `OnlineRoomTransportPolicyTests`.
- Updated lobby connection status to show the policy line.

## Policy

- Selected transport remains `Photon Realtime`.
- Trust mode remains `trusted-client friend room`.
- Ranked security is not enabled.
- Custom server remains paused.
- Transport switch requires a new ADR.

## Verification

- Unity compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m25_01_photon_trusted_client_policy.log`.
- Unity EditMode passed `1054/1054`:
  `client/unity/VanguardThaiSim/work/unity_editmode_m25_01_photon_trusted_client_policy.xml`.
- Windows build passed:
  `client/unity/VanguardThaiSim/work/windows_build_m25_01_photon_trusted_client_policy.log`.
- Windows player smoke passed:
  `client/unity/VanguardThaiSim/work/player_smoke_m25_01_photon_trusted_client_policy.json`
  with `blockers=[]`.

## Next Target

Continue with `M25-02`: lobby flow create, join, ready, start, rematch, and back
home.

