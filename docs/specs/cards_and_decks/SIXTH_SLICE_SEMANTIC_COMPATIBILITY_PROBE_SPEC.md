# Sixth-Slice Semantic / Compatibility Probe Spec

Milestone: `M55-03`

## Purpose

`M55-03` runs the sixth selected slice through the existing selected-slice
semantic and compatibility pipeline before allowing recipe pipeline work.

The selected target is:

```text
Shadow Paladin / g_next_z
```

This probe is advisory only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, G Zone runtime, Stride runtime, or
mutate `GameState`.

## Inputs

```text
outputs/target_slice/m55_01_sixth_target_slice_selection.json
outputs/target_slice/m55_02_sixth_slice_fixture_readiness.json
data/packs/vanguard_th/cards.sqlite
```

## Pipeline

`M55-03` normalizes the M55 selection/readiness reports into the existing
selected-slice probe contract, then runs these stages in memory:

1. B1 semantic vocabulary
2. B2 semantic tags
3. B3 requirement/provider model
4. B4 manual review queue
5. C1 pair compatibility graph
6. C2 resource detector
7. C3 timing detector
8. C4 zone/target detector
9. C5 selected compatibility output

The wrapper must not write intermediate M35 artifacts. Only the M55-03 report
and markdown summary are produced.

## Expected Result

The probe passes when M55-02 semantic readiness is true and all stage readiness
flags pass. It then routes to `M55-04`.

Current source-backed result:

```text
semantic cards: 77
manual-review cards: 11
pair graph edges: 2069
candidate edges: 70
```

## Runtime Boundary

- Advisory offline probe only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No G Zone or Stride runtime enablement.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m55_03_sixth_slice_semantic_compatibility_probe.json
outputs/target_slice/m55_03_sixth_slice_semantic_compatibility_probe.md
```

## Verification

```powershell
python tools\deck\build_sixth_slice_semantic_compatibility_probe.py
python -m unittest tests.test_sixth_slice_semantic_compatibility_probe
python -m unittest discover -s tests -p "test_*.py"
```
