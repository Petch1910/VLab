# Fourth-Slice Semantic / Compatibility Probe Spec

Milestone: `M47-03`

## Purpose

`M47-03` runs the fourth-slice Royal Paladin semantic/compatibility probe using
the applied offline source scope from `M47-repair-apply-scope`.

The probe reuses the existing M35 B/C advisory pipeline:

- semantic vocabulary
- semantic tag extraction
- requirement/provider model
- manual review queue
- pair compatibility graph
- resource, timing, and zone detectors
- selected compatibility output

## Inputs

- `outputs/target_slice/m47_01_fourth_target_slice_selection.json`
- `outputs/target_slice/m47_repair_apply_scope.json`

## Outputs

- `outputs/target_slice/m47_03_fourth_slice_semantic_compatibility_probe.json`
- `outputs/target_slice/m47_03_fourth_slice_semantic_compatibility_probe.md`

## Checks

The probe must:

- use `m47_repair_apply_scope.json` as the applied source scope
- pass the applied effective series scope through `format_policy.series_scope`
- keep all generated data advisory-only
- require all B/C stage readiness flags to pass
- route to `M47-04` only when all stages pass

## Boundary

This milestone must not:

- edit card data
- create recipe drafts
- create runtime fixtures
- mutate runtime packs
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fourth_slice_semantic_compatibility_probe.py
python -m unittest tests.test_fourth_slice_semantic_compatibility_probe
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M47-03` is done when the probe uses the applied source scope, all stage
readiness flags pass, tests cover the advisory boundary, docs are updated, and
the next target is `M47-04`.
