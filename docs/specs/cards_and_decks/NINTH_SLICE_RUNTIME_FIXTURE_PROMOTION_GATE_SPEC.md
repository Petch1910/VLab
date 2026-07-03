# Ninth-Slice Runtime Fixture Promotion Gate Spec

Milestone: `M69-06`

## Purpose

`M69-06` is the ninth-slice offline runtime/test fixture promotion gate.

It may create a fixture artifact only after the repaired ninth-slice main deck
has passed `M69-05` validation with explicit human repair acceptance and
explicit G Zone / Stride / Aqua Force boundary decisions.

This milestone does not publish a playable saved deck.

## Inputs

- `outputs/target_slice/m69_03_ninth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m69_05_ninth_slice_repaired_recipe_validation_report.json`

Tests may pass in-memory M69-03 and M69-05 evidence until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m69_06_ninth_slice_runtime_fixture_promotion_gate.json`
- `outputs/target_slice/m69_06_ninth_slice_runtime_fixture_promotion_gate.md`
- `outputs/target_slice/runtime_fixtures/m68_recipe_001_aqua_force_m69_06.json`
  only when every gate check passes

## Gate Checks

The gate must pass all of these checks before it creates the fixture:

- human selection, human acceptance, and repair acceptance are recorded in
  M69-03 and preserved by M69-05
- accepted repair is ready for the system decision boundary
- G Zone option is `main_deck_only_review_no_runtime_promotion`
- Stride option is `main_deck_only_review_no_runtime_promotion`
- Aqua Force option is `manual_semantic_review_only_no_runtime_promotion`
- all three boundary decisions are recorded
- main-deck-only validation boundary is applied
- G Zone, Stride, and Aqua Force battle-order runtime are disabled
- M69-05 validation status is `validator_passed`
- runtime-ready flag is true
- blocker count is zero
- blocker and review code lists are empty
- main deck count is `50`
- trigger count is `16`
- trigger profile is `4/4/4/4` for Critical/Draw/Heal/Stand
- grade profile is `17/14/11/8`
- Grade 4 / G-unit cards are not in the main deck
- selected combo-pair cards are present
- M69-05 reports `ready_for_m69_06=true`

## Fixture Shape

The fixture uses `deck_recipe_runtime_fixture_v1` and includes:

- fixture id and fixture scope
- source artifact references
- selected target and format policy
- main deck card ids and quantities only
- count summary
- accepted repair package ids
- selected system decision option ids
- G Zone / Stride / Aqua Force boundary flags
- runtime boundary flags

## Runtime Boundary

This milestone must not:

- modify M69 source artifacts
- mutate the runtime deck library
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- parse live card text at runtime
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- mutate `GameState`

The created fixture is for offline runtime/test evidence only.

## Verification

```powershell
python -m unittest tests.test_ninth_slice_runtime_fixture_promotion_gate
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after real M69-03 and M69-05 output files exist:

```powershell
python tools\deck\build_ninth_slice_runtime_fixture_promotion_gate.py
```

## Done Rule

`M69-06` is done when:

- the gate blocks fixture creation unless all checks pass
- the passing path creates an offline runtime/test fixture model
- the fixture preserves main deck counts, trigger counts, grade counts, clan
  counts, and series counts from validation
- G Zone / Stride / Aqua Force boundaries remain disabled in both gate and
  fixture outputs
- saved deck, UI publication, bot/playbook, live card text parsing, and
  `GameState` mutation remain disabled
- targeted and full Python tests pass
- roadmap/current-status docs point to `M69-closeout`
