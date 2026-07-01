# M22-01 PlayerSettings Closeout

Status: Done

## Scope

`M22-01` added the pure local `PlayerSettings` model for Windows-first player
preferences.

Implemented fields:

- `player_name`
- `default_deck_id`
- `preferred_format`
- `ui_scale`
- `image_cache_mode`

## Guardrails

- Model-only work; no Settings UI yet.
- No filesystem persistence yet.
- No online payload or Photon change.
- No deck legality behavior change.
- No RulesCore or `GameState` mutation path change.
- No Android, APK, mobile QA, app packaging, or release work.

## Files

- Spec: `docs/specs/settings/PLAYER_SETTINGS_SPEC.md`
- Model:
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Settings/PlayerSettings.cs`
- Tests:
  `client/unity/VanguardThaiSim/Assets/Tests/EditMode/PlayerSettingsTests.cs`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_01_player_settings.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_01_player_settings.xml`
  passed `988/988`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m22_01_player_settings.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_01_player_settings.json`
  passed with `blockers=[]`.

## Next

Proceed to `M22-02`: add `DeckAppearanceMetadata` for sleeve/card back,
playmat key, crest/persona shield/marker options as pure cosmetic metadata.
Keep it separate from deck legality validation.
