# Ninth-Slice Review Packet Spec

Milestone: `M68-02`

## Purpose

Export a reviewable packet for the ninth slice before advisory recipe draft
work begins.

The packet combines:

- the `M68-01` fixture scaffold note
- manual-review cards rebuilt from the selected-slice semantic pipeline
- candidate compatibility edges rebuilt from the compatibility pipeline
- G Zone / Stride / Aqua Force battle-order review notes

This packet is review-only. It does not create deck recipes, runtime fixtures,
saved decks, UI deck entries, bot playbooks, or mutate `GameState`.

## Inputs

```text
outputs/target_slice/m67_01_ninth_target_slice_selection.json
outputs/target_slice/m67_02_ninth_slice_fixture_readiness.json
outputs/target_slice/m67_03_ninth_slice_semantic_compatibility_probe.json
outputs/target_slice/m67_04_ninth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m68_01_ninth_slice_fixture_scaffold.json
```

Tests may pass in-memory reports until the real upstream files exist.

## Expected Packet

Current source-backed packet from the in-memory ninth-slice evidence:

```text
fixture scaffold items: 1
manual-review cards: 10
candidate edges: 95
total review items: 106
```

## Review Policy

- Human/team selection is required before recipe drafting.
- Manual-review cards are blocked until semantic review.
- Candidate edges are advisory inputs only.
- Fixture scaffold is policy evidence, not a saved deck.
- Runtime G Zone and Stride support remains deferred.
- Stride / G-unit text requires manual review.
- Generation Break text requires manual review.
- Aqua Force attack-count / battle-order text requires manual review.
- Grade 4/G-zone cards remain review-only until format support exists.
- No live card text parsing.
- No direct `GameState` mutation.

## Outputs

```text
outputs/target_slice/m68_02_ninth_slice_review_packet.json
outputs/target_slice/m68_02_ninth_slice_review_packet.md
outputs/target_slice/m68_02_ninth_slice_review_packet.csv
```

## Verification

```powershell
python -m unittest tests.test_ninth_slice_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M67-01, M67-02, M67-03, M67-04, and M68-01
outputs exist:

```powershell
python tools\deck\build_ninth_slice_review_packet.py
```
