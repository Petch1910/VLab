# Third Fixture Deck Text Export Spec

Milestone: `M46-02`

## Purpose

`M46-02` exports the third offline runtime/test fixture into human-reviewable
count-line deck text.

The output is for review, handoff, and the next headless fixture smoke test.
It must not be inserted into the user's saved deck library automatically.

## Inputs

- `outputs/target_slice/runtime_fixtures/m44_recipe_001_link_joker_legion_bermuda_triangle_m45_04.json`
- `outputs/target_slice/m46_01_third_fixture_schema_validation.json`
- `data/packs/vanguard_th/cards.sqlite`
- `data/packs/vanguard_th/manifest.json`

## Outputs

- `outputs/target_slice/m46_02_third_fixture_deck_text_export.txt`
- `outputs/target_slice/m46_02_third_fixture_deck_text_export.json`
- `outputs/target_slice/m46_02_third_fixture_deck_text_export.md`

## Text Format

The text export follows the existing `CountLineDeckCodec` shape:

```text
# Vanguard Thai Sim Deck List
Name: Link Joker Legion Bermuda Triangle Fixture (m44_recipe_001)
Format: link_joker_legion_mate
PackId: vanguard_th
PackVersion: 251
PackDefinitionHash: <current pack definition hash>

[Main]
4 EB06-003TH

[Ride]

[G]
```

Review comments are allowed because the codec ignores comment lines beginning
with `#`. The exporter may include `# Card: ...` comments above card lines to
make the text easy to inspect without breaking import compatibility.

## Gate Rules

The exporter must block when:

- the `M46-01` validation report is missing or not schema-valid
- the fixture is not ready for `M46-02`
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
python tools\deck\export_third_fixture_deck_text.py
python -m unittest tests.test_third_fixture_deck_text_export
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M46-02` is done when the deck text, JSON report, and markdown report are
generated, tests cover valid and invalid validation gates, docs are updated,
and the next target is `M46-03`.
