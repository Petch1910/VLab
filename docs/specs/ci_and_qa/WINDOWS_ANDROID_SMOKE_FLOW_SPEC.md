# Windows + Android Smoke Flow Spec

## Milestone

`M16-10`

## Goal

Provide a repeatable smoke flow that proves the current Unity client can load
the card browser data path, validate a deck, execute basic Play Table
commands, pass Android responsive layout QA, and confirm Windows/Android build
settings are present before full M18 build artifacts.

## Scope

- Runtime smoke verifier:
  - load default Vanguard TH pack manifest and SQLite card database
  - query card summaries and load a card detail
  - create a playable 50 main + 4 ride deck from unique card ids
  - validate the deck and deck-code round-trip
  - create a two-player `GameState`
  - execute RulesCore draw, move, and phase commands
  - run Android responsive layout QA
- Editor command runner:
  - verify at least one enabled build scene exists
  - verify Windows Standalone and Android build target support are installed
  - verify `VANGUARD_PHOTON_REALTIME` scripting define exists for Standalone
    and Android
- Player smoke bootstrap:
  - accepts `-vanguardPlayerSmoke`
  - writes the same runtime smoke report to `-vanguardPlayerSmokeOutput`
  - exits `0` when card browser/deck/play table/layout smoke passes

## Non-Goals

- No APK/AAB or Windows executable artifact is produced in this milestone.
  Build artifacts remain M18 work.
- No Photon live room smoke is rerun here; M13-11 already owns that path.
- No gameplay rules, card data, deck data, image files, or network payloads are
  changed.

## Verification Commands

Unity compile and EditMode tests must pass after any code changes.

Editor smoke command:

```powershell
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
$logPath = Join-Path $projectPath "work\client_smoke_m16_10.log"
& $unityExe -batchmode -nographics -quit -projectPath $projectPath `
  -executeMethod VanguardThaiSim.EditorTools.ClientSmokeFlowRunner.RunFromCommandLine `
  -logFile $logPath
```

The command exits `0` only when runtime smoke and build-settings smoke pass.

Player build smoke command after a Windows artifact exists:

```powershell
$exe = (Resolve-Path "client\unity\VanguardThaiSim\build\windows\latest\VanguardThaiSim.exe").Path
$output = (Resolve-Path "client\unity\VanguardThaiSim\work").Path + "\player_smoke_manual_test.json"
& $exe -batchmode -nographics -vanguardPlayerSmoke -vanguardPlayerSmokeOutput $output
```

The command exits `0` only when the built player loads the copied runtime pack,
queries cards, validates a playable smoke deck, commits basic Play Table
commands, and writes a report with no blockers.

Android install-smoke command after an APK exists:

```powershell
python tools\smoke\android_install_smoke.py `
  --adb auto `
  --output client\unity\VanguardThaiSim\work\android_install_smoke_auto_adb.json
```

This command writes a JSON report. If no emulator/device is listed by
`adb devices`, the report status is `waiting` with blocker `NO_ADB_DEVICE`.
When a device is attached, the same command installs the APK with
`adb install -r`. `--adb auto` probes common ADB paths, including LDPlayer's
bundled ADB when installed. The helper auto-detects the package id with `aapt
dump badging` when available, so optional launch smoke is available with
`--launch`; `--package <id>` remains available as an explicit override.

Android runtime-pack smoke command for current local LDPlayer/mobile testing:

```powershell
python tools\smoke\android_install_smoke.py `
  --adb auto `
  --push-pack `
  --force-stop-before-launch `
  --launch `
  --timeout 600 `
  --output client\unity\VanguardThaiSim\work\android_install_smoke_json_catalog_push_launch.json
```

This command installs the APK, pushes `data/packs/vanguard_th` into the app
external files directory, force-stops the package, and launches it. Latest
visual smoke on LDPlayer shows Home loading `Pack: Vanguard TH / 251`, `Cards
10836`, and Card Browser loading `Total 10836`, `Showing 24` from
`card_catalog.json`. Card images show expected fallback until the external image
dataset is provisioned on Android.
