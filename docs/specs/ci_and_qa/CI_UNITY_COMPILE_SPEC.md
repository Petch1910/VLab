# CI Unity Compile Spec

## Milestone

`M18-03`

## Goal

Add a GitHub Actions workflow for Unity batchmode compile only. Unity EditMode
tests remain separate for `M18-04`.

## Workflow

`.github/workflows/unity-compile.yml`

Triggers:

- `push`
- `pull_request`

Runner:

- self-hosted Windows runner with labels: `self-hosted`, `Windows`, `Unity`

Unity path:

- uses `UNITY_EXE` environment variable when set
- otherwise falls back to `%LOCALAPPDATA%\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe`

Command:

```powershell
Unity.exe -batchmode -nographics -quit -projectPath client\unity\VanguardThaiSim -logFile work\ci_unity_compile.log
```

The workflow rejects logs containing:

- `fatal error`
- `error CS`
- `Compilation failed`
- `Scripts have compiler errors`

## Non-Goals

- No Unity EditMode tests. That is `M18-04`.
- No Windows/Android build artifact. Those are `M18-08` and `M18-09`.
- No GameCI hosted setup yet. This workflow assumes a controlled self-hosted
  Windows machine with Unity installed/licensed.

## Verification

- Local Unity batchmode compile passes.
- Workflow command matches local compile behavior.
