# Bot Integration Gate Spec

Milestone: `M35-E4`

## Purpose

`M35-E4` is an offline safety gate for future bot use of deck/combo playbook
hints. It confirms which reviewed advisory seed entries may be treated as
future bot hint candidates, and which generalized compatibility outputs must
stay blocked from runtime bot use.

This spec does not enable runtime bot wiring, mutate Unity state, publish a
bot playbook, or promote heuristic card-pair edges into executable decisions.

## Inputs

- `outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.json`
- `outputs/target_slice/m35_e3_generalized_semantic_compatibility_probe.json`
- `docs/CORE_DEVELOPMENT_GUARDRAILS.md`
- `docs/specs/bot_and_headless/BOT_LEGAL_ACTION_MASKED_STATE_GATE_SPEC.md`
- `docs/specs/bot_and_headless/PLAYBOOK_INTEGRATION_SPEC.md`

## Gate Policy

- The bot may consume only reviewed hint candidates.
- Legal action mask is required before any future runtime use.
- Masked state view is required before any future runtime use.
- RulesCore command validation is required before any future runtime use.
- Direct `GameState` mutation is forbidden.
- True hidden state access is forbidden.
- Live card text parsing is forbidden.
- Unreviewed `M35-E3` semantic/compatibility probe edges are forbidden from
  runtime bot use.
- Automatic publication to runtime or bot playbooks is forbidden.

## Allowed Evidence

Only `M35-D4` reviewed seed entries can become future bot hint candidates.
Every candidate must retain the following required-before-runtime-use gates:

- human acceptance
- legal action mask
- masked state view
- RulesCore command validation
- no live card text parser
- player-readable bot trace

## Blocked Evidence

`M35-E3` semantic/compatibility probe edges remain advisory scale-out evidence.
They are blocked from runtime bot use because they are generalized compatibility
edges, not human-reviewed playbook hints.

## Outputs

- `outputs/target_slice/m35_e4_bot_integration_gate.json`
- `outputs/target_slice/m35_e4_bot_integration_gate.md`

## Verification

```powershell
python tools\deck\build_bot_integration_gate.py
python -m unittest tests.test_bot_integration_gate
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M35-E4` is done when the gate report:

- passes all evidence-file checks
- lists reviewed future hint candidates
- blocks unreviewed generalized probe sources
- keeps `runtime_bot_integration_enabled` as `false`
- points the next target to `M35-closeout`

