# M21-07 Player-Readable Event Log Closeout

Status: Done

## Scope

- Converted default PlayTable event/replay output from reducer-style text to a
  player-facing match log.
- `PlayTableEventLogFormatter` now produces lines such as:
  - `P1 drew 1 card.`
  - `P1 moved a card from hand to rear-guard.`
  - `P1 changed phase from main to battle.`
  - `P1 performed a drive check.`
- `PlayTableEventReplayPanelFormatter` now reuses the same event-line
  formatter so the match log and replay panel do not drift.
- Updated formatter tests to assert that raw action names, private card
  instance ids, target ids, and reducer-style zone traces do not appear in the
  default player-facing log.

## Safety

- Formatting is read-only and does not mutate `GameState` or event records.
- Private card instance ids remain omitted from trigger checks, attack logs,
  resource flips, and mulligan logs.
- No online protocol, bot logic, Android flow, app packaging, or release work
  was changed.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_07_player_event_log.log`
  has no compiler-error markers and exits batchmode successfully.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_07_player_event_log.xml`
  passed `982/982`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_07_player_event_log.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_07_player_event_log.json`
  passed with `blockers=[]`.

## Next

Move to `M21-08`: roll up tests and Windows player smoke evidence for the
PlayTable Windows UX pass before closing M21.
