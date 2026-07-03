# Sixth-Slice Human Acceptance Preflight Spec

Milestone: `M57-03-preflight`

## Purpose

`M57-03-preflight` checks whether a proposed explicit `acceptance_text` can be
passed to the real `M57-03` human-accepted repair artifact generator.

This is a read-only guard between the `M57-03-prerequisite` acceptance request
packet and the real `M57-03` artifact. It exists so a human/team can dry-run an
acceptance decision without writing the accepted repair artifact.

## Inputs

- `outputs/target_slice/m57_03_sixth_slice_human_acceptance_request_packet.json`
- optional CLI/user input: `acceptance_text`

Default no-input mode must write a report with `preflight_passed=false` and
route to `M57-03-user-acceptance`.

## Outputs

- `outputs/target_slice/m57_03_sixth_slice_human_acceptance_preflight.json`
- `outputs/target_slice/m57_03_sixth_slice_human_acceptance_preflight.md`

## Preflight Rules

When `acceptance_text` is supplied, the preflight must:

- verify the M57-03 acceptance request packet is ready
- require non-empty `acceptance_text`
- call the real `M57-03` generator in memory only
- require the dry-run artifact to be ready for `M57-04`
- expose the exact real command that can create the accepted repair artifact

## Runtime Boundary

This milestone must not:

- create the real `M57-03` accepted repair artifact
- record human acceptance
- record a G Zone / Stride decision
- declare the repaired recipe valid
- modify M56/M57 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_slice_human_acceptance_preflight
python -m unittest discover -s tests -p "test_*.py"
```

Default report generation:

```powershell
python tools\deck\build_sixth_slice_human_acceptance_preflight.py
```

Dry-run a proposed acceptance without writing the real M57-03 artifact:

```powershell
python tools\deck\build_sixth_slice_human_acceptance_preflight.py `
  --acceptance-text "<explicit user/team acceptance text>"
```

## Done Rule

The preflight is complete when default no-input output routes to
`M57-03-user-acceptance`, valid proposed acceptance text dry-runs through the
real M57-03 generator contract in memory, pass/fail evidence is written to
JSON/MD, docs/status are updated, and all acceptance, G Zone, validation,
runtime, UI, bot, and `GameState` boundaries remain closed.
