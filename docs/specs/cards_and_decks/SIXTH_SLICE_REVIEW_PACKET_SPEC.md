# Sixth-Slice Review Packet Spec

Milestone: `M56-02`

## Purpose

Export a reviewable packet for the sixth slice before advisory recipe draft
work begins.

The packet combines:

- the `M56-01` fixture scaffold note
- manual-review cards rebuilt from the selected-slice semantic pipeline
- candidate compatibility edges rebuilt from the compatibility pipeline

This packet is review-only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m55_01_sixth_target_slice_selection.json
outputs/target_slice/m55_02_sixth_slice_fixture_readiness.json
outputs/target_slice/m55_03_sixth_slice_semantic_compatibility_probe.json
outputs/target_slice/m55_04_sixth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m56_01_sixth_slice_fixture_scaffold.json
```

## Expected Packet

Current source-backed packet:

```text
fixture scaffold items: 1
manual-review cards: 11
candidate edges: 70
total review items: 82
```

## Review Policy

- Human/team selection is required before recipe drafting.
- Manual-review cards are blocked until semantic review.
- Candidate edges are advisory inputs only.
- Fixture scaffold is policy evidence, not a saved deck.
- G Zone and Grade 4 recipe support remains deferred.
- Stride / Generation Break text requires manual review.
- Ritual / retire-heavy text requires manual review.
- No live card text parsing.
- No direct `GameState` mutation.

## Outputs

```text
outputs/target_slice/m56_02_sixth_slice_review_packet.json
outputs/target_slice/m56_02_sixth_slice_review_packet.md
outputs/target_slice/m56_02_sixth_slice_review_packet.csv
```

## Verification

```powershell
python tools\deck\build_sixth_slice_review_packet.py
python -m unittest tests.test_sixth_slice_review_packet
python -m unittest discover -s tests -p "test_*.py"
```
