# Sixth-Slice Recipe Pipeline Entry Gate Spec

Milestone: `M55-04`

## Purpose

Decide whether the sixth selected slice may enter offline recipe pipeline work.

This is a gate artifact only. It must not create deck recipes, runtime
fixtures, saved decks, UI deck entries, bot playbooks, G Zone runtime, Stride
runtime, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m55_02_sixth_slice_fixture_readiness.json
outputs/target_slice/m55_03_sixth_slice_semantic_compatibility_probe.json
```

## Gate Rules

The gate passes only when:

- `M55-02` fixture/format readiness passed
- `M55-02` marks the selected slice ready for semantic probe
- `M55-03` semantic/compatibility probe passed
- `M55-03` produced at least one candidate edge
- neither input allows runtime or bot promotion

Because the selected slice is `g_next_z`, recipe validation must start with a
fixture scaffold before any recipe validator work.

## Allowed Next Work

If the gate passes, the next queue is `M56`:

```text
M56-01 Sixth-slice fixture scaffold
M56-02 Sixth-slice review packet
M56-03 Sixth-slice recipe draft model
M56-04 Sixth-slice recipe validator
M56-05 Sixth-slice combo-to-recipe consistency
M56-06 Sixth-slice blocker repair candidates
M56-closeout Sixth-slice runtime readiness decision
```

## Runtime Boundary

- Decision artifact only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No G Zone or Stride runtime enablement.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m55_04_sixth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m55_04_sixth_slice_recipe_pipeline_entry_gate.md
```

## Verification

```powershell
python tools\deck\build_sixth_slice_recipe_pipeline_entry_gate.py
python -m unittest tests.test_sixth_slice_recipe_pipeline_entry_gate
python -m unittest discover -s tests -p "test_*.py"
```
