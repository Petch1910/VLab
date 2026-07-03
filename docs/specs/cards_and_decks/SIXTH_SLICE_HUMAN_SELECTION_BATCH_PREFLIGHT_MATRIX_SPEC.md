# Sixth-Slice Human Selection Batch Preflight Matrix Spec

Milestone: `M57-02-batch-preflight-matrix`

## Purpose

`M57-02-batch-preflight-matrix` checks every ready sixth-slice selection
candidate against the real `M57-02` selected recipe artifact generator contract
in memory.

It gives the team evidence that any listed candidate can become a valid
`M57-02` selected artifact after a real human/team selection is supplied.

It must not write the real selected artifact and must not record human
selection.

## Inputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_request_packet.json`
- `outputs/target_slice/m57_01_sixth_slice_human_repair_review_packet.json`

## Outputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_batch_preflight_matrix.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_batch_preflight_matrix.md`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_batch_preflight_matrix.csv`

## Rules

- Run an in-memory dry-run for every ready candidate from the request packet.
- Use synthetic dry-run text only; it is not real user/team selection text.
- A candidate passes only when the real `M57-02` generator:
  - accepts the `review_item_id`
  - would record selection in the hypothetical artifact
  - would not record acceptance
  - would not record a G Zone / Stride decision
  - would not allow runtime promotion
  - would be ready for `M57-03`
  - returns the same selected recipe id as the candidate row
- If any candidate fails, route back to prerequisite repair rather than user
  selection.
- If all candidates pass, route to `M57-02-user-selection`.

## Boundary

This matrix must not:

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
python -m unittest tests.test_sixth_slice_human_selection_batch_preflight_matrix
python -m unittest discover -s tests -p "test_*.py"
```

Generate the matrix:

```powershell
python tools\deck\build_sixth_slice_human_selection_batch_preflight_matrix.py
```

## Done Rule

The matrix is complete when all ready candidates are dry-run through the real
M57-02 generator in memory, pass/fail evidence is written to JSON/MD/CSV,
failed candidate paths are tested, docs/status are updated, and all selection,
acceptance, runtime, UI, bot, and `GameState` boundaries remain closed.
