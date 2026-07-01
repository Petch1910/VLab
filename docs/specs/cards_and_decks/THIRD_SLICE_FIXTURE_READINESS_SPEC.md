# Third Slice Fixture Readiness Spec

Milestone: `M43-02`

## Purpose

Check whether the third selected slice has enough source-backed local card data
to enter a semantic/compatibility probe.

This is a readiness gate only. It must not create deck recipes, runtime
fixtures, saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m43_01_third_target_slice_selection.json
data/packs/vanguard_th/cards.sqlite
```

## Selected Slice

```text
Group: Bermuda Triangle
Era preset: link_joker_legion_mate
Series scope: TD07-TD17, BT10-BT17, EB06-EB12
```

## Readiness Rules

The selected slice is ready for `M43-03` only when all checks pass:

- selected group has source-backed cards in the selected era scope
- grade 0, 1, 2, and 3 cards are present
- trigger capacity is at least 16 cards
- Critical, Draw, Heal, and Stand trigger types are present
- total main-deck capacity is at least 50 cards
- non-trigger capacity is at least 34 cards

Because this slice is not `classic_part1`, it must use a new third-slice
fixture scaffold before any recipe/runtime promotion.

## Outputs

```text
outputs/target_slice/m43_02_third_slice_fixture_readiness.json
outputs/target_slice/m43_02_third_slice_fixture_readiness.md
```

## Runtime Boundary

- Offline fixture readiness only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No `GameState` mutation.

## Verification

```powershell
python tools\deck\build_third_slice_fixture_readiness.py
python -m unittest tests.test_third_slice_fixture_readiness
python -m unittest discover -s tests -p "test_*.py"
```
