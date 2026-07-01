# Unity Data Access Contract

เอกสารนี้กำหนด contract สำหรับงาน `M2-01: Card data access layer` เมื่อเริ่มทำ Unity project

## Runtime Pack

Unity client ต้องอ่าน pack จาก:

- Manifest: `data/packs/vanguard_th/manifest.json`
- SQLite: `data/packs/vanguard_th/cards.sqlite`
- JSON catalog fallback: `data/packs/vanguard_th/card_catalog.json`
- Image root: ค่าจาก `manifest.json` field `image_root`

ห้ามอ่านจาก `outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json` ใน runtime ปกติ เพราะไฟล์ JSON ใหญ่และเป็น source/export layer ไม่ใช่ runtime layer

`card_catalog.json` is a runtime artifact exported from `cards.sqlite` by
`tools/data/export_runtime_card_catalog.py`. It is the Android/mobile fallback
when the native SQLite provider is unavailable. It is not the raw source export
JSON.

## Required Manifest Fields

Unity ต้องอ่าน field เหล่านี้:

- `pack_id`
- `display_name`
- `schema_version`
- `source`
- `source_version`
- `card_count`
- `image_count`
- `existing_image_count`
- `series_count`
- `clan_count`
- `definition_hash`
- `image_manifest_hash`
- `image_root`
- `sqlite_file`
- `sqlite_fts_enabled`
- `catalog_file` (optional, default `card_catalog.json`)
- `catalog_schema_version` (optional, current `1`)

ถ้า `schema_version` ไม่ใช่ `1` ให้ client แสดง error ว่า pack version ยังไม่รองรับ

## Card Summary Model

ใช้สำหรับ card grid/search/filter:

```text
CardSummary
  cardId: string
  nameTh: string?
  series: string
  seriesCode: string?
  clan: string
  nation: string?
  grade: int?
  type1: string?
  trigger: string?
  imageRelativePath: string
  imageExists: bool
```

Query:

```sql
SELECT
  c.card_id,
  c.name_th,
  c.series,
  c.series_code,
  c.clan,
  c.nation,
  c.grade,
  c.type_1,
  c.trigger,
  i.image_relative_path,
  i.image_exists
FROM cards c
JOIN card_images i ON i.card_id = c.card_id
ORDER BY c.card_id
LIMIT @limit OFFSET @offset;
```

## Card Detail Model

ใช้สำหรับหน้า detail:

```text
CardDetail
  cardId: string
  sourceId: string
  sourceKey: string?
  nameTh: string?
  textTh: string?
  series: string
  seriesCode: string?
  clan: string
  nation: string?
  nation2: string?
  grade: int?
  power: int?
  shield: int?
  trigger: string?
  deckLimit: int
  type1: string?
  type2: string?
  race1: string?
  race2: string?
  warning: string?
  imageUrl: string
  imageRelativePath: string
  rawDetails: CardRawDetail[]
  formats: CardFormat[]
```

Main query:

```sql
SELECT
  c.card_id,
  c.source_id,
  c.source_key,
  c.name_th,
  c.text_th,
  c.series,
  c.series_code,
  c.clan,
  c.nation,
  c.nation_2,
  c.grade,
  c.power,
  c.shield,
  c.trigger,
  c.deck_limit,
  c.type_1,
  c.type_2,
  c.race_1,
  c.race_2,
  c.warning,
  i.image_url,
  i.image_relative_path,
  i.image_exists
FROM cards c
JOIN card_images i ON i.card_id = c.card_id
WHERE c.card_id = @cardId;
```

Details query:

```sql
SELECT label, value
FROM card_details
WHERE card_id = @cardId
ORDER BY sort_order;
```

Formats query:

```sql
SELECT format_key, format_value
FROM card_formats
WHERE card_id = @cardId
ORDER BY format_key;
```

## Repository Selection

- Editor/Windows uses SQLite first and falls back to JSON catalog only if
  SQLite fails and the catalog exists.
- Android player uses JSON catalog first to avoid native SQLite provider
  failures on emulator/device builds.
- `CardRepositoryFactory` owns this selection. UI, bot, smoke, and deck
  validation paths should consume `ICardRepository` instead of constructing
  repositories directly.

## Search

ถ้า `manifest.sqlite_fts_enabled = true` ให้ใช้ `cards_fts`:

```sql
SELECT c.card_id, c.name_th, c.series_code, c.clan, c.grade, c.type_1, i.image_relative_path
FROM cards_fts f
JOIN cards c ON c.card_id = f.card_id
JOIN card_images i ON i.card_id = c.card_id
WHERE cards_fts MATCH @query
ORDER BY rank
LIMIT @limit OFFSET @offset;
```

ถ้า FTS ใช้ไม่ได้ ให้ fallback ไปที่ `search_terms LIKE`:

```sql
SELECT c.card_id, c.name_th, c.series_code, c.clan, c.grade, c.type_1, i.image_relative_path
FROM search_terms s
JOIN cards c ON c.card_id = s.card_id
JOIN card_images i ON i.card_id = c.card_id
WHERE s.text LIKE @pattern
ORDER BY c.card_id
LIMIT @limit OFFSET @offset;
```

## Filters

Series list:

```sql
SELECT series_code, series, card_count
FROM series
ORDER BY series_code;
```

Clan list:

```sql
SELECT clan, card_count
FROM clans
ORDER BY card_count DESC, clan;
```

Nation list for UI taxonomy:

```sql
SELECT TRIM(nation) AS nation, COUNT(*) AS card_count
FROM cards
WHERE nation IS NOT NULL AND TRIM(nation) <> ''
GROUP BY TRIM(nation)
ORDER BY card_count DESC, nation;
```

Filtered card grid:

```sql
SELECT c.card_id, c.name_th, c.series_code, c.clan, c.grade, c.type_1, i.image_relative_path
FROM cards c
JOIN card_images i ON i.card_id = c.card_id
WHERE (@series IS NULL OR c.series = @series)
  AND (@clan IS NULL OR c.clan = @clan)
  AND (@nation IS NULL OR TRIM(c.nation) = @nation)
  AND (@grade IS NULL OR c.grade = @grade)
  AND (@type1 IS NULL OR c.type_1 = @type1)
ORDER BY c.card_id
LIMIT @limit OFFSET @offset;
```

## Image Path Resolution

Runtime image path:

```text
absoluteImagePath = projectRoot / manifest.image_root / card.imageRelativePath
```

ใน build จริงควรย้าย pack ไปอยู่ใน persistent data หรือ streaming assets ตาม platform:

- Windows dev: อ่านจาก workspace path ได้
- Windows release: copy pack ไปข้าง executable หรือ user data
- Android: ใช้ thumbnail/cache strategy ไม่ควรยัดรูปทั้งหมดเข้า memory

## Minimum Acceptance For M2-01

- อ่าน `manifest.json` ได้
- เปิด `cards.sqlite` ได้
- query card count ได้ `10,836`
- query `BT01-001TH` ได้ชื่อ `ราชาแห่งอัศวิน, อัลเฟรด`
- query series count ได้ `206`
- query clan count ได้ `33`
- resolve image path ของ `BT01-001TH` แล้วไฟล์มีอยู่จริง
- มี error ที่อ่านง่ายเมื่อ pack หาย, schema version ไม่รองรับ, หรือ SQLite เปิดไม่ได้
