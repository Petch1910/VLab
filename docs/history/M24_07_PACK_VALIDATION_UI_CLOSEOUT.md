# M24-07 Pack Validation UI Closeout

## Scope

Added a read-only Pack Check surface to Windows Deck Builder -> Deck Tools.

## Changes

- Added `docs/specs/formats_and_custom/PACK_VALIDATION_UI_SPEC.md`.
- Added `PackValidationUiFormatter` and `LocalCustomImportValidationReport`.
- Added `PackValidationUiFormatterTests`.
- Added `Pack Check` button to Deck Tools.
- Expanded Deck Tools status area for validation text.

## Behavior

- If Deck Tools input contains `validate_local_custom_import.py --json` output,
  Pack Check shows local custom import validation status.
- If Deck Tools input is empty or not a local report, Pack Check shows active
  runtime pack validation status.
- The display includes pack id, schema/adapter, set/card/image counts, missing
  image count, ability count, unsupported field count for local reports, bounded
  warnings/errors, and the source-boundary note.
- The UI does not run Python, download data, extract assets, import packs,
  mutate decks, mutate runtime packs, or touch `GameState`.

## Verification

- Unity compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m24_07_pack_validation_ui.log`.
- Unity EditMode passed `1047/1047`:
  `client/unity/VanguardThaiSim/work/unity_editmode_m24_07_pack_validation_ui.xml`.
- Windows build passed with `errors=0`, `warnings=0`:
  `client/unity/VanguardThaiSim/work/windows_build_m24_07_pack_validation_ui.log`.
- Windows player smoke wrote:
  `client/unity/VanguardThaiSim/work/player_smoke_m24_07_pack_validation_ui.json`
  with `blockers=[]`.

## Next Target

Continue with `M24-08`: Deck image export.

