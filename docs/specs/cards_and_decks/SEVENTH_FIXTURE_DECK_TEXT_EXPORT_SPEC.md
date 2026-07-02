# Seventh Fixture Deck Text Export Spec

Milestone: `M62-02`

## Purpose

`M62-02` exports the seventh offline runtime/test fixture into
human-reviewable count-line deck text.

The exporter is scaffold-safe: tests may build the seventh fixture and M62-01
validation report in memory, while the real CLI artifacts remain gated until
the M61-06 fixture and M62-01 validation report files exist.

## Inputs

- `outputs/target_slice/runtime_fixtures/m60_recipe_001_neo_nectar_m61_06.json`
- `outputs/target_slice/m62_01_seventh_fixture_schema_validation.json`
- `data/packs/vanguard_th/cards.sqlite`
- `data/packs/vanguard_th/manifest.json`

## Outputs

- `outputs/target_slice/m62_02_seventh_fixture_deck_text_export.txt`
- `outputs/target_slice/m62_02_seventh_fixture_deck_text_export.json`
- `outputs/target_slice/m62_02_seventh_fixture_deck_text_export.md`

These outputs are written only when the CLI is run against existing M61-06 and
M62-01 real artifacts.

## Text Format

The text export follows the existing `CountLineDeckCodec` shape:

```text
# Vanguard Thai Sim Deck List
Name: G Series Neo Nectar Fixture (m60_recipe_001)
Format: g_series_first
PackId: vanguard_th
PackVersion: 251
PackDefinitionHash: <current pack definition hash>

[Main]
4 G-BTxx-xxxTH

[Ride]

[G]
# Empty: G Zone and Stride runtime are disabled for this fixture.
# Note: Bloom/token runtime is disabled; Bloom/token text remains review-only.
```

Review comments are allowed because the codec ignores comment lines beginning
with `#`. The exporter may include `# Card: ...` comments above card lines to
make the text easy to inspect without breaking import compatibility.

## Gate Rules

The exporter must block when:

- the `M62-01` validation report is missing or not schema-valid
- the fixture is not ready for `M62-02`
- a fixture card id is missing from SQLite
- a fixture main deck row is malformed

## Boundary

This milestone must not:

- mutate the fixture artifact
- add or replace saved player decks
- publish the fixture to UI deck selection
- enable bot playbook behavior
- enable G Zone runtime
- enable Stride runtime
- enable Bloom/token runtime
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_seventh_fixture_deck_text_export
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M61-06 and M62-01 outputs exist:

```powershell
python tools\deck\export_seventh_fixture_deck_text.py
```

## Done Rule

`M62-02` scaffold work is ready when the exporter and tests produce
review-only deck text from the in-memory fixture, reject invalid validation
gates, update docs, and keep the real CLI artifacts gated until the required
files exist.
