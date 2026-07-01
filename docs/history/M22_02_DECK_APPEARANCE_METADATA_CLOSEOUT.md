# M22-02 DeckAppearanceMetadata Closeout

Status: Done

## Scope

`M22-02` added the pure deck appearance metadata model for Windows-first deck
accessories.

Implemented fields:

- `sleeve_key`
- `card_back_key`
- `playmat_key`
- `crest_key`
- `persona_shield_key`
- `gift_marker_key`
- `quick_shield_key`

## Guardrails

- Model-only work; no Deck Builder accessory UI yet.
- No user asset manifest loading yet.
- No filesystem path or URL is accepted as an appearance key.
- No online payload or Photon change.
- No deck legality behavior change.
- Existing `DeckCosmetics` remains for legacy/backward-compatible deck data.
- No RulesCore or `GameState` mutation path change.
- No Android, APK, mobile QA, app packaging, or release work.

## Files

- Spec: `docs/specs/settings/DECK_APPEARANCE_METADATA_SPEC.md`
- Model:
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Decks/DeckAppearanceMetadata.cs`
- Integration:
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Decks/VanguardDeck.cs`
- Tests:
  `client/unity/VanguardThaiSim/Assets/Tests/EditMode/DeckAppearanceMetadataTests.cs`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_02_deck_appearance.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_02_deck_appearance.xml`
  passed `997/997`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m22_02_deck_appearance.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_02_deck_appearance.json`
  passed with `blockers=[]`.

## Next

Proceed to `M22-03`: add the Settings screen from Home. It should consume the
local `PlayerSettings` model without changing RulesCore, deck validation,
online payloads, Android, or release workflows.
