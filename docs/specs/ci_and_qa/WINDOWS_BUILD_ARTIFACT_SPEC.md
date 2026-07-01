# Windows Build Artifact Spec

## Milestone

`M18-08`

## Goal

Produce a repeatable Windows x64 Unity build artifact for local release
candidate checks.

## Build Runner

Editor command:

```text
VanguardThaiSim.EditorTools.WindowsBuildArtifactRunner.RunFromCommandLine
```

Default artifact:

```text
client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe
```

Optional override:

```powershell
-windowsBuildOutput "build/windows/latest/VanguardThaiSim.exe"
```

The runner:

- uses enabled scenes from `EditorBuildSettings`
- targets `BuildTarget.StandaloneWindows64`
- writes under the Unity project `build/` directory by default
- copies the runtime card pack from `data/packs/vanguard_th/` to
  `build/windows/latest/data/packs/vanguard_th/`
- exits `0` only when `BuildPipeline.BuildPlayer` succeeds and the `.exe`
  exists and the runtime pack copy succeeds

Runtime UI requirement:

- `ProjectSettings.activeInputHandler` must remain `Both` while the runtime UI
  uses `StandaloneInputModule`; setting it to Input System-only makes mouse and
  touch UI clicks fail in the player.

## Non-Goals

- No Android artifact. That is `M18-09`.
- No installer, updater, or GitHub Release upload.
- No full card-image dump bundling. The Windows artifact copies the runtime
  manifest, SQLite database, asset index, verification report, and ability data
  pack only.
- No Photon ranked/security guarantee.

## Verification Command

```powershell
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
$logPath = Join-Path $projectPath "work\windows_build_m18_08.log"
& $unityExe -batchmode -nographics -quit -projectPath $projectPath `
  -executeMethod VanguardThaiSim.EditorTools.WindowsBuildArtifactRunner.RunFromCommandLine `
  -logFile $logPath
```

Expected output:

- Unity process exits `0`
- `client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe`
  exists
- `client/unity/VanguardThaiSim/build/windows/latest/data/packs/vanguard_th/manifest.json`
  exists
- `client/unity/VanguardThaiSim/build/windows/latest/data/packs/vanguard_th/cards.sqlite`
  exists
- build log contains `Windows build artifact result: Succeeded`
- build log contains `Windows build runtime pack copied:`

## Next Target

After this gate passes, continue with `M18-09 Android build artifact`.
