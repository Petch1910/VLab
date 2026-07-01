# Selected Slice Semantic / Compatibility Probe Spec

Milestone: `M35-E3`

## Purpose

Generalize the M35 semantic and compatibility workflow so it can be driven by a
selected-slice report instead of being tied to the first selected slice.

The proof target is the second selected slice:

```text
Classic Core / Oracle Think Tank
```

## Inputs

```text
outputs/target_slice/m35_e1_second_target_slice_report.json
outputs/target_slice/m35_e2_second_slice_fixture_readiness.json
data/packs/vanguard_th/cards.sqlite
```

## Pipeline Contract

The selected report must provide:

- `selected_target`
- `format_policy`

The readiness report must provide:

- `selected_target`
- `fixture_policy`
- `readiness`

The generalized probe runs the following stages with injected report data:

1. B1 semantic vocabulary
2. B2 semantic tags
3. B3 requirement/provider model
4. B4 manual review queue
5. C1 pair compatibility graph
6. C2 resource detector
7. C3 timing detector
8. C4 zone/target detector
9. C5 selected compatibility output

## Runtime Boundary

- Advisory probe only.
- No player-deck mutation.
- No runtime card pack mutation.
- No bot/runtime playbook publication.
- No playbook seed publication.
- E3 output edges are not runtime instructions.

## Result

The `Classic Core / Oracle Think Tank` probe passes all B1-C5 readiness gates:

```text
cards: 103
compatibility edges: 2660
candidate edges: 259
manual review cards: 7
```

## Outputs

```text
outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.json
outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.md
```

## Verification

```powershell
python tools\deck\build_selected_slice_semantic_compatibility_probe.py
python -m unittest tests.test_selected_slice_semantic_compatibility_probe
python -m unittest discover -s tests -p "test_*.py"
```
