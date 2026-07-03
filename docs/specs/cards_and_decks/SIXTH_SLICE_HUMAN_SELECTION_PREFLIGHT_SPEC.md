# Sixth-Slice Human Selection Preflight Spec

Milestone: `M57-02-preflight`

## Purpose

`M57-02-preflight` checks whether a proposed sixth-slice human selection is
safe to pass to the real `M57-02` selected recipe artifact generator.

This is a read-only guard between the `M57-02-prerequisite` request packet and
the real `M57-02` artifact. It exists so a human/team can dry-run one
`review_item_id` plus non-empty `selection_text` before any selected artifact is
written.

## Inputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_request_packet.json`
- `outputs/target_slice/m57_01_sixth_slice_human_repair_review_packet.json`
- Optional CLI input: `review_item_id`
- Optional CLI input: `selection_text`

When no CLI selection input is provided, the tool still writes a report, but it
must show `preflight_passed=false` and route to `M57-02-user-selection`.

## Outputs

- `outputs/target_slice/m57_02_sixth_slice_human_selection_preflight.json`
- `outputs/target_slice/m57_02_sixth_slice_human_selection_preflight.md`

## Rules

- The preflight must not choose a review item automatically.
- The preflight must not create
  `outputs/target_slice/m57_02_sixth_slice_human_selected_recipe_artifact.*`.
- `preflight_passed=true` only when:
  - the request packet is ready
  - `review_item_id` is non-empty
  - `review_item_id` exists in the request packet
  - the candidate is ready for user selection
  - `selection_text` is non-empty
  - the real M57-02 generator accepts the proposed selection in memory
  - the dry-run selected artifact would be `ready_for_m57_03=true`
- Missing CLI input is an input issue, not a structural blocker.
- Unknown or unready review ids are blockers.

## Boundary

This preflight must not:

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
python -m unittest tests.test_sixth_slice_human_selection_preflight
python -m unittest discover -s tests -p "test_*.py"
```

Generate the default no-selection handoff report:

```powershell
python tools\deck\build_sixth_slice_human_selection_preflight.py
```

Dry-run a proposed human/team selection without writing the real M57-02
artifact:

```powershell
python tools\deck\build_sixth_slice_human_selection_preflight.py `
  --review-item-id <M57-01 review_item_id> `
  --selection-text "<explicit user/team selection reason>"
```

## Done Rule

The preflight is complete when it writes a no-selection report for handoff,
dry-runs valid proposed selections through the real M57-02 generator contract in
memory, rejects bad ids and blank selection text, updates docs/status, and keeps
all selection, acceptance, runtime, UI, bot, and `GameState` boundaries closed.
