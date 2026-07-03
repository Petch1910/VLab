# Sixth-Slice Human Selection Candidate Digest Spec

Milestone: `M57-02-candidate-digest`

## Purpose

`M57-02-candidate-digest` is a read-only comparison matrix for the twelve ready
sixth-slice review candidates.

It exists to make the explicit user/team selection easier by grouping the
candidates by source card and target card, showing the repair/grade/G Zone
signals, and preserving the real `M57-02` command template for each candidate.

It must not recommend, rank, or choose a candidate.

## Inputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_request_packet.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_preflight.json`

## Outputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_candidate_digest.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_candidate_digest.md`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_candidate_digest.csv`

## Rules

- Include every candidate from the request packet in original order.
- Preserve `review_item_id`, `recipe_id`, `source_candidate_edge`, source card,
  target card, manual-repair counts, grade counts, G Zone deferred flag, and
  selection command template.
- Group ready candidates by source card and target card.
- Detect whether ready candidates share the same structural readiness profile.
- If all ready candidates share the same structural profile, the digest may say
  the tie breaker is human/team source-target preference.
- The digest must not set any selected id or recommended id.
- The digest must route to `M57-02-user-selection` when the request packet is
  ready and at least one candidate is ready.

## Boundary

This digest must not:

- choose or recommend a review item
- create the real `M57-02` selected artifact
- record human selection
- record human acceptance
- record a G Zone / Stride decision
- modify recipe drafts or review packets
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot playbooks
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_slice_human_selection_candidate_digest
python -m unittest discover -s tests -p "test_*.py"
```

Generate the digest:

```powershell
python tools\deck\build_sixth_slice_human_selection_candidate_digest.py
```

## Done Rule

The digest is complete when JSON/MD/CSV outputs list all ready candidates,
preserve selection command templates, provide source/target grouping and
structural-readiness summary, route the next action to human/team selection, and
keep selection, acceptance, runtime, UI, bot, and `GameState` boundaries closed.
