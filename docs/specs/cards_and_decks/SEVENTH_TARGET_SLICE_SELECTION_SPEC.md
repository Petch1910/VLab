# Seventh Target Slice Selection Spec

Milestone: `M59-01`

## Purpose

`M59-01` selects the next clan/nation group for offline analysis after the
six-fixture scale gate. It consumes the `M58-04` six-fixture scale decision and
chooses the first remaining candidate from that decision's candidate queue.

This milestone is a scaffold-safe target selection step only. It does not build
recipes, fixtures, saved decks, UI entries, bot playbooks, G Zone runtime, or
Stride runtime.

## Inputs

- `outputs/target_slice/m58_04_six_fixture_scale_decision.json`

Tests may pass an in-memory `M58-04` report built from first-five real fixture
smoke reports plus an in-memory sixth fixture smoke report. Real CLI artifacts
remain gated until the real `M58-04` report exists.

## Output Artifacts

Real CLI execution writes:

- `outputs/target_slice/m59_01_seventh_target_slice_selection.json`
- `outputs/target_slice/m59_01_seventh_target_slice_selection.md`

These artifacts are generated only when the real `M58-04` input file exists and
allows `M59-01`.

## Selection Rules

The selector may choose a target only when:

- `M58-04.summary.ready_for_m59` is `true`
- `M58-04.summary.seventh_slice_offline_pipeline_allowed` is `true`
- `M58-04.decision.seventh_slice_offline_pipeline_allowed` is `true`
- `M58-04.candidate_queue` has at least one item

The selected target must be copied from the first candidate queue item and must
preserve:

- rank
- group
- group field
- best era preset as `era_preset`
- priority score
- mechanic tier metadata
- priority reasons

If any rule fails, the output routes to `M58-repair`.

## Non-Mutation Boundaries

`M59-01` must keep all of these disabled:

- recipe draft creation
- runtime fixture creation
- saved deck injection
- UI deck list publication
- bot/playbook promotion
- G Zone runtime
- Stride runtime
- `GameState` mutation

## Verification

Targeted tests:

```powershell
python -m unittest tests.test_seventh_target_slice_selection
```

Full Python regression:

```powershell
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after `M58-04` output exists:

```powershell
python tools\deck\build_seventh_target_slice_selection.py
```

## Done

`M59-01` scaffold work is ready when the selector and tests prove that the first
candidate can be selected from an in-memory `M58-04` scale decision, failed
scale decisions route to repair, output round-trips as JSON/Markdown, and all
runtime/UI/bot/G Zone/Stride/GameState boundaries remain disabled.
