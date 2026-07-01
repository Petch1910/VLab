# First Slice Semantic Vocabulary Spec

Milestone: `M35-B1`

## Purpose

Create a bounded advisory semantic vocabulary for the selected first slice:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

This vocabulary becomes the allowed tag set for `M35-B2` offline semantic
extraction.

## Scope

The vocabulary covers:

- ability types
- zones
- timing tags
- condition tags
- cost tags
- effect tags
- duration tags
- mechanic groups
- trigger icons

## Hard Boundaries

- advisory offline vocabulary only
- no runtime effect execution
- no live card text parser
- no broad all-era vocabulary
- no bot/playbook promotion

Later mechanics stay excluded from this first slice, including Stride,
Imaginary Gift, Ride Deck, orders, Energy, overDress/XoverDress, and Divine
Skill.

## Outputs

```text
outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.json
outputs/target_slice/m35_b1_first_slice_semantic_vocabulary.md
```

## Verification

```powershell
python tools\deck\build_first_slice_semantic_vocabulary.py
python -m unittest tests.test_first_slice_semantic_vocabulary
python -m unittest discover -s tests -p "test_*.py"
```
