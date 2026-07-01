# M26-06 Solo Play Home Entry Flow Closeout

## Status

Complete.

## What Changed

- Added `SoloPlayEntryFlow` as a pure setup model for Home -> Solo Practice.
- Home `Solo Play` now opens a setup panel before starting PlayTable.
- The setup panel supports:
  - difficulty cycling: Easy, Normal, Hard
  - bot deck choice: mirror player deck, random saved deck, or a specific saved
    deck when saved decks exist
- The setup validates player and opponent decks before start.
- PlayTable local mode can display a Solo Practice summary without mutating
  `GameState`.

## Guardrails

- No bot turn automation was enabled.
- Difficulty is metadata for the practice profile until later bot gates connect
  it to CPU execution.
- Decks passed to PlayTable are cloned.
- Home UI does not mutate `GameState`.
- No Android, APK, LDPlayer, mobile QA, app packaging, or release work was run.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m26_06_solo_play_entry_flow.log`
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m26_06_solo_play_entry_flow.xml`
  passed `1115/1115`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m26_06_solo_play_entry_flow.log`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m26_06_solo_play_entry_flow.json`
  passed with `blockers=[]`

## Next Target

`M26-07`: no-hidden-leak / simulation no-live-mutation / replay determinism
regression gate.
