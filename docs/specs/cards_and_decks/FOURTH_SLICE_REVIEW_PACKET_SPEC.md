# Fourth-Slice Review Packet Spec

Milestone: `M48-02`

## Purpose

Export a reviewable packet for the fourth slice before advisory recipe draft
work begins.

The packet combines:

- the `M48-01` fixture scaffold note
- manual-review cards rebuilt from the selected-slice semantic pipeline
- candidate compatibility edges rebuilt from the compatibility pipeline

This packet is review-only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m47_01_fourth_target_slice_selection.json
outputs/target_slice/m47_repair_apply_scope.json
outputs/target_slice/m47_03_fourth_slice_semantic_compatibility_probe.json
outputs/target_slice/m47_04_fourth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m48_01_fourth_slice_fixture_scaffold.json
```

## Expected Packet

Current source-backed packet:

```text
fixture scaffold items: 1
manual-review cards: 15
candidate edges: 785
total review items: 801
```

## Review Policy

- Human/team selection is required before recipe drafting.
- Manual-review cards are blocked until semantic review.
- Candidate edges are advisory inputs only.
- Fixture scaffold is policy evidence, not a saved deck.
- Grade 4/G Zone boundaries require manual review.
- No live card text parsing.
- No direct `GameState` mutation.

## Outputs

```text
outputs/target_slice/m48_02_fourth_slice_review_packet.json
outputs/target_slice/m48_02_fourth_slice_review_packet.md
outputs/target_slice/m48_02_fourth_slice_review_packet.csv
```

## Verification

```powershell
python tools\deck\build_fourth_slice_review_packet.py
python -m unittest tests.test_fourth_slice_review_packet
python -m unittest discover -s tests -p "test_*.py"
```
