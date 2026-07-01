# M34-01 Deck Possibility Analysis Closeout

## Summary

Added an offline deck-construction possibility analyzer by clan/nation group.
The tool calculates whether each group has enough local runtime card data to
build theoretical 50-card main decks and exports large combinatoric counts under
copy limits.

This is a math/reporting artifact, not a playable deck generator.

## Added

- `tools/deck/analyze_deck_possibilities.py`
- `tools/deck/__init__.py`
- `tests/test_deck_possibility_analyzer.py`
- `docs/specs/cards_and_decks/DECK_POSSIBILITY_ANALYSIS_SPEC.md`

## Generated Outputs

Output directory:

```text
outputs/deck_possibility/
```

Generated files:

- `full_runtime_deck_possibility.json`
- `classic_part1_deck_possibility.json`
- `link_joker_legion_mate_deck_possibility.json`
- `g_series_first_deck_possibility.json`
- `g_next_z_deck_possibility.json`
- `v_reboot_deck_possibility.json`
- `v_shinemon_if_deck_possibility.json`
- `d_overdress_deck_possibility.json`
- `d_willdress_deck_possibility.json`
- `dz_divinez_deck_possibility.json`
- `deck_possibility_summary.csv`
- `deck_possibility_summary.json`

## Calculation Model

The report calculates:

- 50-card main deck capacity
- 16 trigger + 34 non-trigger main-deck capacity
- bounded count-distribution possibilities under `deck_limit`
- grade 0/1/2/3 ride-deck-style choice availability
- 16-card G-zone capacity

It excludes G-units, G-guardians, token units, markers, and crests from main
deck calculations.

## Headline Results

From `deck_possibility_summary.csv`:

- `full_runtime`: 45 groups, 37 groups pass the 16-trigger/34-non-trigger
  theoretical main-deck capacity check.
- `classic_part1`: 21 groups, 20 pass.
- `link_joker_legion_mate`: 23 groups, 18 pass.
- `g_series_first`: 25 groups, 24 pass; missing `G-SD01`, `G-SD02`.
- `g_next_z`: 28 groups, 24 pass.
- `v_reboot`: 26 groups, 24 pass.
- `v_shinemon_if`: 32 groups, missing several requested `V-BT`/`V-SS` set
  codes from the runtime pack.
- `d_overdress`: 10 groups, 6 pass.
- `d_willdress`: 10 groups, missing several requested `D-BT`/`D-LBT`/`D-SS`
  set codes from the runtime pack.
- `dz_divinez`: 0 groups because the runtime pack currently has no `DZ-*`
  cards.

Largest exact-16-trigger theoretical counts observed in spot checks:

- `classic_part1` / `โนว่า เกรปเปอร์`: about `9.094e+39`
- `link_joker_legion_mate` / `เบอร์มิวด้า ไทรแองเกิล`: about `1.559e+42`
- `g_series_first` / `เกียร์โครนิเคิล`: about `2.403e+46`
- `d_overdress` / `ลิริคัลโมนาสเทริโอ้`: about `3.773e+50`
- `full_runtime` / `เบอร์มิวด้า ไทรแองเกิล`: about `3.011e+71`

## Verification

Commands run:

```powershell
python -m unittest tests.test_deck_possibility_analyzer
python tools\deck\analyze_deck_possibilities.py --all-presets
```

Results:

- deck possibility tests passed: `4/4`
- all reports generated successfully
- missing set codes are reported explicitly

Unity compile/EditMode was not run because this task changed only Python,
generated CSV/JSON artifacts, and docs.

## Next Target

```text
M34-02: use deck possibility results to choose which clan/nation groups should
be reviewed first for playable archetype seeds.
```
