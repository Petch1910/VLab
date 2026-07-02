# Ninth-Slice Repaired Recipe Validation Rerun Spec

Milestone: `M69-05`

## Purpose

`M69-05` reruns validation for the repaired ninth-slice main-deck preview from
`M69-03`, using the explicit G Zone / Stride / Aqua Force boundary decision
from `M69-04`.

This milestone decides whether the repaired 50-card main deck is ready for the
next offline fixture promotion gate. It does not create a runtime fixture.

## Inputs

- `outputs/target_slice/m69_03_ninth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m69_04_ninth_slice_system_decision_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory M69-03 and M69-04 artifacts until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m69_05_ninth_slice_repaired_recipe_validation_report.json`
- `outputs/target_slice/m69_05_ninth_slice_repaired_recipe_validation_report.md`

## Validation Rules

The tool builds an in-memory one-recipe validation report from M69-03
`accepted_repair.repaired_quantities`.

It must validate:

- main deck count is `50`
- trigger count is `16`
- trigger profile is `4/4/4/4` for Critical/Draw/Heal/Stand
- heal count is at most `4`
- grade profile is `17/14/11/8`
- Grade 4 / G-unit cards are not in the main deck
- card ids exist in SQLite
- copy limits are respected
- cards stay in the expected clan/source-series scope
- original manual-review overlap is cleared by the accepted repair
- selected combo-pair cards are present in the repaired recipe

## Boundary Policy

M69-05 may suppress only these review issue codes when M69-04 explicitly
selects main-deck/manual-semantic review options:

- `g_zone_support_deferred`
- `stride_support_deferred`
- `aqua_force_battle_order_support_deferred`

If any of the three M69-04 decisions chooses a defer-until-runtime option, the
validation may still pass structurally, but `ready_for_m69_06` must remain
`false`.

## Runtime Boundary

This milestone must not:

- record human selection
- record human repair acceptance
- record G Zone / Stride / Aqua Force decisions
- modify M68/M69 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_ninth_slice_repaired_recipe_validation
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M69-03 and M69-04 output files exist:

```powershell
python tools\deck\validate_ninth_slice_repaired_recipe.py
```

## Done Rule

`M69-05` is done when:

- the repaired preview validates as one in-memory recipe
- accepted repair and M69-04 boundary context are required
- G Zone / Stride / Aqua Force deferred issue codes are suppressed only when
  M69-04 allows main-deck/manual-semantic validation
- defer decisions keep `ready_for_m69_06=false`
- consistency confirms the selected pair cards are present
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- targeted and full Python tests pass
- roadmap/current-status docs point to `M69-06`
