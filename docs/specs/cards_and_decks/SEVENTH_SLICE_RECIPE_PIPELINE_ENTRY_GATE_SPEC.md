# Seventh-Slice Recipe Pipeline Entry Gate Spec

Milestone: `M59-04`

## Purpose

Decide whether the seventh selected slice may enter offline recipe pipeline work.

This is a gate artifact only. It must not create deck recipes, runtime
fixtures, saved decks, UI deck entries, bot playbooks, G Zone runtime, Stride
runtime, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m59_02_seventh_slice_fixture_readiness.json
outputs/target_slice/m59_03_seventh_slice_semantic_compatibility_probe.json
```

Tests may use in-memory M59-02 and M59-03 reports. Real CLI artifacts remain
gated until the real upstream files exist.

## Gate Rules

The gate passes only when:

- `M59-02` fixture/format readiness passed
- `M59-02` marks the selected slice ready for semantic probe
- `M59-03` semantic/compatibility probe passed
- `M59-03` produced at least one candidate edge
- neither input allows runtime or bot promotion

Because the selected slice is `g_series_first`, recipe validation must start
with a fixture scaffold before any recipe validator work. G Zone and Stride
runtime remain disabled.

## Allowed Next Work

If the gate passes, the next queue is `M60`:

```text
M60-01 Seventh-slice fixture scaffold
M60-02 Seventh-slice review packet
M60-03 Seventh-slice recipe draft model
M60-04 Seventh-slice recipe validator
M60-05 Seventh-slice combo-to-recipe consistency
M60-06 Seventh-slice blocker repair candidates
M60-closeout Seventh-slice runtime readiness decision
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
outputs/target_slice/m59_04_seventh_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m59_04_seventh_slice_recipe_pipeline_entry_gate.md
```

## Verification

```powershell
python -m unittest tests.test_seventh_slice_recipe_pipeline_entry_gate
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M59-02 and M59-03 outputs exist:

```powershell
python tools\deck\build_seventh_slice_recipe_pipeline_entry_gate.py
```
