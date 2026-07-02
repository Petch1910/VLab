# Seventh-Slice Review Packet Spec

Milestone: `M60-02`

## Purpose

Export a reviewable packet for the seventh slice before advisory recipe draft
work begins.

The packet combines:

- the `M60-01` fixture scaffold note
- manual-review cards rebuilt from the selected-slice semantic pipeline
- candidate compatibility edges rebuilt from the compatibility pipeline

This packet is review-only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m59_01_seventh_target_slice_selection.json
outputs/target_slice/m59_02_seventh_slice_fixture_readiness.json
outputs/target_slice/m59_03_seventh_slice_semantic_compatibility_probe.json
outputs/target_slice/m59_04_seventh_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m60_01_seventh_slice_fixture_scaffold.json
```

Tests may pass in-memory reports until the real upstream files exist.

## Expected Packet

Current source-backed packet from the in-memory seventh-slice evidence:

```text
fixture scaffold items: 1
manual-review cards: 10
candidate edges: 107
total review items: 118
```

## Review Policy

- Human/team selection is required before recipe drafting.
- Manual-review cards are blocked until semantic review.
- Candidate edges are advisory inputs only.
- Fixture scaffold is policy evidence, not a saved deck.
- G Zone and Grade 4 recipe support remains deferred.
- Stride / Generation Break text requires manual review.
- Bloom/token-like text requires manual review.
- No live card text parsing.
- No direct `GameState` mutation.

## Outputs

```text
outputs/target_slice/m60_02_seventh_slice_review_packet.json
outputs/target_slice/m60_02_seventh_slice_review_packet.md
outputs/target_slice/m60_02_seventh_slice_review_packet.csv
```

## Verification

```powershell
python -m unittest tests.test_seventh_slice_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M59-01, M59-02, M59-03, M59-04, and M60-01
outputs exist:

```powershell
python tools\deck\build_seventh_slice_review_packet.py
```
