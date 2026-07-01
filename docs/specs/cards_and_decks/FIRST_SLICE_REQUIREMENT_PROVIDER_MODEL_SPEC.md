# First Slice Requirement / Provider Model Spec

Milestone: `M35-B3`

## Purpose

Transform selected-slice advisory semantic tags into card-level requirements
and providers for later compatibility graph work.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_b2_first_slice_semantic_tags.json
```

## Model Contract

Each card receives:

- requirements: costs, timing needs, zone needs, and condition needs
- providers: resource recovery, pressure, card advantage, restand/multi-attack,
  trigger value, and related advisory outputs
- manual review flag carried forward from extraction

## Hard Boundaries

- no compatibility graph yet
- no deck skeleton
- no bot playbook
- no runtime effect execution
- no live card text parser

## Outputs

```text
outputs/target_slice/m35_b3_first_slice_requirement_provider_model.json
outputs/target_slice/m35_b3_first_slice_requirement_provider_model.md
```

## Verification

```powershell
python tools\deck\build_first_slice_requirement_provider_model.py
python -m unittest tests.test_first_slice_requirement_provider_model
python -m unittest discover -s tests -p "test_*.py"
```
