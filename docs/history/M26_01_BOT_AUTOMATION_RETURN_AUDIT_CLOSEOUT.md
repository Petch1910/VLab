# M26-01 Bot / Automation Return Audit Closeout

## Status

Done.

## Scope

Added an audit gate before returning to bot and automation work after the
Windows-first UX and online-room passes. This milestone does not add smarter
bot behavior, new search algorithms, RL/self-play, or live card-effect
resolution.

## Implemented

- Added `docs/specs/bot_and_headless/BOT_AUTOMATION_RETURN_AUDIT_SPEC.md`.
- Added `BotAutomationReturnAuditGate`.
- Added an audit report proving:
  - `M21-M25` closeouts are present as prerequisites.
  - M26 may continue to `M26-02`.
  - Advanced search/RL expansion remains blocked.
  - Bot work must use legal actions, masked state, no live-state mutation,
    planning-only probability, and no live text/LLM effect resolution.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m26_01_bot_return_audit.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m26_01_bot_return_audit.xml`
  passed `1082/1082`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m26_01_bot_return_audit.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m26_01_bot_return_audit.json`
  passed with `blockers=[]`.

## Next Target

Proceed to `M26-02`: enforce that bot work uses legal action masks and masked
state only.
