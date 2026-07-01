# Accepted Seed Human Review Packet Spec

Milestone: `M38-01`

## Purpose

`M38-01` exports a concise human review packet for accepted seed recipe
`recipe_003` and the recommended trigger repair.

This packet does not record acceptance. It prepares the information needed for
a later explicit human decision.

## Inputs

- `outputs/target_slice/m37_closeout_first_runtime_ready_recipe_decision.json`
- `outputs/target_slice/m37_05_revised_recipe_validation_rerun.json`
- `outputs/target_slice/m37_02_trigger_package_repair_proposal.json`

## Outputs

- `outputs/target_slice/m38_01_accepted_seed_human_review_packet.json`
- `outputs/target_slice/m38_01_accepted_seed_human_review_packet.md`
- `outputs/target_slice/m38_01_accepted_seed_human_review_packet.csv`

## Required Fields

- recipe id
- recommended trigger package id/profile
- quantity delta
- validation and consistency status after repair
- unresolved review codes
- decision blockers
- decision options
- recommended reviewer action

## Runtime Boundary

This milestone must not:

- record human acceptance
- mutate recipe draft files
- create runtime decks
- promote playbook hints
- enable bot integration
- parse live card text
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_accepted_seed_human_review_packet.py
python -m unittest tests.test_accepted_seed_human_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M38-01` is done when it emits JSON, Markdown, and CSV review packets, keeps
runtime promotion disabled, and points to `M38-02`.

