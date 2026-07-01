# Fourth-Slice Human-Accepted Repair Artifact Spec

Milestone: `M49-03`

## Purpose

`M49-03` records explicit user acceptance of one repaired fourth-slice
main-deck candidate after the `M49-02` G Zone boundary decision.

This artifact does not declare the recipe valid. It only records the accepted
repair preview that `M49-04` must validate.

## Inputs

- `outputs/target_slice/m49_01_fourth_slice_human_repair_review_packet.json`
- `outputs/target_slice/m49_02_fourth_slice_g_zone_support_decision.json`
- `outputs/target_slice/m48_03_fourth_slice_recipe_draft_model.json`
- `outputs/target_slice/m48_01_fourth_slice_fixture_scaffold.json`

## Outputs

- `outputs/target_slice/m49_03_fourth_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m49_03_fourth_slice_human_accepted_repair_artifact.md`

## Default Selection

The current artifact accepts:

- review item: `m49_01_m48_recipe_001_repair_review`
- recipe: `m48_recipe_001`
- manual repair package: `m48_recipe_001_manual_overlap_pkg_001`
- source grade package: `m48_recipe_001_grade_profile_pkg_001`
- combined repair package: `m48_recipe_001_combined_manual_grade_pkg_001`

The selection is the first-ranked complete repair candidate from the `M49-01`
packet and is accepted under the `M49-02`
`main_deck_only_for_current_windows_fixture` boundary.

## G Zone Boundary Rule

Acceptance is valid only when the matching `M49-02` decision item says:

- `selected_option_id=main_deck_only_for_current_windows_fixture`
- `main_deck_only_validation_allowed=true`
- `g_zone_runtime_enabled=false`
- `stride_runtime_enabled=false`

`M49-03` must not enable G Zone slots, Stride, Generation Break runtime, Grade 4
main-deck usage, or G-unit runtime usage.

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

- mutate M49-01 review packet files
- mutate M49-02 G Zone decision files
- mutate M48-03 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`
- declare the recipe valid before M49-04 validation rerun

## Verification

```powershell
python tools\deck\build_fourth_slice_human_accepted_repair_artifact.py --accepted-at 2026-06-30
python -m unittest tests.test_fourth_slice_human_accepted_repair_artifact
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M49-03` is done when:

- one review item is explicitly recorded as accepted
- the accepted item is guarded by the M49-02 main-deck-only boundary
- combined grade repair is recomputed after manual substitutions
- repaired quantity preview totals `50` cards
- repaired grade profile equals `{0:17, 1:14, 2:11, 3:8}`
- G Zone and Stride runtime remain disabled
- output still marks runtime promotion disabled
- output still says validation must run in `M49-04`
- project status docs point the active queue to `M49-04`
