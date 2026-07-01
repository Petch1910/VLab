# M31-04 Windows UI Visual Evidence Pass Closeout

Date: 2026-06-28

## Scope

Capture and review current Windows build screenshots after M31 Card Workshop
polish, then choose the next UI slice from player-visible evidence.

## Implemented

- Added runtime visual evidence capture flag:
  `-vanguardVisualEvidence`
- Added output arguments:
  - `-vanguardVisualEvidenceDir`
  - `-vanguardVisualEvidenceOutput`
- The runner captures:
  - Home
  - Deck Builder
  - Card Browser
  - PlayTable
  - Replay
- Added EditMode tests for visual evidence flag/output resolution.

## Evidence

- Report:
  `client/unity/VanguardThaiSim/work/visual_evidence_m31_04_report.json`
- Screenshots:
  - `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/home.png`
  - `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/deck_builder.png`
  - `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/card_browser.png`
  - `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/play_table.png`
  - `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/replay.png`

## Findings

### Passed

- Home now reads as a usable lobby-first screen.
- Deck Builder/Card Browser show the M31 readiness summary.
- Card Workshop toolbar status is shorter than before.
- Replay screen is reachable and visible.

### Next-Fix Issue

- Deck Builder and Card Browser detail preview images are visibly stretched
  horizontally. The grid thumbnails are acceptable, so the problem is scoped to
  the selected-card detail preview surface.

### Later Polish

- PlayTable still needs a separate board/action-density pass.
- Replay modal is functionally usable but visually heavy.
- Home locked/secondary actions could be styled more clearly later.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m31_04_visual_evidence_r2.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m31_04_visual_evidence_r2.xml`
  - `1171/1171` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m31_04_visual_evidence.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m31_04_visual_evidence_r2.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m31_04_visual_evidence.json`
  - `blockers=[]`
- Visual evidence capture:
  `client/unity/VanguardThaiSim/work/visual_evidence_m31_04_report.json`
  - all screenshots exist
  - `issues=[]`

## Decision

Next target: `M31-05` Card detail preview aspect-ratio fix.
