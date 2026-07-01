# First Runtime-Ready Recipe Decision Spec

Milestone: `M37-closeout`

## Purpose

`M37-closeout` decides whether the first repaired accepted seed recipe can
become a runtime/test fixture or must remain advisory.

This is a decision artifact. It must not create a runtime deck or modify recipe
draft source files.

## Inputs

- `outputs/target_slice/m37_01_accepted_seed_slot_gap_candidates.json`
- `outputs/target_slice/m37_02_trigger_package_repair_proposal.json`
- `outputs/target_slice/m37_03_rejected_line_support_gap_triage.json`
- `outputs/target_slice/m37_04_manual_semantic_mapping_candidates.json`
- `outputs/target_slice/m37_05_revised_recipe_validation_rerun.json`

## Outputs

- `outputs/target_slice/m37_closeout_first_runtime_ready_recipe_decision.json`
- `outputs/target_slice/m37_closeout_first_runtime_ready_recipe_decision.md`

## Required Decision

- whether M37 is complete
- whether a first runtime-ready recipe exists
- whether `recipe_003` can become a runtime fixture
- whether `recipe_003` remains advisory
- explicit blockers preventing runtime promotion
- next queue selection

## Runtime Boundary

This milestone must not:

- create runtime decks
- mutate recipe draft files
- accept rejected combo lines
- promote playbook hints
- enable bot integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_first_runtime_ready_recipe_decision.py
python -m unittest tests.test_first_runtime_ready_recipe_decision
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M37-closeout` is done when it records the runtime-ready decision, keeps
promotion disabled if human/grade review remains open, and points to the next
queue.

