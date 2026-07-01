# First Slice Deck Skeleton Ratio Planner Spec

Milestone: `M35-D2`

## Purpose

Convert M35-D1 candidate packages into advisory deck skeleton ratio plans.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_d1_first_slice_candidate_packages.json
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json
data/packs/vanguard_th/cards.sqlite
```

## Ratio Target

M35-D2 uses a Classic-era advisory ratio target:

```text
main deck: 50
trigger slots: 16
normal unit slots: 34
grade 0 target: 17
grade 1 target: 14
grade 2 target: 11
grade 3 target: 8
```

Trigger ratios are emitted as bands, not final trigger card choices.

## Guard / Shield Profile

M35-D2 may read `power`, `shield`, `deck_limit`, `grade`, and `trigger` from
runtime SQLite. The guard/shield profile is package-local and must be marked as
not a full-deck shield profile.

## Hard Boundaries

- no per-card quantities
- no final deck list
- no playbook promotion
- no bot runtime data
- no runtime effect execution
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.json
outputs/target_slice/m35_d2_first_slice_deck_skeleton_ratio_plans.md
```

## Verification

```powershell
python tools\deck\build_first_slice_deck_skeleton_ratio_planner.py
python -m unittest tests.test_first_slice_deck_skeleton_ratio_planner
python -m unittest discover -s tests -p "test_*.py"
```
