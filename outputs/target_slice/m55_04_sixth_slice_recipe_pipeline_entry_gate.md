# M55-04 Sixth-Slice Recipe Pipeline Entry Gate

## Selected Target

- Group: `ชาโดว์ พาลาดิน`
- Era preset: `g_next_z`

## Decision

- Decision ready: `True`
- Offline recipe pipeline allowed: `True`
- Fixture scaffold required before recipe validation: `True`
- Runtime deck promotion allowed: `False`
- Saved deck/UI publication allowed: `False`
- Bot playbook promotion allowed: `False`
- G Zone runtime enabled: `False`
- Stride runtime enabled: `False`
- Blocking issues: `0`

## Evidence

- Source card count: `77`
- Semantic card count: `77`
- Manual-review card count: `11`
- Pair graph edges: `2069`
- Candidate edges: `70`
- Policy reuse decision: `requires_sixth_slice_fixture_scaffold`

## Proposed M56 Queue

- `M56-01`: Sixth-slice fixture scaffold - Define source-backed fixture policy for the Shadow Paladin G NEXT/Z slice before validator work.
- `M56-02`: Sixth-slice review packet - Export candidate edges, manual-review cards, and format notes for human review.
- `M56-03`: Sixth-slice recipe draft model - Create advisory recipe drafts only; no saved deck or UI injection.
- `M56-04`: Sixth-slice recipe validator - Validate count, trigger, grade, identity, copy limits, missing cards, and fixture scaffold constraints.
- `M56-05`: Sixth-slice combo-to-recipe consistency - Check candidate combo cards are present and not blocked by manual-review dependency.
- `M56-06`: Sixth-slice blocker repair candidates - Generate source-backed repair candidates for blocked recipes.
- `M56-closeout`: Sixth-slice runtime readiness decision - Decide whether any recipe can later enter human acceptance and runtime fixture gates.

## Boundary

- `decision_artifact_only`: `True`
- `does_not_create_deck`: `True`
- `does_not_create_recipe_draft`: `True`
- `does_not_create_runtime_fixture`: `True`
- `does_not_mutate_runtime_pack`: `True`
- `does_not_publish_to_ui`: `True`
- `does_not_publish_to_bot`: `True`
- `g_zone_runtime_enabled`: `False`
- `stride_runtime_enabled`: `False`
- `GameState_mutation`: `False`

## Issues

- None

## Next

`M56-01`: Sixth-slice fixture scaffold.
