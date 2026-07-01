# First Slice Semantic Extractor Spec

Milestone: `M35-B2`

## Purpose

Extract advisory semantic tags from local runtime card text for the selected
first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

The extractor uses the bounded `M35-B1` vocabulary and produces offline data
for `M35-B3` requirement/provider modeling.

## Inputs

```text
outputs/target_slice/m35_a2_first_target_slice_report.json
outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.json
data/packs/vanguard_th/cards.sqlite
```

## Output Contract

Each selected-slice card receives:

- raw feature tags from the existing offline combo feature extractor
- mapped semantic tags grouped by vocabulary category
- unmapped feature tags
- manual review flag and reason list

## Hard Boundaries

- advisory offline extraction only
- no runtime effect execution
- no live match text parsing
- no structured card script generation
- no bot/playbook promotion

## Outputs

```text
outputs/target_slice/m35_b2_first_slice_semantic_tags.json
outputs/target_slice/m35_b2_first_slice_semantic_tags.md
```

## Verification

```powershell
python tools\deck\extract_first_slice_semantics.py
python -m unittest tests.test_first_slice_semantic_extractor
python -m unittest discover -s tests -p "test_*.py"
```
