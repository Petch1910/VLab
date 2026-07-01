# First Slice Pair Compatibility Graph Spec

Milestone: `M35-C1`

## Purpose

Build an advisory pair graph for the selected first slice by connecting cards
that provide a capability to cards that require that capability.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Inputs

```text
outputs/target_slice/m35_b3_first_slice_requirement_provider_model.json
outputs/target_slice/m35_b4_first_slice_manual_review_queue.json
```

## Graph Contract

Each node is one selected-slice card from the requirement/provider model.

Each edge is directed:

```text
provider card -> consumer card
```

An edge records:

- source providers
- target requirements
- matched compatibility rule ids
- advisory score
- compatibility categories
- manual-review gating status

## Compatibility Categories

M35-C1 may emit only coarse advisory categories:

- `resource_support`
- `board_support`
- `consistency_support`
- `battle_pressure_support`
- `trigger_support`

Detailed verdicts are intentionally deferred:

- resource conflict verdict: `M35-C2`
- timing compatibility verdict: `M35-C3`
- zone/target compatibility verdict: `M35-C4`
- final selected-slice compatibility output: `M35-C5`

## Manual Review Policy

If either side of an edge is in the M35-B4 manual review queue, the edge must be
marked:

```text
confidence = review_required
allowed_next_use = manual_review_required_before_high_confidence_compatibility
```

Those edges remain visible for analysis, but they cannot become high-confidence
compatibility or playbook input until reviewed.

## Hard Boundaries

- no resource conflict verdict yet
- no timing compatibility verdict yet
- no zone/target compatibility verdict yet
- no deck skeleton
- no bot playbook
- no runtime effect execution
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.json
outputs/target_slice/m35_c1_first_slice_pair_compatibility_graph.md
```

## Verification

```powershell
python tools\deck\build_first_slice_pair_compatibility_graph.py
python -m unittest tests.test_first_slice_pair_compatibility_graph
python -m unittest discover -s tests -p "test_*.py"
```
