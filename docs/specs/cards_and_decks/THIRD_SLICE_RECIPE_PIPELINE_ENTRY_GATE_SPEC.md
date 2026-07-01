# Third Slice Recipe Pipeline Entry Gate Spec

Milestone: `M43-04`

## Purpose

Decide whether the third selected slice may enter offline recipe pipeline work.

This is a gate artifact only. It must not create deck recipes, runtime
fixtures, saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m43_02_third_slice_fixture_readiness.json
outputs/target_slice/m43_03_third_slice_semantic_compatibility_probe.json
```

## Gate Rules

The gate passes only when:

- `M43-02` fixture/format readiness passed
- `M43-02` marks the selected slice ready for semantic probe
- `M43-03` semantic/compatibility probe passed
- `M43-03` produced at least one candidate edge
- neither input allows runtime or bot promotion

Because the selected slice is `link_joker_legion_mate`, recipe validation must
start with a fixture scaffold before any recipe validator work.

## Allowed Next Work

If the gate passes, the next queue is `M44`:

```text
M44-01 Third-slice fixture scaffold
M44-02 Third-slice review packet
M44-03 Third-slice recipe draft model
M44-04 Third-slice recipe validator
M44-05 Third-slice combo-to-recipe consistency
M44-06 Third-slice blocker repair candidates
M44-closeout Third-slice runtime readiness decision
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
outputs/target_slice/m43_04_third_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m43_04_third_slice_recipe_pipeline_entry_gate.md
```

## Verification

```powershell
python tools\deck\build_third_slice_recipe_pipeline_entry_gate.py
python -m unittest tests.test_third_slice_recipe_pipeline_entry_gate
python -m unittest discover -s tests -p "test_*.py"
```
