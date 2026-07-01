# Fifth Fixture Deck Text Export Spec

Milestone: `M54-02`

## Purpose

`M54-02` exports the fifth offline runtime/test fixture into human-reviewable
count-line deck text.

The output is for review, handoff, and the next headless fixture smoke test.
It must not be inserted into the user's saved deck library automatically.

## Inputs

- `outputs/target_slice/runtime_fixtures/m52_recipe_001_gold_paladin_m53_05.json`
- `outputs/target_slice/m54_01_fifth_fixture_schema_validation.json`
- `data/packs/vanguard_th/cards.sqlite`
- `data/packs/vanguard_th/manifest.json`

## Outputs

- `outputs/target_slice/m54_02_fifth_fixture_deck_text_export.txt`
- `outputs/target_slice/m54_02_fifth_fixture_deck_text_export.json`
- `outputs/target_slice/m54_02_fifth_fixture_deck_text_export.md`

## Text Format

The text export follows the existing `CountLineDeckCodec` shape:

```text
# Vanguard Thai Sim Deck List
Name: Link Joker Legion Gold Paladin Fixture (m52_recipe_001)
Format: link_joker_legion_mate
PackId: vanguard_th
PackVersion: 251
PackDefinitionHash: <current pack definition hash>

[Main]
3 BT10-011TH

[Ride]

[G]
```

Review comments are allowed because the codec ignores comment lines beginning
with `#`. The exporter may include `# Card: ...` comments above card lines to
make the text easy to inspect without breaking import compatibility.

## Gate Rules

The exporter must block when:

- the `M54-01` validation report is missing or not schema-valid
- the fixture is not ready for `M54-02`
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
python tools\deck\export_fifth_fixture_deck_text.py
python -m unittest tests.test_fifth_fixture_deck_text_export
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M54-02` is done when the deck text, JSON report, and markdown report are
generated, tests cover valid and invalid validation gates, docs are updated,
and the next target is `M54-03`.
