# Fifth-Slice Recipe Pipeline Entry Gate Spec

Milestone: `M51-04`

## Purpose

Decide whether the fifth selected slice may enter offline recipe pipeline work.

This is a gate artifact only. It must not create deck recipes, runtime
fixtures, saved decks, UI deck entries, bot playbooks, G Zone runtime, Stride
runtime, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m51_02_fifth_slice_fixture_readiness.json
outputs/target_slice/m51_03_fifth_slice_semantic_compatibility_probe.json
```

## Gate Rules

The gate passes only when:

- `M51-02` fixture/format readiness passed
- `M51-02` marks the selected slice ready for semantic probe
- `M51-03` semantic/compatibility probe passed
- `M51-03` produced at least one candidate edge
- neither input allows runtime or bot promotion

Because the selected slice is `link_joker_legion_mate`, recipe validation must
start with a fixture scaffold before any recipe validator work.

## Allowed Next Work

If the gate passes, the next queue is `M52`:

```text
M52-01 Fifth-slice fixture scaffold
M52-02 Fifth-slice review packet
M52-03 Fifth-slice recipe draft model
M52-04 Fifth-slice recipe validator
M52-05 Fifth-slice combo-to-recipe consistency
M52-06 Fifth-slice blocker repair candidates
M52-closeout Fifth-slice runtime readiness decision
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
outputs/target_slice/m51_04_fifth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m51_04_fifth_slice_recipe_pipeline_entry_gate.md
```

## Verification

```powershell
python tools\deck\build_fifth_slice_recipe_pipeline_entry_gate.py
python -m unittest tests.test_fifth_slice_recipe_pipeline_entry_gate
python -m unittest discover -s tests -p "test_*.py"
```
