# Deck Possibility Analysis Spec

## Status

`M34-01` adds an offline theoretical deck-construction possibility report by
clan/nation group.

`M34-02` imported the 2026-06-29 rule/development corpus and changed the next
step from direct combo-pair review to a rule-corpus-driven plan:

```text
M34-03 feasible group priority v2
M35 rule taxonomy integration
M36 deck legality v2
M37 semantic requirement/provider tags
M38 combo compatibility/conflict checks
```

Use `RULE_CORPUS_DECK_COMBO_PLAN_SPEC.md` for the current plan beyond raw
deck capacity.

## Purpose

Before promoting combo pairs into playbook seeds, the project needs to know
which clans or nations have enough local runtime card data to build theoretical
50-card decks.

This analysis answers:

- how many cards each clan/nation has in each era preset
- whether the group has enough copy capacity for a 50-card main deck
- whether the group has enough trigger and non-trigger capacity for a basic
  `16 trigger + 34 non-trigger` main-deck shape
- how many theoretical count-distribution deck configurations exist under
  `deck_limit`
- whether a group has grade `0/1/2/3` choices for a ride-deck-style package
- whether a group has enough G-zone capacity for 16 G cards

## Boundary

This is an offline math/reporting tool only.

It does not:

- validate complete official deck legality
- enforce all trigger subtype restrictions
- enforce banned/restricted lists
- optimize ratios
- build a playable decklist
- mutate `GameState`
- run in Unity runtime

The counts are count distributions over card ids and copy limits, not shuffled
deck orders.

## Source

Input:

```text
data/packs/vanguard_th/cards.sqlite
```

Important columns:

- `card_id`
- `series_code`
- `clan`
- `nation`
- `grade`
- `trigger`
- `deck_limit`
- `type_1`
- `type_2`

## Grouping

Default group mode is `auto`:

- use `clan` when it is a real clan value
- if `clan` is `N/A`, fall back to `nation`
- this keeps D-era nation cards out of a useless `N/A` bucket

## Combinatorics

The analyzer uses bounded dynamic programming:

```text
coefficient of x^N in product((1 + x + ... + x^deck_limit_i))
```

Computed values:

- `main_50_any_mix`: ways to choose 50 cards from main-deck-eligible card ids
- `trigger_16_component`: ways to choose 16 trigger cards
- `non_trigger_34_component`: ways to choose 34 non-trigger cards
- `main_50_exact_16_triggers`: trigger component multiplied by non-trigger
  component
- `ride_deck_grade_0_1_2_3_choices`: one card choice per grade 0/1/2/3
- `g_zone_16`: ways to choose 16 G-zone cards

Large integers are exported as exact strings plus digit/scientific summaries.

## Outputs

Default output directory:

```text
outputs/deck_possibility/
```

Generated reports:

- `full_runtime_deck_possibility.json`
- one `<preset>_deck_possibility.json` per era preset
- `deck_possibility_summary.csv`
- `deck_possibility_summary.json`

## CLI

Single preset:

```powershell
python tools\deck\analyze_deck_possibilities.py --preset classic_part1
```

All presets plus full runtime:

```powershell
python tools\deck\analyze_deck_possibilities.py --all-presets
```

## Verification

Python tests cover:

- bounded distribution count behavior
- 50-card deck feasibility with 16 triggers and 34 non-triggers
- D-era `clan=N/A` nation fallback
- summary JSON feasible group counts

## Known Limitations

- Trigger subtype caps are summarized but not fully enforced yet.
- D ride deck and G deck rules are approximated as capacity/choice checks.
- Exact official format legality still belongs in the deck validator, not this
  offline report.
- A high possibility count means the card pool is broad, not that the deck is
  good.
- A feasible group must still pass format legality and combo compatibility
  gates before being promoted into playbook or bot data.
