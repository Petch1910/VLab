# Custom Import Workflow Test Rollup Spec

Milestone: `M24-09`

## Purpose

Collect and harden the import/custom pack workflow regression coverage after
M24-02 through M24-08.

This milestone is a test/verification rollup, not a new import feature.

## Required Coverage

- Count-line deck parser/codec coverage remains passing.
- Local custom import validator coverage remains passing.
- Custom pack schema validator coverage remains passing.
- Failed custom pack import must not mutate or delete an existing output
  directory.
- Custom pack import output must remain isolated from the default runtime
  `vanguard_th` pack.

## Boundaries

- Do not auto-download public CGS/comparator data.
- Do not import into the active runtime pack.
- Do not move or delete existing KK Card Fight export data.
- Do not add Android/mobile/release verification.

## Verification

Run Python unit tests:

```powershell
python -m unittest discover -s tests -p "test_*.py"
```

Unity tests are not required unless C#/Unity files change during this milestone.

