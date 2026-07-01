# Android Build Artifact Spec

## Milestone

`M18-09`

## Goal

Produce a repeatable Android APK Unity build artifact for local mobile release
candidate checks.

## Build Runner

Editor command:

```text
VanguardThaiSim.EditorTools.AndroidBuildArtifactRunner.RunFromCommandLine
```

Default artifact:

```text
client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk
```

Optional override:

```powershell
-androidBuildOutput "build/android/latest/VanguardThaiSim.apk"
```

The runner:

- uses enabled scenes from `EditorBuildSettings`
- verifies Android build target support is installed
- switches active build target to Android
- forces APK output with `EditorUserBuildSettings.buildAppBundle = false`
- writes under the Unity project `build/` directory by default
- exits `0` only when `BuildPipeline.BuildPlayer` succeeds and the `.apk`
  exists

## Non-Goals

- No Android App Bundle output.
- No Play Store signing/release upload.
- No installer/updater work.
- No mobile performance optimization beyond existing M16 QA gates.
- The APK does not yet bundle the runtime card database/catalog internally.
  Local Android smoke provisions `data/packs/vanguard_th` with
  `tools/smoke/android_install_smoke.py --push-pack`.
- Full card images remain external and are not bundled into the APK.

## Verification Command

```powershell
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
$logPath = Join-Path $projectPath "work\android_build_m18_09.log"
& $unityExe -batchmode -nographics -quit -projectPath $projectPath `
  -executeMethod VanguardThaiSim.EditorTools.AndroidBuildArtifactRunner.RunFromCommandLine `
  -logFile $logPath
```

Expected output:

- Unity process exits `0`
- `client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk`
  exists
- build log contains `Android build artifact result: Succeeded`

Latest verified Android rebuild after M19-09 icon override loader:

```text
client/unity/VanguardThaiSim/work/android_build_m19_09_icon_override_loader.log
```

The APK was then installed on LDPlayer with runtime pack push and launched via:

```powershell
python tools\smoke\android_install_smoke.py --adb auto --push-pack --force-stop-before-launch --launch --timeout 600 --output client\unity\VanguardThaiSim\work\android_install_smoke_m19_09_icon_override_loader.json
```

## Next Target

After this gate passes, use the install-smoke helper for local device/emulator
validation, then continue release packaging only when a distribution channel is
chosen.
