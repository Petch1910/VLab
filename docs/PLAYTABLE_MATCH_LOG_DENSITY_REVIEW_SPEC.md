# PlayTable Match Log / Preview Density Review Spec

Milestone: `M28-10`

## Purpose

Reduce the remaining primary side-panel density after M28-09 without removing
important manual-play information.

## Decision

- Keep `Selected Card Preview` in the primary side panel.
- Change the primary `Match log` to a compact summary.
- Move the fuller match log into the hidden-by-default Advanced drawer.

This follows the M28-08 audit: card preview is useful during manual play, while
the large full match log competes with guidance and preview space.

## Runtime Contract

Primary side panel:

- Shows `PlayTable Compact Match Log`.
- Uses the latest 3 events.
- Shows total event count, latest event, recent events, and a hint that the full
  log is in Advanced.
- Does not expose raw enum names or private card instance ids.
- Keeps `Selected Card Preview` visible.

Advanced drawer:

- Shows `PlayTable Full Match Log`.
- Keeps the larger existing event/replay panel outside the primary player flow.

## Non-Goals

- No RulesCore changes.
- No event model changes.
- No replay protocol changes.
- No network payload changes.
- No card-preview rewrite.

## Verification Plan

- Formatter tests for empty, full, and compact match log output.
- Runtime UI test proving compact log is primary, full log is under Advanced,
  and compact log height is bounded.
- Unity compile and EditMode tests.
- Editor client smoke and Windows player smoke if runtime UI passes.

## Closeout Result

M28-10 is complete.

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_10_match_log_density.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_10_match_log_density.xml`
  passed `1151/1151`.
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_10_match_log_density.log`
  reported `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m28_10_match_log_density.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m28_10_match_log_density.json`
  reported `blockers=[]`.
