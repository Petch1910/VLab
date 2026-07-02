# Seventh-Slice Runtime Fixture Promotion Gate Spec

Milestone: `M61-06`

## Purpose

`M61-06` checks whether the human-selected, accepted, system-boundary-bounded,
and revalidated seventh-slice recipe can be promoted into an offline
runtime/test fixture.

The gate may create a fixture artifact only. It must not inject saved decks,
publish UI deck lists, enable bot playbooks, enable G Zone/Stride runtime,
enable Bloom/token runtime, or mutate `GameState`.

## Inputs

- `outputs/target_slice/m61_03_seventh_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m61_05_seventh_slice_repaired_recipe_validation_report.json`

Tests may pass in-memory `M61-03` and `M61-05` artifacts until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m61_06_seventh_slice_runtime_fixture_promotion_gate.json`
- `outputs/target_slice/m61_06_seventh_slice_runtime_fixture_promotion_gate.md`
- `outputs/target_slice/runtime_fixtures/<recipe_id>_neo_nectar_m61_06.json`
  only if all gate checks pass

## Gate Checks

- Human selection and human acceptance are recorded.
- The G Zone / Stride boundary is
  `main_deck_only_review_no_runtime_promotion`.
- The Bloom/token boundary is
  `manual_semantic_review_only_no_runtime_promotion`.
- G Zone runtime, Stride runtime, Bloom/token runtime, Grade 4 main-deck use,
  G-unit main-deck use, and token main-deck use remain disabled.
- Validation passes with runtime readiness true.
- Main deck is 50 cards.
- Trigger count is 16 with `Critical=4/Draw=4/Heal=4/Stand=4`.
- Grade profile is `G0=17/G1=14/G2=11/G3=8`.
- Grade 4 main-deck count is 0.
- Combo-to-recipe consistency passes and allows promotion.
- M61-05 reports `ready_for_m61_06=true`.
- M61-03 repaired rows still match the M61-05 accepted recipe id and sum to
  50 cards.
- M61-05 did not create runtime fixtures, UI entries, bot playbooks, or
  `GameState` mutation.

## Runtime Boundary

This milestone must not:

- modify M60/M61 source artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable G Zone/Stride runtime
- enable Bloom/token effect runtime
- enable token lifecycle runtime
- enable same-name runtime tracking
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_seventh_slice_runtime_fixture_promotion_gate
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M61-03 and M61-05 outputs exist:

```powershell
python tools\deck\build_seventh_slice_runtime_fixture_promotion_gate.py
```

## Done Rule

`M61-06` is done when:

- all gate checks pass for an in-memory M61-03/M61-05 chain
- the generated fixture has 50 Neo Nectar main-deck cards and 16 triggers
- the fixture records G Zone/Stride/Bloom/token runtime disabled
- failing either the G Zone boundary or Bloom/token boundary blocks fixture
  creation
- no saved deck, UI deck list, bot playbook, live text parser, or `GameState`
  mutation occurs
- docs point the next target to `M61-closeout`
