# M43-04 Third-Slice Recipe Pipeline Entry Gate

## Selected Target

- Group: `เบอร์มิวด้า ไทรแองเกิล`
- Era preset: `link_joker_legion_mate`

## Decision

- Decision ready: `True`
- Offline recipe pipeline allowed: `True`
- Fixture scaffold required before recipe validation: `True`
- Runtime deck promotion allowed: `False`
- Saved deck/UI publication allowed: `False`
- Bot playbook promotion allowed: `False`
- Blocking issues: `0`

## Evidence

- Source card count: `127`
- Semantic card count: `127`
- Manual-review card count: `61`
- Pair graph edges: `4835`
- Candidate edges: `109`
- Policy reuse decision: `requires_third_slice_fixture_scaffold`

## Proposed M44 Queue

- `M44-01`: Third-slice fixture scaffold - Define source-backed fixture policy for the Link Joker/Legion Mate slice before validator work.
- `M44-02`: Third-slice review packet - Export candidate edges, manual-review cards, and format notes for human review.
- `M44-03`: Third-slice recipe draft model - Create advisory recipe drafts only; no saved deck or UI injection.
- `M44-04`: Third-slice recipe validator - Validate count, trigger, grade, identity, copy limits, missing cards, and fixture scaffold constraints.
- `M44-05`: Third-slice combo-to-recipe consistency - Check candidate combo cards are present and not blocked by manual-review dependency.
- `M44-06`: Third-slice blocker repair candidates - Generate source-backed repair candidates for blocked recipes.
- `M44-closeout`: Third-slice runtime readiness decision - Decide whether any recipe can later enter human acceptance and runtime fixture gates.

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

`M44-01`: Third-slice fixture scaffold.
