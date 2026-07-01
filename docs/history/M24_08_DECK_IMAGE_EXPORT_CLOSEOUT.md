# M24-08 Deck Image Export Closeout

## Scope

Added first-pass Windows Deck Builder image export.

## Changes

- Added `docs/specs/deck_builder/DECK_IMAGE_EXPORT_SPEC.md`.
- Added `DeckImageExportPlanner`.
- Added `DeckImageExportPlannerTests`.
- Added `Export Image` action to Deck Builder -> Deck Tools.

## Behavior

- `Export Image` creates a PNG screenshot request under:
  `<persistentDataPath>/deck_exports/`.
- Filenames use a sanitized deck name and timestamp.
- Deck Tools closes before capture so the image shows the Deck Builder
  workspace rather than the modal.
- The export does not change deck data, pack data, RulesCore state, online
  payloads, or release/distribution artifacts.

## Verification

- Unity compile passed:
  `client/unity/VanguardThaiSim/work/unity_compile_m24_08_deck_image_export.log`.
- Unity EditMode passed `1051/1051`:
  `client/unity/VanguardThaiSim/work/unity_editmode_m24_08_deck_image_export.xml`.
- Windows build passed with no build-failed markers:
  `client/unity/VanguardThaiSim/work/windows_build_m24_08_deck_image_export.log`.
- Windows player smoke passed:
  `client/unity/VanguardThaiSim/work/player_smoke_m24_08_deck_image_export.json`
  with `blockers=[]`.

## Next Target

Continue with `M24-09`: import/custom pack workflow test rollup for parser,
validator, failed-import no-mutation, and isolated pack behavior.

