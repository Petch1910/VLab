# Ninth Fixture Deck Text Export Spec

Milestone: `M70-02`

## Purpose

`M70-02` exports the ninth offline runtime/test fixture into
human-reviewable count-line deck text.

The exporter is scaffold-safe: tests may build the ninth fixture and M70-01
validation report in memory, while the real CLI artifacts remain gated until
the M69-06 fixture and M70-01 validation report files exist.

## Inputs

- `outputs/target_slice/runtime_fixtures/m68_recipe_001_aqua_force_m69_06.json`
- `outputs/target_slice/m70_01_ninth_fixture_schema_validation.json`
- `data/packs/vanguard_th/cards.sqlite`
- `data/packs/vanguard_th/manifest.json`

## Outputs

- `outputs/target_slice/m70_02_ninth_fixture_deck_text_export.txt`
- `outputs/target_slice/m70_02_ninth_fixture_deck_text_export.json`
- `outputs/target_slice/m70_02_ninth_fixture_deck_text_export.md`

These outputs are written only when the CLI is run against existing M69-06 and
M70-01 real artifacts.

## Text Format

The text export follows the existing `CountLineDeckCodec` shape:

```text
# Vanguard Thai Sim Deck List
Name: G Series Aqua Force Fixture (m68_recipe_001)
Format: g_series_first
PackId: vanguard_th
PackVersion: 251
PackDefinitionHash: <current pack definition hash>

[Main]
4 G-BTxx-xxxTH

[Ride]

[G]
# Note: G Zone runtime is disabled; G-unit and G-zone cards remain review-only.
# Note: Stride runtime is disabled; stride text remains review-only.
# Note: Aqua Force battle-order runtime is disabled; wave and battle-order text remain manual review only.
```

Review comments are allowed because the codec ignores comment lines beginning
with `#`. The exporter may include `# Card: ...` comments above card lines to
make the text easy to inspect without breaking import compatibility.

## Gate Rules

The exporter must block when:

- the `M70-01` validation report is missing or not schema-valid
- the fixture is not ready for `M70-02`
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
- enable Aqua Force battle-order runtime
- enable battle-count tracker runtime
- enable attack-order predicate runtime
- enable multi-attack label runtime
- parse live card text
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_ninth_fixture_deck_text_export
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M69-06 and M70-01 outputs exist:

```powershell
python tools\deck\export_ninth_fixture_deck_text.py
```

## Done Rule

`M70-02` scaffold work is ready when the exporter and tests produce
review-only deck text from the in-memory fixture, reject invalid validation
gates, update docs, and keep the real CLI artifacts gated until the required
files exist.
