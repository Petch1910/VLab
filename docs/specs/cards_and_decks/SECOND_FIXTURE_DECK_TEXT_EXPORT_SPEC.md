# Second Fixture Deck Text Export Spec

Milestone: `M42-02`

## Purpose

`M42-02` exports the Oracle Think Tank offline runtime fixture into
human-reviewable count-line deck text.

The output is for review, handoff, and future fixture smoke tests. It must not
be inserted into the user's saved deck library automatically.

## Inputs

- `outputs/target_slice/runtime_fixtures/m40_recipe_001_classic_core_oracle_think_tank_m41_04.json`
- `outputs/target_slice/m42_01_second_fixture_schema_validation.json`
- `data/packs/vanguard_th/cards.sqlite`
- `data/packs/vanguard_th/manifest.json`

## Outputs

- `outputs/target_slice/m42_02_second_fixture_deck_text_export.txt`
- `outputs/target_slice/m42_02_second_fixture_deck_text_export.json`
- `outputs/target_slice/m42_02_second_fixture_deck_text_export.md`

## Text Format

The text export follows the existing `CountLineDeckCodec` shape:

```text
# Vanguard Thai Sim Deck List
Name: Classic Core Oracle Think Tank Fixture (m40_recipe_001)
Format: classic_part1
PackId: vanguard_th
PackVersion: 251
PackDefinitionHash: <current pack definition hash>

[Main]
4 BT01-006TH

[Ride]

[G]
```

Review comments are allowed because the codec ignores comment lines beginning
with `#`. The exporter may include `# Card: ...` comments above card lines to
make the text easy to inspect without breaking import compatibility.

## Gate Rules

The exporter must block when:

- the `M42-01` validation report is missing or not schema-valid
- the fixture is not ready for `M42-02`
- a fixture card id is missing from SQLite
- a fixture main deck row is malformed

## Boundary

This milestone must not:

- mutate the fixture artifact
- add or replace saved player decks
- publish the fixture to UI deck selection
- enable bot playbook behavior
- mutate `GameState`

## Verification

```powershell
python tools\deck\export_second_fixture_deck_text.py
python -m unittest tests.test_second_fixture_deck_text_export
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M42-02` is done when the deck text, JSON report, and markdown report are
generated, tests cover valid and invalid validation gates, and the next target
is `M42-03`.
