# M24-06 Local Custom Import Validator Closeout

## Scope

Implemented a local-only validator for VangPro-like custom import packages
before staging or runtime import.

## Changes

- Added `docs/specs/formats_and_custom/LOCAL_CUSTOM_IMPORT_VALIDATOR_SPEC.md`.
- Added `tools/data/validate_local_custom_import.py`.
- Added `tests/test_local_custom_import_validator.py`.

## Behavior

- Supports directory and `.zip` packages containing `manifest.json`.
- Validates schema `vanguard-custom-import-v1`.
- Validates safe relative paths for declared files.
- Requires SHA-256 entries for declared files and rejects mismatches.
- Accepts `cards.csv` and rejects `.xlsx` until parser support exists.
- Parses required card columns and reports card/image counts.
- Inspects `images.zip` without extracting it.
- Rejects unsafe package zip members and unsafe `images.zip` members.
- Validates optional `abilities.json` through the existing ability schema
  validator.
- Does not mutate `data/packs`, staging folders, or active runtime packs.

## Verification

- Python unit tests passed: `53/53`.
- Unity compile/EditMode were not run because this milestone changed only
  Python tooling and docs.

## Next Target

Continue with `M24-07`: Pack validation UI showing schema, set count, card
count, missing images, unsupported fields, and copyright/source-boundary note.

