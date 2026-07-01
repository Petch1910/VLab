# First Slice Deck Legality Fixtures Spec

Milestone: `M35-A3`

## Purpose

Create minimal, source-traceable deck legality fixtures for the first selected
Hybrid Vertical-Slice target before semantic tagging or combo compatibility
work begins.

Selected target from `M35-A2`:

```text
Slice: Classic Core
Era preset: classic_part1
Set scope: TD01-TD06 / BT01-BT09 / EB01-EB05
Group: โนว่า เกรปเปอร์
```

## Fixture Contract

Fixtures are generated from:

```text
outputs/target_slice/m35_a2_first_target_slice_report.json
data/packs/vanguard_th/cards.sqlite
```

The minimal validator checks:

- main deck count is exactly `50`
- trigger count is exactly `16`
- non-trigger setup choices include grades `0`, `1`, `2`, and `3`
- all cards stay inside the selected set scope
- all cards stay inside the selected clan identity
- each card count stays within runtime `cards.deck_limit`

## Fixtures

The generated fixture set must include:

- one passing selected-slice deck
- one failing short-main deck
- one failing trigger-count deck
- one failing missing setup grade deck
- one failing copy-limit deck
- one failing identity-mismatch deck

## Non-Goals

- no full official deck legality claim
- no broad format legality engine
- no semantic ability parsing
- no combo compatibility scoring
- no Unity/runtime mutation
- no bot/playbook promotion

## Deferred Source-Backed Limits

These are documented but not broadly enforced in this slice:

- official heal trigger maximum
- format-wide copy-limit exceptions beyond runtime `deck_limit`

## Outputs

```text
outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.json
outputs/target_slice/m35_a3_first_slice_deck_legality_fixtures.md
```

## Verification

```powershell
python tools\deck\build_first_slice_deck_fixtures.py
python -m unittest tests.test_first_slice_deck_fixtures
python -m unittest discover -s tests -p "test_*.py"
```
