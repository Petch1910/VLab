# Third-Slice Human-Accepted Repair Artifact Spec

Milestone: `M45-02`

## Purpose

`M45-02` records explicit acceptance of one third-slice repair candidate from
the `M45-01` review packet.

The third-slice repair packet carries both manual-overlap substitutions and a
grade-profile repair package. The source grade-profile package was produced
from the original M44-03 draft, so `M45-02` must not apply it blindly after
manual substitutions. Instead, it records conflicts against the source grade
package and recomputes a combined grade-profile package after the manual repair
preview.

This artifact still does not declare the recipe valid. `M45-03` must rerun
validation on the repaired quantity preview.

## Inputs

- `outputs/target_slice/m45_01_third_slice_human_repair_review_packet.json`
- `outputs/target_slice/m44_03_third_slice_recipe_draft_model.json`
- `outputs/target_slice/m44_01_third_slice_fixture_scaffold.json`

## Outputs

- `outputs/target_slice/m45_02_third_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m45_02_third_slice_human_accepted_repair_artifact.md`

## Default Selection

The current artifact accepts:

- review item: `m45_01_m44_recipe_001_repair_review`
- recipe: `m44_recipe_001`
- manual repair package: `m44_recipe_001_manual_overlap_pkg_001`
- source grade package: `m44_recipe_001_grade_profile_pkg_001`
- combined repair package: `m44_recipe_001_combined_manual_grade_pkg_001`

The selection is the first-ranked complete repair candidate from the M45-01
packet.

## Combined Repair Rule

The accepted preview must:

1. Apply manual substitutions to a cloned quantity map.
2. Detect source grade-package removals that conflict after manual
   substitutions.
3. Recompute grade-profile substitutions against the manual-repaired quantity
   map.
4. Export a repaired quantity preview totaling `50` cards.
5. Keep runtime promotion disabled until later validation and fixture gates.

## Runtime Boundary

This milestone must not:

- mutate M45-01 review packet files
- mutate M44-03 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`
- declare the recipe valid before M45-03 validation rerun

## Verification

```powershell
python tools\deck\build_third_slice_human_accepted_repair_artifact.py --accepted-at 2026-06-30
python -m unittest tests.test_third_slice_human_accepted_repair_artifact
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M45-02` is done when:

- one review item is explicitly recorded as accepted
- source grade-package conflicts after manual repair are visible
- combined grade repair is recomputed after manual substitutions
- repaired quantity preview totals `50` cards
- repaired grade profile equals `{0:17, 1:14, 2:11, 3:8}`
- output still marks runtime promotion disabled
- output still says validation must run in `M45-03`
- project status docs point the active queue to `M45-03`
