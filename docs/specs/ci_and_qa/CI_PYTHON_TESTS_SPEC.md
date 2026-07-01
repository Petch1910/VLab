# CI Python Tests Spec

## Milestone

`M18-01`

## Goal

Add the first GitHub Actions workflow for Python unit tests. This milestone
keeps CI narrow and aligned with the existing local baseline.

## Workflow

`.github/workflows/python-tests.yml`

Triggers:

- `push`
- `pull_request`

Job:

- runner: `ubuntu-latest`
- Python: `3.11`
- command: `python -m unittest discover -s tests -p "test_*.py"`

## Non-Goals

- No Unity compile CI. That is `M18-03`.
- No Unity EditMode CI. That is `M18-04`.
- No data validation CI. That is `M18-02`.
- No release artifacts.

## Verification

- Local Python unit test command passes.
- Workflow command matches the local baseline command exactly.
