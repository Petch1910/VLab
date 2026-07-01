# M24-05 VangPro-Like Custom Import Spec Closeout

## Scope

Documented a local custom import package workflow for `.csv/.xlsx + images.zip
+ manifest/hash`.

## Changes

- Added `docs/specs/formats_and_custom/VANGPRO_LIKE_CUSTOM_IMPORT_SPEC.md`.

## Key Decisions

- Workflow uses this project's own manifest, schema, staging, hash validation,
  and import pipeline.
- No VangPro data, assets, code, exact package format, or proprietary contents
  are copied.
- `.csv` is the first supported card source.
- `.xlsx` is reserved until a parser is implemented.
- Hash mismatch blocks staging.
- Failed imports must not mutate active packs.

## Verification

- Docs-only milestone; no Unity or Python runtime code changed.
- Verified the spec file exists and is indexed in project docs.

## Next Target

Continue with `M24-06`: implement local custom import validator before any
public CGS auto-download or remote pack behavior.
