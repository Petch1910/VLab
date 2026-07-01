# M21-04b Soul Status And Resource Ledger Closeout

## Status

`M21-04b` is complete for the scoped gap fix: PlayTable Soul visibility and
ResourceLedger Soul count wiring.

`M21-04b` does not complete event-sourced ride-to-soul or live SoulBlast /
SoulCharge execution. Those require a separate command/event model change and
are split to `M21-04d`.

## Completed Changes

- PlayTable Zone Status now reports real `Soul`, `G Zone`, and `Guardian`
  counts from the existing `PlayerGameState` zones.
- PlayTable now includes a visible `Soul` zone panel in the resource row.
- `ResourceLedgerState.FromGameState` derives available soul from
  `PlayerGameState.soul`.
- Explicit `availableSoul` overrides remain supported for offline previews.
- Added EditMode tests for Soul/G Zone/Guardian status and SoulBlast resource
  ledger validation.

## Safety

- No new `GameZone` value was added; `GameZone.Soul` already existed.
- No new `PlayerGameState` zone was added; `PlayerGameState.soul` already
  existed.
- No live card movement semantics changed.
- No ride action behavior changed.
- No structured SoulBlast or SoulCharge execution changed.
- UI remains read-only and does not mutate `GameState`.

## Deferred To M21-04d

- Event-sourced ride command that moves the new card to Vanguard and the old
  Vanguard to Soul without breaking undo/replay.
- Live SoulBlast / SoulCharge execution through structured effects.
- Replay determinism and no-mutation reject tests for the new command/event
  path.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_04b_soul_status_ledger.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_04b_soul_status_ledger.xml`
  passed `920/920`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_04b_soul_status_ledger.log`
  reports `Windows build artifact result: Succeeded`, `errors=0`,
  `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_04b_soul_status_ledger.json`
  reports four steps and `blockers=[]`.

## Next Target

`M21-04d`: add the event-sourced ride/Soul resource command path before board
thumbnail polish, because this is a core correctness gap rather than a visual
polish task.
