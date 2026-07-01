# Hybrid Vertical-Slice Closeout Spec

Milestone: `M35-closeout`

## Purpose

`M35-closeout` closes the M35 Hybrid Vertical-Slice Strategy and selects the
next implementation queue. It is an offline coordination artifact for AI
handoff, not a runtime gameplay change.

## Inputs

The closeout consumes all generated M35 target-slice reports from:

- `M35-A2` through `M35-A4`
- `M35-B1` through `M35-B4`
- `M35-C1` through `M35-C5`
- `M35-D1` through `M35-D4`
- `M35-E1` through `M35-E4`

## Output

- `outputs/target_slice/m35_closeout_hybrid_vertical_slice.json`
- `outputs/target_slice/m35_closeout_hybrid_vertical_slice.md`

## Required Findings

The closeout must report:

- all Phase A-E inputs exist
- all M35 phases are closed
- first-slice clean candidate edges, packages, skeletons, combo lines, reviewed
  seed entries, and rejected lines
- second-slice probe cards, edges, and candidate edges
- bot gate result, future hint candidates, blocked bot sources, and runtime bot
  status
- explicit next queue selection

## Next Queue

The selected next queue is `M36`: Human-review-assisted deck recipe validation.

`M36` stays offline and data/tooling focused. It must not wire runtime bot logic,
parse live card text, mutate `GameState`, or inject decks automatically.

## Verification

```powershell
python tools\deck\build_hybrid_vertical_slice_closeout.py
python -m unittest tests.test_hybrid_vertical_slice_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M35-closeout` is done when:

- the closeout report says `m35_hybrid_vertical_slice_complete=true`
- `runtime_bot_integration_enabled=false`
- the next queue is `M36`
- central docs point the active target to `M36-01`

