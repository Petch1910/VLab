# M27-03 Windows Performance Baseline Spec

## Goal

Record a baseline for the Windows Card Browser and Deck Builder data paths
before adding hard memory/performance gates in M27-04.

## Scope

The baseline records:

- runtime pack repository load time
- card query time
- card detail load time
- deck validation time
- deck code export/import round-trip time

## Non-Goals

- Do not enforce hard timing thresholds in this milestone.
- Do not profile GPU, rendering, or interactive scroll yet.
- Do not run Android/mobile/app packaging verification.

## Verification

- Unity compile.
- Unity EditMode tests.
- Windows player smoke is not required unless the baseline is wired into the
  player smoke path.
