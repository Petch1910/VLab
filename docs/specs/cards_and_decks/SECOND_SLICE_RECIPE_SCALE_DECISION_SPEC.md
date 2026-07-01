# Second-Slice Recipe Scale Decision Spec

Milestone: `M39-04`

## Purpose

`M39-04` decides whether the project can safely start an offline recipe pipeline
for the second slice, `Classic Core / Oracle Think Tank`, after the first
fixture has passed offline and Unity headless consumption.

This is a decision artifact only. It does not create a deck, publish a deck,
enable bot playbooks, or mutate runtime state.

## Inputs

- `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
- `outputs/target_slice/m36_05_second_slice_readiness_comparison.json`
- `outputs/target_slice/m35_e2_second_slice_fixture_readiness.json`
- `outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.json`
- `outputs/target_slice/m35_e4_bot_integration_gate.json`

## Outputs

- `outputs/target_slice/m39_04_second_slice_recipe_scale_decision.json`
- `outputs/target_slice/m39_04_second_slice_recipe_scale_decision.md`

## Decision Rules

The decision may open the next queue only when:

- `M39-03` reports `ready_for_m39_04 = true`
- second-slice fixture readiness is true
- Classic Core policy is reusable for the second slice
- second-slice semantic/compatibility probe passed
- second-slice evidence still blocks runtime/bot promotion
- bot integration remains disabled

## Allowed If Passed

- Create a second-slice review packet.
- Create advisory recipe drafts.
- Validate recipe counts, trigger profile, grade profile, clan identity, copy
  limits, and missing cards.
- Check combo-to-recipe consistency.
- Generate source-backed repair candidates.

## Still Blocked

- Saved deck injection.
- UI deck-list publication.
- Runtime deck fixture promotion.
- Bot playbook promotion.
- Live card text parsing.
- Direct `GameState` mutation.

## Proposed Next Queue

- `M40-01`: Second-slice review packet
- `M40-02`: Second-slice recipe draft model
- `M40-03`: Second-slice recipe validator
- `M40-04`: Second-slice combo-to-recipe consistency
- `M40-05`: Second-slice blocker repair candidates
- `M40-closeout`: Second-slice runtime readiness decision

## Verification

```powershell
python tools\deck\build_second_slice_recipe_scale_decision.py
python -m unittest tests.test_second_slice_recipe_scale_decision
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M39-04` is done when the decision artifact is generated, tests cover pass and
block cases, docs point to `M40-01`, and runtime/bot/saved-deck boundaries
remain closed.
