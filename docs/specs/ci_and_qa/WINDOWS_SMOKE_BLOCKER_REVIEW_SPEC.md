# M27-02 Windows Smoke Blocker Review Spec

## Goal

Review the M27-01 Windows player smoke report and fix any crash or blocker
found there.

## Input

- `client/unity/VanguardThaiSim/work/player_smoke_m27_01_windows_stability_smoke.json`

## Done Rule

- If blockers exist, fix them with code/tests and rerun the relevant Windows
  verification.
- If blockers are empty, record a docs-only closeout and move to M27-03.

## Result

The M27-01 smoke report has 8 steps and `blockers=[]`, so no runtime fix is
required for M27-02.
