# Third Slice Semantic / Compatibility Probe Spec

Milestone: `M43-03`

## Purpose

Run the third selected slice through the existing selected-slice semantic and
compatibility pipeline before allowing recipe pipeline work.

The selected target is:

```text
Bermuda Triangle / link_joker_legion_mate
```

This probe is advisory only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m43_01_third_target_slice_selection.json
outputs/target_slice/m43_02_third_slice_fixture_readiness.json
data/packs/vanguard_th/cards.sqlite
```

## Pipeline

`M43-03` normalizes the M43 selection/readiness reports into the existing
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

The wrapper must not write intermediate M35 artifacts. Only the M43-03 report
and markdown summary are produced.

## Expected Result

The probe passes when all stage readiness flags pass and the output routes to
`M43-04`.

Current source-backed result:

```text
semantic cards: 127
manual-review cards: 61
pair graph edges: 4835
candidate edges: 109
```

## Runtime Boundary

- Advisory offline probe only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m43_03_third_slice_semantic_compatibility_probe.json
outputs/target_slice/m43_03_third_slice_semantic_compatibility_probe.md
```

## Verification

```powershell
python tools\deck\build_third_slice_semantic_compatibility_probe.py
python -m unittest tests.test_third_slice_semantic_compatibility_probe
python -m unittest discover -s tests -p "test_*.py"
```
