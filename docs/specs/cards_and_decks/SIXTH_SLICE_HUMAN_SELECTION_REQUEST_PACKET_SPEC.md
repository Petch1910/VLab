# Sixth-Slice Human Selection Request Packet Spec

Milestone: `M57-02-prerequisite`

## Purpose

This packet prepares the handoff needed for `M57-02` without recording a
selection on behalf of the user or team.

It reads the `M57-01` human repair review packet, lists all review items that
are ready for explicit selection, and emits command templates for creating the
real `M57-02` selected recipe artifact after a human chooses exactly one
`review_item_id` and provides non-empty selection text.

## Inputs

- `outputs/target_slice/m57_01_sixth_slice_human_repair_review_packet.json`

## Outputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_request_packet.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_request_packet.md`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_request_packet.csv`

## Required User/Team Action

Choose exactly one ready `review_item_id` and provide non-empty selection text,
then run:

```powershell
python tools\deck\build_sixth_slice_human_selected_recipe_artifact.py `
  --review-item-id <review_item_id> `
  --selection-text "<explicit user/team selection reason>"
```

## Boundary

This packet must not:

- choose a review item automatically
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
python -m unittest tests.test_sixth_slice_human_selection_request_packet
python -m unittest discover -s tests -p "test_*.py"
```

Real packet generation:

```powershell
python tools\deck\build_sixth_slice_human_selection_request_packet.py
```

## Done Rule

The packet is complete when it lists all ready M57-01 review items, preserves
pair/manual/grade/G Zone context, provides exact command templates for the real
`M57-02` artifact, routes unready review packets to repair, updates docs/status,
and keeps all selection, acceptance, runtime, UI, bot, and `GameState`
boundaries closed.
