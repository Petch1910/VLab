# Eighth-Slice Recipe Pipeline Entry Gate Spec

Milestone: `M63-04`

## Purpose

Decide whether the eighth selected slice may enter offline recipe pipeline work.

This is a gate artifact only. It must not create deck recipes, runtime
fixtures, saved decks, UI deck entries, bot playbooks, G Zone runtime, Stride
runtime, Bloom/token runtime, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m63_02_eighth_slice_fixture_readiness.json
outputs/target_slice/m63_03_eighth_slice_semantic_compatibility_probe.json
```

Tests may use in-memory M63-02 and M63-03 reports. Real CLI artifacts remain
gated until the real upstream files exist.

## Gate Rules

The gate passes only when:

- `M63-02` fixture/format readiness passed
- `M63-02` marks the selected slice ready for semantic probe
- `M63-03` semantic/compatibility probe passed
- `M63-03` produced at least one candidate edge
- neither input allows runtime or bot promotion

Because the selected slice is `link_joker_legion_mate`, recipe validation must
start with a fixture scaffold before any recipe validator work. G Zone, Stride,
and Bloom/token runtime remain disabled.

## Allowed Next Work

If the gate passes, the next queue is `M64`:

```text
M64-01 Eighth-slice fixture scaffold
M64-02 Eighth-slice review packet
M64-03 Eighth-slice recipe draft model
M64-04 Eighth-slice recipe validator
M64-05 Eighth-slice combo-to-recipe consistency
M64-06 Eighth-slice blocker repair candidates
M64-closeout Eighth-slice runtime readiness decision
```

## Runtime Boundary

- Decision artifact only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No G Zone, Stride, or Bloom/token runtime enablement.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m63_04_eighth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m63_04_eighth_slice_recipe_pipeline_entry_gate.md
```

## Verification

```powershell
python -m unittest tests.test_eighth_slice_recipe_pipeline_entry_gate
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M63-02 and M63-03 outputs exist:

```powershell
python tools\deck\build_eighth_slice_recipe_pipeline_entry_gate.py
```
