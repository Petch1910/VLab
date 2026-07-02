# Eighth-Slice Semantic / Compatibility Probe Spec

Milestone: `M63-03`

## Purpose

`M63-03` runs the eighth selected slice through the existing selected-slice
semantic and compatibility pipeline before allowing recipe pipeline work.

This probe is advisory only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, G Zone runtime, Stride runtime,
Bloom/token runtime, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m63_01_eighth_target_slice_selection.json
outputs/target_slice/m63_02_eighth_slice_fixture_readiness.json
data/packs/vanguard_th/cards.sqlite
```

Tests may pass in-memory M63-01 and M63-02 reports. Real CLI artifacts remain
gated until the real upstream files exist.

## Pipeline

`M63-03` normalizes the M63 selection/readiness reports into the existing
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

The wrapper must not write intermediate M35 artifacts. Only the M63-03 report
and markdown summary are produced.

## Expected Result

The probe passes when M63-02 semantic readiness is true and all stage readiness
flags pass. It then routes to `M63-04`.

The scaffold-selected target is:

```text
Kagero / link_joker_legion_mate
```

The scaffold result is source-backed by:

```text
semantic cards: 121
manual-review cards: 6
pair graph edges: 3398
candidate edges: 355
```

## Runtime Boundary

- Advisory offline probe only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No G Zone, Stride, or Bloom/token runtime enablement.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m63_03_eighth_slice_semantic_compatibility_probe.json
outputs/target_slice/m63_03_eighth_slice_semantic_compatibility_probe.md
```

## Verification

```powershell
python -m unittest tests.test_eighth_slice_semantic_compatibility_probe
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M63-01 and M63-02 outputs exist:

```powershell
python tools\deck\build_eighth_slice_semantic_compatibility_probe.py
```
