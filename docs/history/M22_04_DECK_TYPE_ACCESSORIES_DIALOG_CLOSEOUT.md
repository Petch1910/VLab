# M22-04 Deck Type / Accessories Dialog Closeout

Status: Done

## Scope

`M22-04` added a player-facing Deck Type / Accessories dialog in the Windows
Deck Builder.

Implemented:

- Deck Builder `Deck Type / Accessories` button.
- Dedicated dialog separate from Deck Tools import/export.
- Summary for deck format and appearance keys.
- Format cycle: `D`, `V`, `Premium`.
- Appearance key cycles for sleeve, card back, playmat, crest, persona shield,
  gift marker, and quick shield.
- Updates stay on the active deck model.

## Guardrails

- No user asset file loading.
- No manifest/hash validation yet.
- No online payload or Photon change.
- No RulesCore or `GameState` mutation path change.
- No Android, APK, mobile QA, app packaging, or release work.

## Files

- Spec: `docs/specs/settings/DECK_TYPE_ACCESSORIES_DIALOG_SPEC.md`
- Formatter:
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/DeckAccessoriesDialogFormatter.cs`
- UI:
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/CardBrowserBootstrap.cs`
- Tests:
  `client/unity/VanguardThaiSim/Assets/Tests/EditMode/DeckAccessoriesDialogFormatterTests.cs`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_04_deck_accessories.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_04_deck_accessories.xml`
  passed `1006/1006`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m22_04_deck_accessories.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_04_deck_accessories.json`
  passed with `blockers=[]`.

## Next

Proceed to `M22-05`: explicitly verify that cosmetic metadata remains separate
from deck legality validation.
