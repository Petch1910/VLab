# Second-Slice Trigger Repair Acceptance Artifact Spec

Milestone: `M41-repair-accept`

## Purpose

`M41-repair-accept` records acceptance of one trigger/profile repair package
from `M41-repair`.

This artifact applies the accepted trigger repair in memory and prepares a new
repaired recipe preview for validation. It does not declare the recipe valid and
does not promote runtime fixtures, saved decks, UI entries, or bot/playbook
data.

## Inputs

- `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.json`

## Outputs

- `outputs/target_slice/m41_repair_accept_second_slice_trigger_repair_artifact.json`
- `outputs/target_slice/m41_repair_accept_second_slice_trigger_repair_artifact.md`

## Accepted Package

The current artifact accepts:

- package: `m41_repair_pkg_001`
- profile: `balanced_classic_trigger_restore`

The package is expected to restore:

- `Critical=4`
- `Draw=4`
- `Heal=4`
- `Stand=4`
- grade counts `G0=17/G1=14/G2=11/G3=8`

Validation still belongs to `M41-repair-validate`.

## Runtime Boundary

This milestone must not:

- mutate previous artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`
- declare the recipe valid before validation rerun

## Verification

```powershell
python tools\deck\build_second_slice_trigger_repair_acceptance_artifact.py --accepted-at 2026-06-30
python -m unittest tests.test_second_slice_trigger_repair_acceptance_artifact
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M41-repair-accept` is done when:

- one trigger repair package is accepted
- repaired quantity preview totals `50` cards
- output remains non-runtime
- output still requires validation rerun
- project status docs point the active queue to `M41-repair-validate`
