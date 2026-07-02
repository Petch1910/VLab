# Seventh-Slice Repaired Recipe Validation Rerun Spec

Milestone: `M61-05`

## Purpose

`M61-05` validates the repaired quantity preview from `M61-03` after the
explicit G Zone / Stride / Bloom-token boundary from `M61-04`.

This is still an offline validation rerun. It may report that the repaired
main-deck recipe is validator-passed and consistency-passed, but it must not
create a runtime fixture, saved deck, UI entry, or bot playbook. `M61-06` is
the runtime fixture promotion gate.

## Inputs

- `outputs/target_slice/m61_03_seventh_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m61_04_seventh_slice_system_decision_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory `M61-03` and `M61-04` artifacts until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m61_05_seventh_slice_repaired_recipe_validation_report.json`
- `outputs/target_slice/m61_05_seventh_slice_repaired_recipe_validation_report.md`

## Validation Rules

Reuse the seventh-slice recipe validator and combo consistency rules against
the `M61-03` repaired quantity preview:

- Card ids must exist in runtime SQLite.
- Quantity must be positive.
- Quantity must not exceed SQLite `deck_limit`.
- Main deck explicit count must equal `50`.
- Trigger count must equal `16`.
- Heal trigger count must not exceed `4`.
- Grade 4 / G units must not appear in the main deck.
- Tokens must not appear in the main deck.
- All cards must match the selected clan.
- Grade profile should equal the classic advisory target.
- Manual-review card overlap must be absent after the accepted repair.
- Human acceptance must already be recorded by `M61-03`.
- The `M61-04` boundary must allow both G Zone / Stride main-deck-only
  validation and Bloom/token manual-semantic validation to open `M61-06`.

## Boundary Policy

`M61-05` validates only the main deck. The `M61-04` boundary may suppress
`g_zone_support_deferred` and `bloom_token_support_deferred` for this validation
pass, but it must not enable:

- G Zone runtime
- Stride runtime
- Generation Break runtime
- Bloom/token effect runtime
- token lifecycle runtime
- same-name runtime tracking
- duration cleanup runtime
- Grade 4 / G units in the main deck

## Runtime Boundary

This milestone must not:

- record human acceptance
- record a new G Zone / Stride decision
- record a new Bloom/token decision
- mutate M61-03 or M61-04 artifact files
- mutate M60 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_seventh_slice_repaired_recipe_validation
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M61-03 and M61-04 outputs exist:

```powershell
python tools\deck\validate_seventh_slice_repaired_recipe.py
```

## Done Rule

`M61-05` is done when:

- repaired preview validates through the seventh-slice validator
- combo/recipe consistency passes for the selected pair
- deferred G Zone and Bloom/token review codes are suppressed only when the
  M61-04 boundary allows main-deck validation
- deferred boundary decisions block `ready_for_m61_06`
- no runtime fixture, saved deck, UI deck list, bot playbook, or GameState
  mutation occurs
- `ready_for_m61_06=true` only when validation, consistency, acceptance, and
  both M61-04 boundary decisions all pass
