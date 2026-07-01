# M26-08 Bot Automation Return Gate Closeout Spec

## Goal

Close M26 after confirming bot/automation work can continue without expanding
the core in unsafe ways.

## Completed Gates

- `M26-01`: M21-M25 audit before returning to bot work.
- `M26-02`: bot decisions use legal action masks and masked state.
- `M26-03`: player-readable bot explanation panel.
- `M26-04`: structured ability automation is allowed only for tested templates.
- `M26-05`: live effect resolution rejects runtime text parsing and LLM
  resolution.
- `M26-06`: Home Solo Practice setup flow.
- `M26-07`: hidden-state, snapshot-simulation, and replay-determinism safety
  regression gate.

## Next Safe Bot Direction

Future CPU/bot work should build on:

- legal action masks
- masked state views
- structured ability templates
- snapshot simulation paths
- replay/event logs
- player-readable bot traces

Do not add advanced search, self-play, RL, live card-text parsing, or hidden
state access without a new explicit milestone and tests.

## Verification

M26-08 is docs-only. It relies on the latest M26 verification:

- M26-07 Unity compile and EditMode `1119/1119`
- M26-06 Windows build and player smoke `blockers=[]`
