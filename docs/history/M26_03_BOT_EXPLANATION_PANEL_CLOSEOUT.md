# M26-03 Bot Explanation Panel Closeout

## Status

Done.

## Scope

Added a player-readable bot explanation formatter and a read-only PlayTable
surface. This milestone does not add new bot intelligence, automatic bot
execution, ISMCTS expansion, RL/self-play, or live text/LLM effect resolution.

## Implemented

- Added `docs/specs/bot_and_headless/BOT_EXPLANATION_PANEL_SPEC.md`.
- Added `BotExplanationPanelFormatter`.
- Added formatter tests for:
  - empty bot trace placeholder
  - selected action text
  - option list limit
  - removal of raw score/playbook fields and card ids
- Added a read-only `Bot Plan` panel to PlayTable side bar.
- Added `PlayTableBootstrap.CreateBotExplanationPanel(...)` for future bot UI
  wiring.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m26_03_bot_explanation_panel.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m26_03_bot_explanation_panel.xml`
  passed `1089/1089`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m26_03_bot_explanation_panel.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m26_03_bot_explanation_panel.json`
  passed with `blockers=[]`.

## Next Target

Proceed to `M26-04`: structured ability only for templates with tests.
