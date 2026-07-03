# Sixth-Slice Human Acceptance Request Packet Spec

Milestone: `M57-03-prerequisite`

## Purpose

This packet prepares the handoff needed for `M57-03` without recording
acceptance on behalf of the user or team.

It reads the real `M57-02` selected recipe artifact, summarizes the selected
recipe, repair context, and decision options, then emits the exact command
template for generating the real `M57-03` human-accepted repair artifact after
a human provides explicit non-empty `acceptance_text`.

## Inputs

- `outputs/target_slice/m57_02_sixth_slice_human_selected_recipe_artifact.json`

## Outputs

- `outputs/target_slice/m57_03_sixth_slice_human_acceptance_request_packet.json`
- `outputs/target_slice/m57_03_sixth_slice_human_acceptance_request_packet.md`
- `outputs/target_slice/m57_03_sixth_slice_human_acceptance_request_packet.csv`

## Required User/Team Action

If the selected recipe repairs are accepted, provide explicit non-empty
acceptance text, then run:

```powershell
python tools\deck\build_sixth_slice_human_accepted_repair_artifact.py `
  --acceptance-text "<explicit user/team acceptance text>"
```

Selection text from `M57-02` is not acceptance text. The acceptance must be a
separate explicit decision.

## Boundary

This packet must not:

- record human acceptance
- record a G Zone / Stride decision
- create the real `M57-03` accepted repair artifact
- declare the repaired recipe valid
- modify M56/M57 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot playbooks
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_slice_human_acceptance_request_packet
python -m unittest discover -s tests -p "test_*.py"
```

Real packet generation:

```powershell
python tools\deck\build_sixth_slice_human_acceptance_request_packet.py
```

## Done Rule

The packet is complete when it preserves selected recipe context, marks exactly
which decision option can proceed to `M57-03`, provides the exact acceptance
command template, routes unready selected artifacts to repair, updates
docs/status, and keeps all acceptance, G Zone, validation, runtime, UI, bot, and
`GameState` boundaries closed.
