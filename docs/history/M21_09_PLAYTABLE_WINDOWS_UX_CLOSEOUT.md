# M21-09 PlayTable Windows UX Closeout

Status: Done

## Summary

`M21` closes the Windows PlayTable UX pass. The table is now more player-facing
and less like a debug harness while preserving developer diagnostics behind
Advanced UI.

## Completed Scope

- Board/table area is the dominant PlayTable surface.
- Zone counts and status are visible for core zones, including ride deck and
  soul.
- Local hand strip and selected-card preview are available.
- Board card faces show minimal thumbnail/name summaries.
- Common actions are phase-aware and use legal actions through `RulesCore`.
- Check/Guard, attack target selection, battle flow guidance, and manual notes
  are available from the player-facing surface.
- Setup flow now blocks unplayable decks, supports first Vanguard placement
  from ride deck, selected-card mulligan, Stand/Ride phase actions, and setup
  guidance.
- Trigger draft, pending AUTO, manual resolution, apply preview, and related
  diagnostics are hidden inside Advanced by default.
- Event/replay log now reads as a match log instead of reducer trace text.

## Current Verification Baseline

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_07_player_event_log.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_07_player_event_log.xml`
  passed `982/982`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_07_player_event_log.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_07_player_event_log.json`
  passed with `blockers=[]`.

## Remaining Product Limits

- The PlayTable is still a manual simulator first. Structured ability
  automation and CPU/bot work remain gated for later milestones.
- The default layout is cleaner, but full art direction/accessory polish is
  intentionally deferred to `M22`.
- Android, APK, mobile QA, app packaging, release-candidate, and public
  distribution work remain deferred.

## Next

Start `M22`: Windows Settings / Deck Type / Accessories.
