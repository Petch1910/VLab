# M22-07 Settings / Accessories Test Roll-up Closeout

Status: Done

## Scope

`M22-07` closes the test requirements for M22 Settings / Deck Type /
Accessories.

Required coverage:

- JSON round-trip.
- fallback behavior.
- deck validation unchanged by cosmetic metadata.

## Evidence

JSON round-trip coverage:

- `PlayerSettingsTests.JsonRoundTripPreservesValidSettings`
- `DeckAppearanceMetadataTests.JsonRoundTripPreservesValidAppearanceKeys`
- `DeckAppearanceMetadataTests.DeckCodeRoundTripPreservesAppearanceMetadata`
- `UserDeckAssetSlotTests.ManifestRoundTripsAssetEntries`
- `UserDeckAssetSlotTests.ExistingFileWithMatchingHashResolvesAsset`

Fallback coverage:

- `PlayerSettingsTests.FromJsonFallsBackForEmptyOrInvalidJson`
- `DeckAppearanceMetadataTests.FromJsonFallsBackForEmptyOrInvalidJson`
- `DeckAppearanceMetadataTests.NormalizeTrimsSafeKeysAndFallbacksUnsafeKeys`
- `UserDeckAssetSlotTests.MissingFileFallsBackWithoutRejectingManifest`
- `UserDeckAssetSlotTests.HashMismatchFallsBackWithoutAcceptingAsset`
- `UserDeckAssetSlotTests.UnknownSlotIsWarningAndUsesFallback`

Deck validation unchanged coverage:

- `DeckAppearanceMetadataTests.AppearanceMetadataDoesNotAffectDeckLegality`
- `DeckCosmeticLegalitySeparationTests.CosmeticMetadataDoesNotChangeDeckValidationResult`
- `DeckCosmeticLegalitySeparationTests.DeckValidatorDoesNotReferenceCosmeticModels`

## Verification

Latest verification for this roll-up is the `M22-06` run:

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

No new runtime code was added in `M22-07`.

## Next

Proceed to `M22-08`: close out M22 and set the next target to `M23` In-App
Manual / Tutorial.
