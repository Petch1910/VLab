# Fourth-Slice Recipe Pipeline Entry Gate Spec

Milestone: `M47-04`

## Purpose

Decide whether the fourth selected slice may enter offline recipe pipeline work.

This is a gate artifact only. It must not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m47_repair_apply_scope.json
outputs/target_slice/m47_03_fourth_slice_semantic_compatibility_probe.json
```

## Gate Rules

The gate passes only when:

- `M47-repair-apply-scope` fixture expectations passed
- `M47-repair-apply-scope` marks the selected slice ready for semantic probe
- `M47-03` semantic/compatibility probe passed
- `M47-03` produced at least one candidate edge
- neither input allows runtime or bot promotion

Because the selected slice is a G-era Royal Paladin expanded source slice,
recipe validation must start with a fixture scaffold before any recipe validator
work.

## Allowed Next Work

If the gate passes, the next queue is `M48`:

```text
M48-01 Fourth-slice fixture scaffold
M48-02 Fourth-slice review packet
M48-03 Fourth-slice recipe draft model
M48-04 Fourth-slice recipe validator
M48-05 Fourth-slice combo-to-recipe consistency
M48-06 Fourth-slice blocker repair candidates
M48-closeout Fourth-slice runtime readiness decision
```

## Runtime Boundary

- Decision artifact only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m47_04_fourth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m47_04_fourth_slice_recipe_pipeline_entry_gate.md
```

## Verification

```powershell
python tools\deck\build_fourth_slice_recipe_pipeline_entry_gate.py
python -m unittest tests.test_fourth_slice_recipe_pipeline_entry_gate
python -m unittest discover -s tests -p "test_*.py"
```
