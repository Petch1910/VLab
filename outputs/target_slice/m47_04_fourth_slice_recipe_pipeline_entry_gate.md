# M47-04 Fourth-Slice Recipe Pipeline Entry Gate

## Selected Target

- Group: `รอยัล พาลาดิน`
- Era preset: `g_series_first`
- Applied expansion: `g_era_heal_expansion`

## Decision

- Decision ready: `True`
- Offline recipe pipeline allowed: `True`
- Fixture scaffold required before recipe validation: `True`
- Runtime deck promotion allowed: `False`
- Saved deck/UI publication allowed: `False`
- Bot playbook promotion allowed: `False`
- Blocking issues: `0`

## Evidence

- Source card count: `190`
- Effective series count: `32`
- Semantic card count: `190`
- Manual-review card count: `15`
- Pair graph edges: `14150`
- Candidate edges: `785`
- Policy reuse decision: `requires_fourth_slice_fixture_scaffold`

## Proposed M48 Queue

- `M48-01`: Fourth-slice fixture scaffold - Define source-backed fixture policy for the Royal Paladin G-era expanded slice before validator work.
- `M48-02`: Fourth-slice review packet - Export candidate edges, manual-review cards, applied scope notes, and format notes for human review.
- `M48-03`: Fourth-slice recipe draft model - Create advisory recipe drafts only; no saved deck or UI injection.
- `M48-04`: Fourth-slice recipe validator - Validate count, trigger, grade, identity, copy limits, missing cards, and fixture scaffold constraints.
- `M48-05`: Fourth-slice combo-to-recipe consistency - Check candidate combo cards are present and not blocked by manual-review dependency.
- `M48-06`: Fourth-slice blocker repair candidates - Generate source-backed repair candidates for blocked recipes.
- `M48-closeout`: Fourth-slice runtime readiness decision - Decide whether any recipe can later enter human acceptance and runtime fixture gates.

## Boundary

- `decision_artifact_only`: `True`
- `does_not_create_deck`: `True`
- `does_not_create_recipe_draft`: `True`
- `does_not_create_runtime_fixture`: `True`
- `does_not_mutate_runtime_pack`: `True`
- `does_not_publish_to_ui`: `True`
- `does_not_publish_to_bot`: `True`
- `GameState_mutation`: `False`

## Issues

- None

## Next

`M48-01`: Fourth-slice fixture scaffold.
