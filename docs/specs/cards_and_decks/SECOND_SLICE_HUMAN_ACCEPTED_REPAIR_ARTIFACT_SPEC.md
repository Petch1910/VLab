# Second-Slice Human-Accepted Repair Artifact Spec

Milestone: `M41-02`

## Purpose

`M41-02` records explicit acceptance of one Oracle Think Tank repair candidate
from the `M41-01` review packet.

This artifact records the selected repair and applies its add/remove delta in
memory so the next milestone can rerun validation. It does not declare the
recipe valid and does not promote runtime fixtures, saved decks, UI entries, or
bot/playbook data.

## Inputs

- `outputs/target_slice/m41_01_second_slice_human_repair_review_packet.json`
- `outputs/target_slice/m40_02_second_slice_recipe_draft_model.json`

## Outputs

- `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.md`

## Default Selection

The current artifact accepts:

- review item: `m41_01_m40_recipe_001_repair_review`
- recipe: `m40_recipe_001`
- repair package: `m40_recipe_001_grade_profile_pkg_001`

The selection is the first-ranked complete repair candidate from the M41-01
packet. M41-03 is still required to validate the repaired recipe.

## Runtime Boundary

This milestone must not:

- mutate M40-02 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`
- declare the recipe valid before M41-03 validation rerun

## Verification

```powershell
python tools\deck\build_second_slice_human_accepted_repair_artifact.py --accepted-at 2026-06-30
python -m unittest tests.test_second_slice_human_accepted_repair_artifact
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M41-02` is done when:

- one review item is explicitly recorded as accepted
- repaired quantity preview totals `50` cards
- output still marks runtime promotion disabled
- output still says validation must run in `M41-03`
- project status docs point the active queue to `M41-03`
