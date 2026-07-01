# First Slice Manual Review Queue Spec

Milestone: `M35-B4`

## Purpose

Export selected-slice cards that need manual review before they are used as
high-confidence compatibility or playbook inputs.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_b2_first_slice_semantic_tags.json
outputs/target_slice/m35_b3_first_slice_requirement_provider_model.json
```

## Queue Contract

Each queue item includes:

- card id and display metadata
- manual review reasons
- unmapped feature tags
- current semantic tags
- current requirements/providers
- explicit policy blocking playbook promotion and high-confidence compatibility

## Hard Boundaries

- no automatic tag correction
- no compatibility graph yet
- no deck skeleton
- no bot/playbook promotion
- no runtime effect execution

## Outputs

```text
outputs/target_slice/m35_b4_first_slice_manual_review_queue.json
outputs/target_slice/m35_b4_first_slice_manual_review_queue.csv
outputs/target_slice/m35_b4_first_slice_manual_review_queue.md
```

## Verification

```powershell
python tools\deck\build_first_slice_manual_review_queue.py
python -m unittest tests.test_first_slice_manual_review_queue
python -m unittest discover -s tests -p "test_*.py"
```
