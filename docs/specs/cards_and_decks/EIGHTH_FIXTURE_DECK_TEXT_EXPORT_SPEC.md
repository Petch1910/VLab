# Eighth Fixture Deck Text Export Spec

Milestone: `M66-02`

## Purpose

`M66-02` exports the eighth offline runtime/test fixture into
human-reviewable count-line deck text.

The exporter is scaffold-safe: tests may build the eighth fixture and M66-01
validation report in memory, while the real CLI artifacts remain gated until
the M65-06 fixture and M66-01 validation report files exist.

## Inputs

- `outputs/target_slice/runtime_fixtures/m64_recipe_001_kagero_m65_06.json`
- `outputs/target_slice/m66_01_eighth_fixture_schema_validation.json`
- `data/packs/vanguard_th/cards.sqlite`
- `data/packs/vanguard_th/manifest.json`

## Outputs

- `outputs/target_slice/m66_02_eighth_fixture_deck_text_export.txt`
- `outputs/target_slice/m66_02_eighth_fixture_deck_text_export.json`
- `outputs/target_slice/m66_02_eighth_fixture_deck_text_export.md`

These outputs are written only when the CLI is run against existing M65-06 and
M66-01 real artifacts.

## Text Format

The text export follows the existing `CountLineDeckCodec` shape:

```text
# Vanguard Thai Sim Deck List
Name: Link Joker/Legion Mate Kagero Fixture (m64_recipe_001)
Format: link_joker_legion_mate
PackId: vanguard_th
PackVersion: 251
PackDefinitionHash: <current pack definition hash>

[Main]
4 BTxx-xxxTH

[Ride]

[G]
# Note: Lock/Unlock runtime is disabled; Lock text remains review-only.
# Note: Legion/Mate runtime is disabled; Legion text remains review-only.
```

Review comments are allowed because the codec ignores comment lines beginning
with `#`. The exporter may include `# Card: ...` comments above card lines to
make the text easy to inspect without breaking import compatibility.

## Gate Rules

The exporter must block when:

- the `M66-01` validation report is missing or not schema-valid
- the fixture is not ready for `M66-02`
- a fixture card id is missing from SQLite
- a fixture main deck row is malformed

## Boundary

This milestone must not:

- mutate the fixture artifact
- add or replace saved player decks
- publish the fixture to UI deck selection
- enable bot playbook behavior
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- enable Mate identity checks
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_eighth_fixture_deck_text_export
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M65-06 and M66-01 outputs exist:

```powershell
python tools\deck\export_eighth_fixture_deck_text.py
```

## Done Rule

`M66-02` scaffold work is ready when the exporter and tests produce
review-only deck text from the in-memory fixture, reject invalid validation
gates, update docs, and keep the real CLI artifacts gated until the required
files exist.
