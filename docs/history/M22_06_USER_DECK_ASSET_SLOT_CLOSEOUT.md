# M22-06 User Deck Asset Slot Closeout

Status: Done

## Scope

`M22-06` added safe user-provided deck accessory asset slots through manifest,
hash validation, and fallback only.

Implemented:

- `UserDeckAssetManifest`
- `UserDeckAssetSlots`
- `UserDeckAssetValidator`
- `UserDeckAssetValidationResult`
- SHA-256 validation before accepting local files
- fallback warnings for missing files and hash mismatches
- path traversal rejection for files outside the manifest root

## Guardrails

- No automatic downloads.
- No official/comparator asset extraction.
- No image loading into UI yet.
- No Deck Builder visual preview yet.
- No online payload or Photon change.
- No deck legality behavior change.
- No RulesCore or `GameState` mutation path change.
- No Android, APK, mobile QA, app packaging, or release work.

## Files

- Spec: `docs/specs/settings/USER_DECK_ASSET_SLOT_SPEC.md`
- Models / validator:
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Decks/UserDeckAssetSlots.cs`
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Decks/UserDeckAssetManifest.cs`
  `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Decks/UserDeckAssetValidation.cs`
- Tests:
  `client/unity/VanguardThaiSim/Assets/Tests/EditMode/UserDeckAssetSlotTests.cs`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_06_user_deck_assets.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_06_user_deck_assets.xml`
  passed `1014/1014`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m22_06_user_deck_assets.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_06_user_deck_assets.json`
  passed with `blockers=[]`.

## Next

Proceed to `M22-07`: test roll-up for JSON round-trip, fallback, and deck
validation unchanged.
