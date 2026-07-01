# First-Slice Review Packet Spec

Milestone: `M36-01`

## Purpose

`M36-01` creates a human-review packet for the first selected slice before
deck recipe drafts are created. It brings together the accepted advisory seed,
rejected combo lines, and manually reviewed cards so the team can decide what
is safe to carry into M36-02.

## Inputs

- `outputs/target_slice/m35_b4_first_slice_manual_review_queue.json`
- `outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.json`
- `outputs/target_slice/m35_closeout_hybrid_vertical_slice.json`

## Outputs

- `outputs/target_slice/m36_01_first_slice_review_packet.json`
- `outputs/target_slice/m36_01_first_slice_review_packet.md`
- `outputs/target_slice/m36_01_first_slice_review_packet.csv`

## Required Contents

- accepted seed review items
- rejected line review items
- manual-review card items
- blocked-until reasons for every item
- recommended next action for every item
- hard policy that this is offline review only

## Runtime Boundary

This milestone must not:

- create a deck recipe draft
- enable runtime bot integration
- publish a runtime playbook
- parse live card text
- mutate `GameState`
- auto-inject cards into player decks

## Verification

```powershell
python tools\deck\build_first_slice_review_packet.py
python -m unittest tests.test_first_slice_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M36-01` is done when the review packet reports:

- `1` accepted seed item
- `24` rejected line items
- `6` manual-review card items
- `31` total review items
- `ready_for_m36_02=true`

