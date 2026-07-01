# Fourth-Slice Repaired Recipe Validation Rerun Spec

Milestone: `M49-04`

## Purpose

`M49-04` validates the repaired quantity preview from the `M49-03`
human-accepted repair artifact.

This is still an offline validation rerun. It may report that the repaired
main-deck recipe is validator-passed and runtime-ready as data, but it must not
create a runtime fixture, saved deck, UI entry, or bot playbook. `M49-05` is the
runtime fixture promotion gate.

## Inputs

- `outputs/target_slice/m49_03_fourth_slice_human_accepted_repair_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m49_04_fourth_slice_repaired_recipe_validation_report.json`
- `outputs/target_slice/m49_04_fourth_slice_repaired_recipe_validation_report.md`

## Validation Rules

Reuse the fourth-slice recipe validator rules against the `M49-03` repaired
quantity preview:

- Card ids must exist in runtime SQLite.
- Quantity must be positive.
- Quantity must not exceed SQLite `deck_limit`.
- Main deck explicit count must equal `50`.
- Trigger count must equal `16`.
- Heal trigger count must not exceed `4`.
- Grade 4 / G units must not appear in the main deck.
- All cards must match the selected clan.
- Grade profile should equal the classic advisory target.
- Manual-review card overlap must be absent after the accepted repair.
- Human acceptance must already be recorded by `M49-03`.
- The `M49-02` boundary must be `main_deck_only_for_current_windows_fixture`.

## G Zone Boundary

`M49-04` validates only the main deck. The `M49-02` boundary may suppress
`g_zone_support_deferred` for this validation pass, but it must not enable G
Zone, Stride, Generation Break runtime, Grade 4 main-deck usage, or G-unit
runtime usage.

## Runtime Boundary

This milestone must not:

- record human acceptance
- record a new G Zone decision
- mutate M49-03 accepted repair files
- mutate M48-03 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\validate_fourth_slice_repaired_recipe.py
python -m unittest tests.test_fourth_slice_repaired_recipe_validation
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M49-04` is done when:

- one repaired recipe is validated
- validator status is `validator_passed`
- runtime-ready recipe count is `1`
- missing-card/copy-limit/slot-gap/trigger-count/manual-overlap/grade-profile/
  Grade 4/G Zone issue counts are `0`
- G Zone and Stride runtime remain disabled
- no fixture/saved deck/UI/bot/GameState mutation is performed
- `ready_for_m49_05=true`
- project status docs point the active queue to `M49-05`
