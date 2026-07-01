# M51-04 Fifth-Slice Recipe Pipeline Entry Gate

## Selected Target

- Group: `โกลด์ พาลาดิน`
- Era preset: `link_joker_legion_mate`

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

- Source card count: `106`
- Semantic card count: `106`
- Manual-review card count: `4`
- Pair graph edges: `3075`
- Candidate edges: `142`
- Policy reuse decision: `requires_fifth_slice_fixture_scaffold`

## Proposed M52 Queue

- `M52-01`: Fifth-slice fixture scaffold - Define source-backed fixture policy for the Gold Paladin Link Joker/Legion Mate slice before validator work.
- `M52-02`: Fifth-slice review packet - Export candidate edges, manual-review cards, and format notes for human review.
- `M52-03`: Fifth-slice recipe draft model - Create advisory recipe drafts only; no saved deck or UI injection.
- `M52-04`: Fifth-slice recipe validator - Validate count, trigger, grade, identity, copy limits, missing cards, and fixture scaffold constraints.
- `M52-05`: Fifth-slice combo-to-recipe consistency - Check candidate combo cards are present and not blocked by manual-review dependency.
- `M52-06`: Fifth-slice blocker repair candidates - Generate source-backed repair candidates for blocked recipes.
- `M52-closeout`: Fifth-slice runtime readiness decision - Decide whether any recipe can later enter human acceptance and runtime fixture gates.

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

`M52-01`: Fifth-slice fixture scaffold.
