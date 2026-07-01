# Fourth Fixture Deck Text Export Spec

Milestone: `M50-02`

## Purpose

`M50-02` exports the fourth offline runtime/test fixture as human-reviewable
count-line deck text.

The export is an offline review artifact only. It is not a saved player deck,
is not published to the UI deck list, does not enable bot playbooks, and does
not enable G Zone or Stride runtime.

## Inputs

- `outputs/target_slice/runtime_fixtures/m48_recipe_001_g_series_first_royal_paladin_m49_05.json`
- `outputs/target_slice/m50_01_fourth_fixture_schema_validation.json`
- `data/packs/vanguard_th/cards.sqlite`
- `data/packs/vanguard_th/manifest.json`

## Outputs

- `outputs/target_slice/m50_02_fourth_fixture_deck_text_export.txt`
- `outputs/target_slice/m50_02_fourth_fixture_deck_text_export.json`
- `outputs/target_slice/m50_02_fourth_fixture_deck_text_export.md`

## Deck Text Shape

The text export must use the count-line deck format:

```text
# Vanguard Thai Sim Deck List
Name: ...
Format: ...
PackId: ...
PackVersion: ...

[Main]
# Card: ...
4 CARD-ID

[Ride]

[G]
# Empty: G Zone and Stride runtime are disabled for this fixture.
```

Rules:

- importable lines are only `<quantity> <card_id>`
- `# Card:` lines are review comments only
- `[Ride]` and `[G]` sections are preserved
- the fourth fixture `[G]` section must be comment-only because the accepted
  boundary is `main_deck_only_for_current_windows_fixture`

## Boundary

This milestone must not:

- mutate the fixture artifact
- inject saved player decks
- publish UI deck library entries
- enable bot playbook behavior
- enable G Zone or Stride runtime
- mutate `GameState`

## Verification

```powershell
python tools\deck\export_fourth_fixture_deck_text.py
python -m unittest tests.test_fourth_fixture_deck_text_export
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M50-02` is done when the export report is ready, the deck text has 14
importable main-deck lines totaling 50 cards, tests cover valid and invalid
validation-report cases, docs are updated, and the next target is `M50-03`.
