# Third Slice Review Packet Spec

Milestone: `M44-02`

## Purpose

Export a reviewable packet for the third slice before advisory recipe draft
work begins.

The packet combines:

- the `M44-01` fixture scaffold note
- manual-review cards rebuilt from the selected-slice semantic pipeline
- candidate compatibility edges rebuilt from the compatibility pipeline

This packet is review-only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m43_01_third_target_slice_selection.json
outputs/target_slice/m43_02_third_slice_fixture_readiness.json
outputs/target_slice/m43_03_third_slice_semantic_compatibility_probe.json
outputs/target_slice/m43_04_third_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m44_01_third_slice_fixture_scaffold.json
```

## Expected Packet

Current source-backed packet:

```text
fixture scaffold items: 1
manual-review cards: 61
candidate edges: 109
total review items: 171
```

## Review Policy

- Human/team selection is required before recipe drafting.
- Manual-review cards are blocked until semantic review.
- Candidate edges are advisory inputs only.
- Fixture scaffold is policy evidence, not a saved deck.
- No live card text parsing.
- No direct `GameState` mutation.

## Outputs

```text
outputs/target_slice/m44_02_third_slice_review_packet.json
outputs/target_slice/m44_02_third_slice_review_packet.md
outputs/target_slice/m44_02_third_slice_review_packet.csv
```

## Verification

```powershell
python tools\deck\build_third_slice_review_packet.py
python -m unittest tests.test_third_slice_review_packet
python -m unittest discover -s tests -p "test_*.py"
```
