# M22-03 Home Settings Screen Closeout

Status: Done

## Scope

`M22-03` replaced the locked Home `Settings` action with a real Windows Home
settings screen.

Implemented:

- Home `Settings` button opens `Settings Screen`.
- Settings summary renders normalized `PlayerSettings`.
- Preferred format cycles through `D`, `V`, and `Premium`.
- Image cache mode cycles through `Balanced`, `MemorySaver`, and `HighQuality`.
- `Close` hides the settings screen and returns to Home.

## Guardrails

- Settings are session-local only in this milestone.
- No filesystem persistence yet.
- No deck accessory dialog yet.
- No online payload or Photon change.
- No deck legality behavior change.
- No RulesCore or `GameState` mutation path change.
- No Android, APK, mobile QA, app packaging, or release work.

## Files

- Spec: `docs/specs/settings/HOME_SETTINGS_SCREEN_SPEC.md`
- Formatter:
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/HomeSettingsPanelFormatter.cs`
- UI:
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/HomeLobbyBootstrap.cs`
- Tests:
  `client/unity/VanguardThaiSim/Assets/Tests/EditMode/HomeSettingsPanelFormatterTests.cs`
  `client/unity/VanguardThaiSim/Assets/Tests/EditMode/HomeLobbyBootstrapTests.cs`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_03_home_settings.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_03_home_settings.xml`
  passed `1002/1002`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m22_03_home_settings.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_03_home_settings.json`
  passed with `blockers=[]`.

## Next

Proceed to `M22-04`: add the Deck Type / Accessories dialog in Deck Builder.
It should edit deck-level appearance metadata without changing deck legality.
