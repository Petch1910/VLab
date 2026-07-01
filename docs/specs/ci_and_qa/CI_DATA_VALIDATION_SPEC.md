# CI Data Validation Spec

## Milestone

`M18-02`

## Goal

Add a GitHub Actions workflow for data validation commands that already pass
locally. This keeps data validation separate from Python unit tests and Unity
CI.

## Workflow

`.github/workflows/data-validation.yml`

Triggers:

- `push`
- `pull_request`

Job:

- runner: `ubuntu-latest`
- Python: `3.11`

Commands:

```bash
python tools/verification/verify_vanguard_th_pack.py
python tools/data/validate_custom_pack_schema.py data/templates/custom_pack
python tools/data/validate_custom_pack_schema.py data/templates/custom_pack_v2
python tools/data/import_custom_pack.py data/templates/custom_pack --output-dir work/custom_pack_import --overwrite
python tools/data/import_custom_pack.py data/templates/custom_pack_v2 --output-dir work/custom_pack_v2_import --overwrite
python tools/data/validate_ability_schema.py data/packs/vanguard_th/abilities/structured_ability_pack_m12_10.json
```

## Non-Goals

- No Python unit tests. That is `M18-01`.
- No Unity compile CI. That is `M18-03`.
- No Unity EditMode CI. That is `M18-04`.
- No build artifacts.

## Verification

- Local data validation commands pass.
- Workflow commands match the local validation baseline.
