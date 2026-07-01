# M26-02 Bot Legal Action / Masked State Gate Closeout

## Status

Done.

## Scope

Hardened the canonical bot decision boundary so bot decisions are made from a
masked player view and a legal action list, rather than from raw true state
inspection. This milestone does not add smarter bot behavior, ISMCTS expansion,
RL/self-play, or live text/LLM card-effect resolution.

## Implemented

- Added `docs/specs/bot_and_headless/BOT_LEGAL_ACTION_MASKED_STATE_GATE_SPEC.md`.
- Added `BotDecisionContext` and `BotDecisionContextFactory`.
- `BotDecisionContextFactory.Create(...)` now:
  - creates a masked player view through `GameStateViewFactory.CreatePlayerView`
  - generates legal actions from the masked view
  - does not mutate the source `GameState`
- Updated current simple/profile/heuristic/playbook bot decision paths to enter
  the context boundary before selecting actions.
- Expanded action cloning in heuristic/playbook bots to preserve target,
  trigger source, and multi-card action fields.
- Updated legal action usage inventory with bot context, heuristic bot, and
  playbook bot entries.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m26_02_bot_masked_legal_gate.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m26_02_bot_masked_legal_gate.xml`
  passed `1086/1086`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m26_02_bot_masked_legal_gate.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m26_02_bot_masked_legal_gate.json`
  passed with `blockers=[]`.

## Next Target

Proceed to `M26-03`: player-readable bot explanation panel.
