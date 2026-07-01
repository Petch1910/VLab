# CGS-Like Custom Pack Adapter Spec

Milestone: `M24-04`

## Purpose

Define a future adapter for CGS-like local custom pack sources. The adapter is a
developer/user import convenience layer only. It must transform local
user-provided files into this project's `custom_pack_v2` source shape and then
reuse the existing validator/importer pipeline.

## Hard Boundaries

- Do not auto-download public CGS data.
- Do not copy CGS repository data, assets, card images, icons, code, or UI
  files into this project.
- Do not treat CGS data as a runtime source of truth.
- Do not bypass `validate_custom_pack_schema.py`.
- Do not bypass copyright/licensing warnings in the Pack Validation UI.
- Do not execute scripts from an imported pack.
- Do not follow remote URLs during import in the Windows-first track.

## Input Shape

The adapter may accept a local folder or archive that contains a CGS-like
manifest, for example:

```text
source/
  cgs.json
  cards.json
  images/
```

Exact CGS field names are reference-only. The adapter must be tolerant but
explicit: unknown fields become warnings, not silent behavior.

## Output Shape

The adapter writes a staged custom pack source:

```text
work/custom_pack_adapter/<pack_id>/
  pack.json
  cards.csv
  abilities.json        optional
  images/               optional local files only
  adapter_report.json
```

The staged `pack.json` should use schema v2:

```json
{
  "schema_version": 2,
  "pack_id": "local_cgs_like_pack",
  "display_name": "Local CGS-Like Pack",
  "source_version": "adapter-local",
  "format": "custom",
  "language": "mixed",
  "cards_file": "cards.csv",
  "image_root": "images",
  "ruleset_profile": "custom",
  "capabilities": {
    "cards": true,
    "images": true,
    "abilities": false,
    "custom_formats": false
  },
  "dependencies": []
}
```

## Mapping Rules

Minimum card fields:

- `card_id`
- `name`
- `card_type`
- `grade`
- `clan` or `nation`
- `text`
- `power`
- `shield`
- `critical`
- `trigger_type`
- `image_relative_path`

Adapter behavior:

- Normalize card ids to stable local ids.
- Preserve source ids in an adapter metadata column/report.
- Convert missing optional fields to empty values.
- Reject missing required card ids or duplicate normalized ids.
- Copy local image files only when paths resolve inside the source root.
- Record missing image paths as warnings, not hard failures unless strict mode
  is enabled later.
- Keep ability/effect text as text unless structured ability data is provided
  in a supported schema.

## Validation Pipeline

The later implementation must run:

1. Source adapter parse.
2. Staging output generation.
3. `validate_custom_pack_schema.py` on the staged pack.
4. `import_custom_pack.py` only if validation passes.
5. Pack Validation UI displays adapter warnings and schema/import warnings.

## Report Shape

`adapter_report.json` should include:

```json
{
  "adapter": "cgs_like_v1",
  "accepted": true,
  "source_kind": "local_folder",
  "input_manifest": "cgs.json",
  "staged_pack_id": "local_cgs_like_pack",
  "card_count": 0,
  "image_count": 0,
  "warnings": [],
  "errors": []
}
```

## Non-Goals

- No direct CGS runtime reader.
- No remote URL downloader.
- No public CGS pack mirroring.
- No automatic conversion of complex scripted card logic.
- No custom format execution beyond existing schema flags.

## Acceptance For Future Implementation

- Local-only adapter CLI exists.
- Unit tests cover valid local source, unknown fields, missing required ids,
  duplicate ids, path traversal, missing local images, and validation handoff.
- Failed adapter output does not mutate active packs.
- Pack Validation UI clearly labels adapter warnings.
