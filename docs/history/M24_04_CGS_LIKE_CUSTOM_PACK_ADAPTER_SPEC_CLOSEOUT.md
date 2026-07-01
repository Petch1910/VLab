# M24-04 CGS-Like Custom Pack Adapter Spec Closeout

## Scope

Documented the future CGS-like custom pack adapter approach without implementing
import code yet.

## Changes

- Added `docs/specs/formats_and_custom/CGS_LIKE_CUSTOM_PACK_ADAPTER_SPEC.md`.

## Key Decisions

- Adapter is local-only.
- No auto-download of public CGS data.
- No copying CGS repository data/assets/code into this project.
- Adapter output must stage into this project's `custom_pack_v2` source shape.
- Existing validation/import tools remain the source of truth.
- Unknown fields become warnings, not hidden behavior.

## Verification

- Docs-only milestone; no Unity or Python runtime code changed.
- Verified the spec file exists and is indexed in project docs.

## Next Target

Continue with `M24-05`: VangPro-like custom import spec for `.csv/.xlsx +
images.zip + manifest/hash`.
