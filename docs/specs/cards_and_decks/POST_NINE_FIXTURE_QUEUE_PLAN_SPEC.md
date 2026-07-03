# Post-Nine Fixture Queue Plan Spec

Milestone: `M71-01`

## Purpose

`M71-01` decides the next bounded queue after the first nine offline fixture
scaffolds have passed their scale decision in `M70-04`.

The result must not select a tenth slice. The safest next queue is a gated
artifact materialization audit because several later fixture records are still
scaffold-safe or in-memory evidence until their real upstream CLI artifacts
exist.

## Inputs

- `outputs/target_slice/m70_04_nine_fixture_scale_decision.json`

Tests may pass an in-memory `M70-04` decision report. The CLI writes real
outputs only when the real `M70-04` JSON exists.

## Outputs

- `outputs/target_slice/m71_01_post_nine_fixture_queue_plan.json`
- `outputs/target_slice/m71_01_post_nine_fixture_queue_plan.md`

These outputs are planning artifacts only.

## Decision Rules

`M71-01` can open `M72-01` only when the `M70-04` report proves:

- version is `M70-04`
- fixture evidence count is `9`
- passed fixture count is `9`
- failed fixture count is `0`
- `post_m70_queue_review_ready` is true
- `ready_for_m71_planning` is true

If any condition fails, route back to `M70-repair`.

When ready, the recommended queue is:

- `M72-01`: Gated fixture artifact materialization audit

Deferred options:

- tenth-slice selection
- runtime fixture creation
- saved deck injection
- UI deck publication
- bot/playbook promotion
- G Zone runtime
- Stride runtime
- Aqua Force battle-order runtime
- Bloom/token runtime
- Lock/Unlock runtime
- Legion/Mate runtime
- live card text parsing

## Boundary

This milestone must not:

- select a tenth slice
- create a runtime fixture
- materialize real artifacts
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable Bloom/token runtime
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- parse live card text
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_post_nine_fixture_queue_plan
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact generation after the real `M70-04` file exists:

```powershell
python tools\deck\build_post_nine_fixture_queue_plan.py
```

## Done Rule

`M71-01` is ready when the plan recommends `M72-01` from passing `M70-04`
evidence, routes failed or malformed `M70-04` evidence to `M70-repair`, tests
cover boundary flags, docs are updated, and no runtime/UI/bot/GameState
mutation occurs.
