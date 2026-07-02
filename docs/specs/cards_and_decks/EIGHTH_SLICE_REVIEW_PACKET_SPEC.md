# Eighth-Slice Review Packet Spec

Milestone: `M64-02`

## Purpose

Export a reviewable packet for the eighth slice before advisory recipe draft
work begins.

The packet combines:

- the `M64-01` fixture scaffold note
- manual-review cards rebuilt from the selected-slice semantic pipeline
- candidate compatibility edges rebuilt from the compatibility pipeline

This packet is review-only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m63_01_eighth_target_slice_selection.json
outputs/target_slice/m63_02_eighth_slice_fixture_readiness.json
outputs/target_slice/m63_03_eighth_slice_semantic_compatibility_probe.json
outputs/target_slice/m63_04_eighth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m64_01_eighth_slice_fixture_scaffold.json
```

Tests may pass in-memory reports until the real upstream files exist.

## Expected Packet

Current source-backed packet from the in-memory eighth-slice evidence:

```text
fixture scaffold items: 1
manual-review cards: 6
candidate edges: 355
total review items: 362
```

## Review Policy

- Human/team selection is required before recipe drafting.
- Manual-review cards are blocked until semantic review.
- Candidate edges are advisory inputs only.
- Fixture scaffold is policy evidence, not a saved deck.
- Runtime Lock and Legion support remains deferred.
- Lock/Unlock text requires manual review.
- Legion/Mate text requires manual review.
- Grade 4 cards remain advisory only until format support exists.
- No live card text parsing.
- No direct `GameState` mutation.

## Outputs

```text
outputs/target_slice/m64_02_eighth_slice_review_packet.json
outputs/target_slice/m64_02_eighth_slice_review_packet.md
outputs/target_slice/m64_02_eighth_slice_review_packet.csv
```

## Verification

```powershell
python -m unittest tests.test_eighth_slice_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M63-01, M63-02, M63-03, M63-04, and M64-01
outputs exist:

```powershell
python tools\deck\build_eighth_slice_review_packet.py
```
