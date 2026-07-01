# Card Database Spec

## Current Source

`outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json`

## Current Verified Counts

- Cards: `10,836`
- Images: `10,836`
- Image type: JPEG
- Total image size: about `2.163 GiB`

## Required Fields

- `card_id`
- `name_th`
- `text_th`
- `series`
- `clan`
- `nation`
- `grade`
- `power`
- `shield`
- `trigger`
- `deck_limit`
- `image_url`
- `local_image_path`

## Runtime Storage

SQLite tables should store:

- cards
- card_details
- card_formats
- card_pack_manifest
- images metadata

Current runtime outputs:

- `data/packs/vanguard_th/manifest.json`
- `data/packs/vanguard_th/cards.sqlite`
- `data/packs/vanguard_th/card_catalog.json`
- `data/packs/vanguard_th/verification_report.json`

## Runtime Pack Resolution

The Unity runtime resolves the default pack by searching for
`data/packs/vanguard_th/manifest.json` from these roots and their ancestors:

- player/project root, including `build/windows/latest/`
- `Application.persistentDataPath`
- `Application.streamingAssetsPath`
- current working directory

This keeps editor/dev runs pointed at the workspace pack while allowing Windows
build artifacts to load the copied pack beside `VanguardThaiSim.exe`.

Android/mobile runtime uses `card_catalog.json` through `JsonCardRepository`
via `CardRepositoryFactory` because the current Android player cannot rely on
the native `Mono.Data.Sqlite` provider. Editor/Windows still prefer SQLite and
fall back to the catalog only if SQLite fails. The catalog is generated from
SQLite with:

```powershell
python tools\data\export_runtime_card_catalog.py
```

LDPlayer smoke currently provisions the runtime pack with:

```powershell
python tools\smoke\android_install_smoke.py --adb auto --push-pack --force-stop-before-launch --launch --timeout 600
```

This pushes pack metadata/database/catalog only. Full card images remain an
external 2.16 GiB dataset, so Android visual smoke shows deterministic image
fallback until image provisioning is explicitly added.

Current SQLite tables:

- `pack_manifest`
- `cards`
- `card_details`
- `card_formats`
- `card_images`
- `series`
- `clans`
- `series_clans`
- `search_terms`
- `cards_fts` when SQLite FTS5 is available

Images should stay in filesystem/cache.

## Card Filter Taxonomy

Product decision 2026-06-27: Card Browser and Deck Builder filters use
`VanguardAreaClanTaxonomy` as the active display/query layer on top of the
verified local database.

- Classic clans are ordered into familiar Vanguard Area-style nation buckets
  such as `US`, `DE`, `SG`, `DZ`, `MG`, and `Zoo`.
- D-era nations are exposed as nation filter options instead of forcing all
  D-series cards under the raw `N/A` clan bucket.
- Special/collaboration-style values remain visible as their own group.
- The taxonomy does not rewrite SQLite rows, card ids, card images, pack
  manifests, or source data.

## Search UI Feedback

`M16-07` adds `CardBrowserSearchPanelFormatter` for Thai/English no-result
messages. This is a UI feedback layer only; it does not change `search_terms`,
`cards_fts`, SQLite query semantics, or pack data.

## Image Fallback UI Feedback

`M16-08` adds deterministic UI text for broken or missing card images. The
fallback status is derived from existing metadata, filesystem checks, and
`CardImageCache` fallback detection only. It does not change image paths,
SQLite rows, pack manifests, downloaded image files, or cache eviction rules.

## Versioning

Every card pack must include:

- `pack_id`
- `source`
- `source_version`
- `card_count`
- `image_count`
- `definition_hash`
- `image_manifest_hash`
- `created_at`

## Import Rule

Never replace an existing pack silently. Import as a new version, then allow migration or selection.
