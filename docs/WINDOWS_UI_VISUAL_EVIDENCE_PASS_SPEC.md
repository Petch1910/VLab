# Windows UI Visual Evidence Pass Spec

Milestone: `M31-04`

## Purpose

Review the current Windows build visually after M31 Card Workshop polish so the
next UI change is based on real screenshots/player-visible evidence, not only
functional smoke.

## Scope

- Home.
- Card Workshop / Card Browser.
- Deck Builder.
- PlayTable.
- Replay screen.

## Boundaries

- Windows only.
- No Android/mobile/APK/release work.
- No comparator asset/code/data copying.
- No gameplay rules, deck validation, or online protocol changes.

## Output

- Screenshot paths or documented visual evidence.
- A short audit closeout.
- A concrete next UI slice if any blocker/polish issue remains.

## Closeout - 2026-06-28

Implemented:

- Added a Windows runtime visual-evidence runner:
  `-vanguardVisualEvidence`
- Added output arguments:
  - `-vanguardVisualEvidenceDir`
  - `-vanguardVisualEvidenceOutput`
- Captured current Windows build screenshots for Home, Deck Builder, Card
  Browser, PlayTable, and Replay.

Captured screenshots:

- `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/home.png`
- `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/deck_builder.png`
- `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/card_browser.png`
- `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/play_table.png`
- `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/replay.png`
- Report:
  `client/unity/VanguardThaiSim/work/visual_evidence_m31_04_report.json`

Visual audit result:

- Home is now usable as a lobby-first screen.
- Card Workshop summary/status changes from M31-02/M31-03 are visible.
- Highest next issue: Card detail preview image is stretched horizontally in
  both Deck Builder and Card Browser.
- PlayTable still needs a larger board/action-density pass later.
- Replay modal is usable but visually heavy; defer until after the card preview
  image bug is fixed.

Verification:

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
- Windows visual evidence capture:
  `client/unity/VanguardThaiSim/work/visual_evidence_m31_04_report.json`
  - all 5 screenshots written
  - `issues=[]`

Next:

- `M31-05` should fix Card Browser/Deck Builder detail preview aspect ratio.
