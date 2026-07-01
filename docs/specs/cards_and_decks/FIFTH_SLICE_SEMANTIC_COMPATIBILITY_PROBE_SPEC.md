# Fifth-Slice Semantic / Compatibility Probe Spec

Milestone: `M51-03`

## Purpose

`M51-03` runs the fifth selected slice through the existing selected-slice
semantic and compatibility pipeline before allowing recipe pipeline work.

The selected target is:

```text
Gold Paladin / link_joker_legion_mate
```

This probe is advisory only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, G Zone runtime, Stride runtime, or
mutate `GameState`.

## Inputs

```text
outputs/target_slice/m51_01_fifth_target_slice_selection.json
outputs/target_slice/m51_02_fifth_slice_fixture_readiness.json
data/packs/vanguard_th/cards.sqlite
```

## Pipeline

`M51-03` normalizes the M51 selection/readiness reports into the existing
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

The wrapper must not write intermediate M35 artifacts. Only the M51-03 report
and markdown summary are produced.

## Expected Result

The probe passes when M51-02 semantic readiness is true and all stage readiness
flags pass. It then routes to `M51-04`.

Current source-backed result:

```text
semantic cards: 106
manual-review cards: 4
pair graph edges: 3075
candidate edges: 142
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
outputs/target_slice/m51_03_fifth_slice_semantic_compatibility_probe.json
outputs/target_slice/m51_03_fifth_slice_semantic_compatibility_probe.md
```

## Verification

```powershell
python tools\deck\build_fifth_slice_semantic_compatibility_probe.py
python -m unittest tests.test_fifth_slice_semantic_compatibility_probe
python -m unittest discover -s tests -p "test_*.py"
```
