# Broken Image Fallback Spec

## Status

Implemented in `M16-08`.

## Purpose

Make missing or unreadable card images visible to the user while preserving the
existing `CardImageCache` fallback texture behavior.

## Scope

`M16-08` adds:

- a read-only way to detect when `CardImageCache` returned its fallback texture
- a pure UI formatter for tile/detail fallback text
- Card Browser tile/detail wiring for missing-image status

## Boundaries

This milestone must not:

- move downloaded card images
- edit runtime pack data
- edit SQLite image metadata
- change image cache eviction policy
- store image binary data in SQLite
- touch deck data or `GameState`

## Output Shape

Tile label:

```text
<card name>
[image fallback]
```

Detail status:

```text
Image: fallback (missing path)
```

or:

```text
Image: fallback (file missing or unreadable)
```

No fallback status is shown for cards whose real image loaded.

## Acceptance Tests

- missing path reports fallback status
- missing file/unreadable load reports fallback status
- normal loaded image reports no fallback status
- tile label appends `[image fallback]` only when needed
- `CardImageCache` can identify its fallback texture
- existing cache clear/eviction behavior remains unchanged
