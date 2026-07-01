# First Slice Reviewed Playbook Seed Spec

Milestone: `M35-D4`

## Purpose

Export an advisory reviewed playbook seed from M35-D3 combo line explanations.

Selected target:

```text
Classic Core / โนว่า เกรปเปอร์
TD01-TD06 / BT01-BT09 / EB01-EB05
```

## Input

```text
outputs/target_slice/m35_d3_first_slice_combo_line_explainer.json
```

## Review Policy

M35-D4 uses static AI review only. A line can become a seed entry only when:

- it has at least one step
- line-level `needs_to_work` is exactly:
  `No additional support gap detected by C2-C4 advisory detectors.`
- no step is manual-review gated
- no step has missing resource, unsafe timing, missing zone, or Vanguard role
  conflict verdicts

Rejected lines must be preserved with review reasons.

## Runtime Boundary

Every seed entry must state:

- not a runtime playbook
- not published to bot
- not auto-injected into player decks
- human acceptance required before runtime use
- future bot use requires legal action mask and masked state

## Hard Boundaries

- no player-deck mutation
- no bot runtime export
- no final play sequence legality claim
- no runtime effect execution
- no mutation of source card data

## Outputs

```text
outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.json
outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.md
```

## Verification

```powershell
python tools\deck\build_first_slice_reviewed_playbook_seed.py
python -m unittest tests.test_first_slice_reviewed_playbook_seed
python -m unittest discover -s tests -p "test_*.py"
```
