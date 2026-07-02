# Eighth-Slice Runtime Fixture Promotion Gate Spec

Milestone: `M65-06`

## Purpose

`M65-06` checks whether the human-selected, grade-repaired,
Lock/Legion-boundary-bounded, and revalidated eighth-slice recipe can be
promoted into an offline runtime/test fixture.

The gate may create a fixture artifact only. It must not inject saved decks,
publish UI deck lists, enable bot playbooks, enable Lock/Unlock runtime, enable
Legion/Mate runtime, or mutate `GameState`.

## Inputs

- `outputs/target_slice/m65_03_eighth_slice_human_accepted_grade_repair_artifact.json`
- `outputs/target_slice/m65_05_eighth_slice_repaired_recipe_validation_report.json`

Tests may pass in-memory `M65-03` and `M65-05` artifacts until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m65_06_eighth_slice_runtime_fixture_promotion_gate.json`
- `outputs/target_slice/m65_06_eighth_slice_runtime_fixture_promotion_gate.md`
- `outputs/target_slice/runtime_fixtures/<recipe_id>_kagero_m65_06.json`
  only if all gate checks pass

## Gate Checks

- Human selection, human acceptance, and grade repair acceptance are recorded.
- The Lock/Unlock boundary is `main_deck_only_review_no_runtime_promotion`.
- The Legion/Mate boundary is `main_deck_only_review_no_runtime_promotion`.
- Lock runtime, Unlock runtime, Legion runtime, and Mate identity checks remain
  disabled.
- Validation passes with runtime readiness true.
- Main deck is 50 cards.
- Trigger count is 16 with `Critical=4/Draw=4/Heal=4/Stand=4`.
- Grade profile is `G0=17/G1=14/G2=11/G3=8`.
- Grade 4 main-deck count is 0.
- Combo-to-recipe consistency passes and allows promotion.
- M65-05 reports `ready_for_m65_06=true`.
- M65-03 repaired rows still match the M65-05 selected recipe id and sum to 50
  cards.
- M65-05 did not create runtime fixtures, UI entries, bot playbooks, or
  `GameState` mutation.

## Runtime Boundary

This milestone must not:

- modify M64/M65 source artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable Lock/Unlock runtime
- enable locked-card visibility or circle-state runtime
- enable Legion/Mate runtime
- enable Legion attack timing or deck-building validation
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_eighth_slice_runtime_fixture_promotion_gate
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M65-03 and M65-05 outputs exist:

```powershell
python tools\deck\build_eighth_slice_runtime_fixture_promotion_gate.py
```

## Done Rule

`M65-06` is done when:

- all gate checks pass for an in-memory M65-03/M65-05 chain
- the generated fixture has 50 Kagero main-deck cards and 16 triggers
- the fixture records Lock/Unlock/Legion/Mate runtime disabled
- failing either the Lock boundary or Legion boundary blocks fixture creation
- no saved deck, UI deck list, bot playbook, live text parser, or `GameState`
  mutation occurs
- docs point the next target to `M65-closeout`
