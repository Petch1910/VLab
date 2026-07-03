# Sixth-Slice Human Selection Support Closeout Spec

Milestone: `M57-02-selection-support-closeout`

## Purpose

`M57-02-selection-support-closeout` closes the AI-generated support work around
sixth-slice human selection.

It combines the M57-02 request packet, default preflight report, candidate
digest, and batch preflight matrix into one handoff artifact. The closeout must
prove that support evidence is ready while keeping the real M57-02 selected
artifact blocked until a user/team explicitly chooses one `review_item_id` and
provides non-empty `selection_text`.

## Inputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_request_packet.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_preflight.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_candidate_digest.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_batch_preflight_matrix.json`

## Outputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_support_closeout.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_support_closeout.md`

## Rules

- Confirm the request packet is ready and has `12` ready candidates.
- Confirm the default preflight report is ready and still requires input.
- Confirm the candidate digest is ready for user/team selection.
- Confirm the batch preflight matrix reports all `12` candidates pass.
- Confirm no support artifact records human selection.
- Provide the real M57-02 command template, but do not fill in a selected id or
  selection text.
- Route to `M57-02-user-selection` only when all support evidence passes.

## Boundary

This closeout must not:

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
python -m unittest tests.test_sixth_slice_human_selection_support_closeout
python -m unittest discover -s tests -p "test_*.py"
```

Generate the closeout:

```powershell
python tools\deck\build_sixth_slice_human_selection_support_closeout.py
```

## Done Rule

The closeout is complete when it summarizes all M57-02 support artifacts,
proves the candidate set is technically ready for explicit user/team selection,
routes the next action to `M57-02-user-selection`, updates docs/status, and
keeps all selection, acceptance, runtime, UI, bot, and `GameState` boundaries
closed.
