# M32-03 PlayTable De-Dashboard Field/HUD Closeout

Date: 2026-06-28

## Scope

M32-03 moved the PlayTable default view further away from a developer dashboard and closer to a Vanguard Area / digital-client table surface.

Story, character portraits, dialogue, campaign UI, proprietary icons, playmats, card art, code, packed content, and extracted commercial game data remained out of scope.

## Changes

- The main table no longer reserves width for a permanent full-height side dashboard.
- The default Inspect surface is now a compact right HUD overlay.
- Recent/full logs, setup detail, zone status, bot, online, trigger, pending AUTO, ability diagnostics, and compact log surfaces remain hidden in Advanced by default.
- Right-side field zones now reserve an inspect-HUD gutter so Trigger, Drop, Bind, and Gift Marker zones do not sit under the HUD.
- Gift marker counts use compact player-facing text (`F:0 A:0 P:0`) so they do not overflow into the Trigger Zone.
- Tests now lock the de-dashboard log placement and right-field gutter budget.

## Verification

- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m32_03_de_dashboard_field_hud_rerun5.xml`
  passed `1178/1178`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m32_03_de_dashboard_field_hud_r3.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m32_03_de_dashboard_field_hud_r3.json`
  passed with `blockers=[]`.
- Windows visual evidence:
  `client/unity/VanguardThaiSim/work/visual_evidence_m32_03_de_dashboard_field_hud_report_r4.json`
  passed with no issues.
- PlayTable screenshot:
  `client/unity/VanguardThaiSim/work/m32_03_de_dashboard_field_hud_visual_evidence_r4/play_table.png`.

## Next Target

`M32-04`: Windows visual evidence comparison and closeout.
