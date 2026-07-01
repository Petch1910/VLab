# Headless CLI Runner Spec

## Milestone

`M17-01`

## Goal

Add a deterministic headless simulation entry point that can run the C# core
without opening Card Browser, Play Table UI, Photon, or any player-facing
screen. This is the first step toward batch simulation and research tooling.

## Scope

- Create a default playable deck from the current Vanguard TH pack.
- Validate the deck with the normal deck validator.
- Create a two-player `GameState` with a fixed seed.
- Execute a small deterministic RulesCore script:
  - draw
  - move one card from hand to rear-guard
  - set phase to Main
  - add one Protect gift marker
- Return a JSON result with seed, action count, event count, final phase, and
  basic player zone counters.
- Add a Unity editor command-line entry point:
  `VanguardThaiSim.EditorTools.HeadlessSimulationCliRunner.RunFromCommandLine`.

## Non-Goals

- No full deck/ruleset/seed CLI argument model. That is `M17-02`.
- No replay/result artifact schema beyond the minimal result JSON. That is
  `M17-03`.
- No batch self-play. That starts at `M17-04`.
- No UI, Photon, bot search, ISMCTS, RL, or packed state changes.

## Verification Command

```powershell
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
$resultPath = Join-Path $projectPath "work\headless_m17_01_result.json"
$logPath = Join-Path $projectPath "work\headless_m17_01.log"
& $unityExe -batchmode -nographics -quit -projectPath $projectPath `
  -executeMethod VanguardThaiSim.EditorTools.HeadlessSimulationCliRunner.RunFromCommandLine `
  -headlessResultPath $resultPath `
  -logFile $logPath
```

The command exits `0` only when the deterministic headless core script accepts
all actions and writes the result JSON.
