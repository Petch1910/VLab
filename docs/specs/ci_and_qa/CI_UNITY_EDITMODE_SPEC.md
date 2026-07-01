# CI Unity EditMode Spec

## Milestone

`M18-04`

## Goal

Add a GitHub Actions workflow for Unity EditMode tests. Unity compile CI remains
separate in `M18-03`.

## Workflow

`.github/workflows/unity-editmode.yml`

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
Unity.exe -batchmode -nographics -runTests -testPlatform EditMode -projectPath client\unity\VanguardThaiSim -testResults work\ci_unity_editmode_results.xml -logFile work\ci_unity_editmode.log
```

Artifacts:

- `client/unity/VanguardThaiSim/work/ci_unity_editmode.log`
- `client/unity/VanguardThaiSim/work/ci_unity_editmode_results.xml`

## Non-Goals

- No PlayMode tests.
- No Windows/Android build artifact. Those are `M18-08` and `M18-09`.
- No hosted GameCI migration.

## Verification

- Local Unity EditMode tests pass.
- Workflow preserves log and XML result artifacts.
