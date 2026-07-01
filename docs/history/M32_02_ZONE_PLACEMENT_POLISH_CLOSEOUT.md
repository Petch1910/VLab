# M32-02 Zone Placement Polish Closeout

Date: 2026-06-28

## Scope

M32-02 continued the Vanguard Area / official digital-client PlayTable reset.
The goal was to make the Windows PlayTable read more like a card table at first
glance while keeping all gameplay actions on the existing session and RulesCore
paths.

## Public Reference Study

The current public reference capture set is stored in:

- `outputs/vanguard_video_game_ux_research/source_manifest.json`
- `outputs/vanguard_video_game_ux_research/reference_captures/`

It covers these public UX references:

- `Cardfight!! Vanguard: Ride to Victory!!`
- `Cardfight!! Vanguard: Lock on Victory!!`
- `Cardfight!! Vanguard G: Stride to Victory!!`
- `Cardfight!! Online`
- `Cardfight!! Vanguard EX`
- `Cardfight!! Vanguard Dear Days`
- `Cardfight!! Vanguard Dear Days 2`

The captures are reference evidence only. They are not asset, icon, card-data,
code, or playmat sources.

## Implementation

- Added M32 public reference coverage to
  `docs/VANGUARD_DIGITAL_CLIENT_UX_BLUEPRINT.md`.
- Added capture manifest link to `docs/INDEX.md`.
- Updated `docs/IMPLEMENTATION_PLAN.md` M32-01b notes with the expanded public
  game list and capture manifest.
- Repositioned PlayTable zones so deck, ride deck, damage, order, vanguard,
  rear-guard, soul, trigger zone, drop, bind, and gift marker areas are visible
  without relying on debug text.
- Reserved right-side space for the Inspect HUD so it no longer covers table
  zones.
- Reduced command dock height and color dominance.
- Increased hand strip height so hand cards are visible in the 1280x720
  Windows evidence capture.
- Kept toolbar button runtime names compatible with existing tests:
  `Advanced Button` and `Seat P2 Button`.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m32_02_digital_reference_ui_r3.log`
  passed with no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m32_02_digital_reference_ui_r3.xml`
  passed `1175/1175`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m32_02_digital_reference_ui_r3.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m32_02_digital_reference_ui_r3.json`
  passed with `blockers=[]`.
- Windows visual evidence:
  `client/unity/VanguardThaiSim/work/visual_evidence_m32_02_digital_reference_ui_report_r3.json`
  passed with all screenshots present.
- Latest PlayTable evidence:
  `client/unity/VanguardThaiSim/work/m32_02_digital_reference_ui_visual_evidence_r3/play_table.png`

## Notes

The visual evidence runner timed out when launched as a hidden window because
`ScreenCapture.CaptureScreenshot` needs a real rendered frame. The successful
verification used a visible 1280x720 window with `-force-d3d11`.

## Remaining Gap

M32-02 improves zone placement and hand visibility, but the right Inspect HUD is
still too tall and text-heavy. M32-03 should convert that surface into a smaller
HUD and move non-essential text into Advanced.

## Next Target

M32-03: Inspect panel text density pass.
