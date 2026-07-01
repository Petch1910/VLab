# Fourth-Slice Applied Scope Spec

Milestone: `M47-repair-apply-scope`

## Purpose

`M47-repair-apply-scope` applies the reviewed Royal Paladin G-era source
expansion to the offline fixture pipeline by writing a bounded scope artifact.
It does not edit card data or create runtime decks.

## Inputs

- `outputs/target_slice/m47_repair_expand_scope_review.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m47_repair_apply_scope.json`
- `outputs/target_slice/m47_repair_apply_scope.md`

## Checks

The applied scope must:

- use the recommended expansion from the review artifact
- preserve the original base series scope
- add only the reviewed same-group G-era Heal series
- re-evaluate source-backed card counts from SQLite
- close all classic trigger gaps
- expose readiness for the next semantic/compatibility probe

## Boundary

This milestone may write the offline applied-scope artifact only.

It must not:

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
python tools\deck\build_fourth_slice_applied_scope.py
python -m unittest tests.test_fourth_slice_applied_scope
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M47-repair-apply-scope` is done when the applied scope artifact is generated,
the scope re-evaluation is fixture-ready, tests cover the applied boundary, docs
are updated, and the next target is `M47-03`.
