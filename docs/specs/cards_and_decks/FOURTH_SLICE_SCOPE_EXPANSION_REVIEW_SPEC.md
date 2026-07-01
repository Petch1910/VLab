# Fourth-Slice Scope Expansion Review Spec

Milestone: `M47-repair-expand-scope`

## Purpose

`M47-repair-expand-scope` reviews whether the Royal Paladin fourth slice can be
repaired by expanding source scope to same-group G-era series that contain Heal
triggers.

This milestone is review-only. It does not apply the scope expansion or mutate
card data.

## Inputs

- `outputs/target_slice/m47_02_fourth_slice_fixture_readiness.json`
- `outputs/target_slice/m47_repair_fourth_slice_readiness_blockers.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m47_repair_expand_scope_review.json`
- `outputs/target_slice/m47_repair_expand_scope_review.md`

## Checks

The review must compare:

- no expansion
- G-era same-group Heal expansion
- all same-group Heal series expansion

The recommended path is G-era same-group Heal expansion only if it closes the
trigger gap and keeps fixture expectations met.

## Boundary

This milestone must not:

- apply the selected expansion
- edit card data
- create recipe drafts
- create runtime fixtures
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fourth_slice_scope_expansion_review.py
python -m unittest tests.test_fourth_slice_scope_expansion_review
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M47-repair-expand-scope` is done when the report recommends a source expansion
or rejects all expansions with reasons, tests cover the recommendation, docs
are updated, and the next target is `M47-repair-apply-scope` when an expansion
is safe to apply.
