# M28-09 Bot Plan Advanced Drawer Closeout

## Scope

`M28-09` moves Bot Plan out of the primary manual PlayTable side panel and into
the Advanced drawer. This keeps the manual table focused on player flow while
preserving bot explanation functionality for future bot work.

## Implementation

- Moved `PlayTable Bot Explanation` under `Advanced Drawer`.
- Kept `BotExplanationPanelFormatter` and
  `PlayTableBootstrap.CreateBotExplanationPanel(...)` unchanged.
- Added EditMode coverage that verifies the Bot Plan surface is parented under
  the Advanced drawer.
- Did not change RulesCore, command execution, legal-action gating, bot
  decision logic, or online transport.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_09_bot_plan_advanced.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_09_bot_plan_advanced.xml`
  passed `1138/1138`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_09_bot_plan_advanced.xml`
  passed `2/2`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_09_bot_plan_advanced.log`
  passed with `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m28_09_bot_plan_advanced.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m28_09_bot_plan_advanced.json`
  passed with `blockers=[]`.

## Result

`M28-09` is complete. The visible manual PlayTable side panel has less bot/debug
noise while Advanced still preserves Bot Plan access.

## Next Target

`M28-10` should review the remaining side-panel density around Match Log and
Selected Card Preview. Prefer a compact log summary or visual QA audit before
any larger UI rewrite.
