# M31-01 Windows UI Evidence Capture And Polish Audit

Date: 2026-06-28

## Scope

Review the current Windows UI direction after `M30` completed the playable loop
and decide the next small implementation slice.

## Evidence Reviewed

- `docs/UI_EXPERIENCE_REDESIGN_SPEC.md`
- `docs/UI_UX_SPEC.md`
- `docs/WINDOWS_UI_EVIDENCE_AUDIT_SPEC.md`
- `docs/WINDOWS_PLAYABLE_LOOP_CHECKLIST.md`
- `docs/history/M30_06_WINDOWS_PLAYABLE_LOOP_CLOSEOUT_AUDIT.md`
- Latest player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_05_playtable_replay_export.json`
- Latest client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m30_05_playtable_replay_export.log`
- Earlier user-provided Windows screenshot recorded in
  `docs/UI_EXPERIENCE_REDESIGN_SPEC.md`.

## Findings

### Passed

- Functional Windows loop has no automated blocker after M30.
- Home, Deck Builder, Manual, Settings, Online Room, PlayTable, and layout smoke
  all pass in the latest smoke reports.
- Raw `Loading` placeholder labels were already replaced in M19-level state
  formatters, and Card Browser populates `All series` / taxonomy options after
  repository load.

### Risks

- Automated smoke is not visual acceptance. It can pass while the first screen
  still feels like an internal tool.
- The Card Workshop/Card Browser area remains the highest-friction first-run
  surface because it has search, filters, cache, pagination, deck state, and
  card grid controls in one dense screen.
- The earlier screenshot showed the exact failure mode to prevent: ambiguous
  loading/filter state plus a large blank grid with no player-facing next step.
- Replay import/export is technically connected, but its raw path workflow is
  polish, not the highest next issue.

## Decision

Next target: `M31-02` Card Workshop first-screen clarity pass.

Minimum slice:

- Add or update a focused spec.
- Improve first-screen labels/status so Card Browser and Deck Builder explain
  whether data is preparing, ready, empty, or filtered.
- Keep repository, deck validation, pack data, and RulesCore unchanged.
- Add formatter/UI tests and run Unity compile/EditMode/client smoke/Windows
  player smoke if runtime UI changes.
