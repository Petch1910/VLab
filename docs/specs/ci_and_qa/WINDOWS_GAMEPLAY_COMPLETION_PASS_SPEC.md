# Windows Gameplay Completion Pass Spec

## Purpose

`M28` starts the post-M27 Windows gameplay completion pass. The goal is to
prove the Windows manual PlayTable can run a longer player-facing Vanguard flow
than the earlier M27-06 PlayMode smoke.

This is still a manual simulator gate. It does not claim full card ability
automation, strict comprehensive-rule completion, ranked-server security, or
mobile readiness.

## Scope

Allowed:

- Windows-only compile, EditMode, PlayMode, and local player smoke.
- Manual PlayTable flow verification through the existing RulesCore command
  facade.
- Event/replay determinism verification for the smoke flow.
- Docs/status updates for the active post-M27 target.

Deferred:

- Android, APK, LDPlayer, mobile layout QA, app packaging, release-candidate
  packaging, and public distribution.
- Runtime LLM card-text parsing.
- Expanding bot/ISMCTS/RL beyond existing correctness gates.
- Comparator asset/code/data copying.

## M28-01 Gameplay Completion Gate

The verifier must execute a single deterministic manual match-readiness route:

1. Create two player game states from a valid smoke deck.
2. Set first Vanguard for both players from Ride Deck.
3. Keep opening hands through the mulligan command.
4. Progress P1 through Stand & Draw, Draw, Ride, Main, Battle, and End.
5. Ride from hand so the previous Vanguard moves to Soul.
6. Call a rear-guard from hand.
7. Declare an attack against the opponent Vanguard.
8. Guard from opponent hand.
9. Resolve one Drive check and one Damage check through committed events.
10. Verify final zone counts and replay determinism.

## Acceptance Criteria

`M28-01` is accepted when:

- `WindowsGameplayCompletionVerifier.Run()` returns `accepted=true`.
- The report has zero blockers and at least 16 committed events.
- P1 has one Vanguard, at least one Soul card, at least one rear-guard, and at
  least one Trigger-zone card.
- P2 has one Vanguard, at least one Guardian card, and at least one
  Trigger-zone card.
- The final phase is `End`.
- `ReplayDeterminismVerifier` accepts the full event log.
- `ClientSmokeFlowVerifier` uses this gate instead of the older three-event
  PlayTable smoke.
- EditMode tests cover the gate and report JSON round-trip.

## M28-02 Local PlayTable Seat Toggle

The M28-01 core/smoke route can drive both players through RulesCore, but the
local PlayTable UI is still centered on the active local player perspective.
Before adding a longer UI-level match smoke, the Windows PlayTable must allow
manual local seat switching:

- Local mode can switch between P1 and P2 without recreating the game state.
- Online mode remains locked to the session local player and must not expose a
  seat switch that changes the authoritative local player index.
- Switching seats clears selected card/target state and refreshes zone/status
  panels for the selected seat.
- The toolbar label must make the current manual seat clear.
- Tests must cover local switching, online lockout, and no `GameState`
  mutation.

## M28-03 UI-Level Two-Seat Match Smoke

After `M28-02`, PlayMode coverage should drive both local seats through the
runtime PlayTable UI:

- P1 sets first Vanguard from Ride Deck.
- Seat toggles to P2 and P2 sets first Vanguard from Ride Deck.
- Seat toggles back to P1 for Stand & Draw, Draw, Ride, Main, rear-guard call,
  Battle, and attack.
- Seat toggles to P2 for Guard and Damage Check.
- Seat toggles back to P1 for Drive Check and End phase.
- Assertions must inspect runtime UI state and zone counts, not direct state
  mutation.

Closeout: `docs/history/M28_03_UI_TWO_SEAT_MATCH_SMOKE_CLOSEOUT.md`.

## M28-04 Windows Manual Match Gap Audit

After the two-seat UI smoke passes, the next step is not mobile/release work.
The next step is a Windows-only audit of what still makes the manual PlayTable
feel unfinished as a player-facing program:

- Setup readability and first Vanguard / mulligan guidance.
- Turn and phase navigation clarity.
- Battle, guard, Drive Check, and Damage Check button ordering.
- Zone count and selected-card feedback during seat switching.
- Event/replay log readability for player use.
- Remaining obvious blockers before expanding bot or automation again.

This audit may add tests or specs if a gap is concrete, but it must not add
Android, APK, release, or comparator asset-copying work.

Closeout: `docs/history/M28_04_WINDOWS_MANUAL_MATCH_GAP_AUDIT.md`.

## M28-05 PlayTable Guided Next-Action Panel

The highest-leverage gap from M28-04 is that the table has many useful panels
and buttons, but no single player-facing line that says what to do next.

`M28-05` should add a pure formatter plus PlayTable surface for a concise
next-action hint. It must:

- Use display state and current local player index only.
- Avoid hidden-state leaks.
- Avoid direct `GameState` mutation.
- Preserve existing action buttons and RulesCore behavior.
- Cover setup, mulligan/stand, ride, main/call, battle/attack, guard/check,
  drive check, and end-phase hints.

Closeout: `docs/history/M28_05_PLAYTABLE_GUIDED_NEXT_ACTION_CLOSEOUT.md`.

## M28-06 Windows Built-Player Smoke

After the guided next-action UI lands, rebuild the Windows player and run the
built-player smoke path:

- Build `client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe`.
- Run the player with `-vanguardPlayerSmoke`.
- Write the report to `client/unity/VanguardThaiSim/work/`.
- Confirm `blockers=[]`.

This validates the actual Windows executable path, not only Editor compile,
EditMode, PlayMode, or client smoke.

Closeout: `docs/history/M28_06_WINDOWS_BUILT_PLAYER_SMOKE_CLOSEOUT.md`.

## M28-07 PlayTable Action Grouping Polish

After the built-player smoke passes, improve the visible PlayTable action rows
without touching RulesCore:

- Make phase controls, selected-card moves, battle/check controls, and utility
  controls easier to scan.
- Prefer clearer labels where button width permits.
- Preserve all existing button commands and legal-action gating.
- Add formatter/layout tests where practical.
- Run Unity compile, EditMode, PlayMode, client smoke, and Windows player smoke
  if runtime UI changes are made.

Closeout: `docs/history/M28_07_PLAYTABLE_ACTION_GROUPING_POLISH_CLOSEOUT.md`.

## M28-08 PlayTable Side-Panel Density Audit

After M28-05 and M28-07 add guidance text, audit the PlayTable side panel for
visual density and scan fit:

- Confirm the side panel still exposes Next Action, Setup, Battle Flow, Manual
  Notes, Selected Card Preview, Zone Status, and Advanced drawer access.
- Identify whether any panel should move into Advanced, collapse, or be shortened.
- Prefer audit/spec first before another UI rewrite.
- Keep the audit Windows-only.

Closeout: `docs/history/M28_08_PLAYTABLE_SIDE_PANEL_DENSITY_AUDIT.md`.

## M28-09 Move Bot Plan Out Of Primary Manual Panel

The M28-08 audit identifies `Bot Plan` as the safest first density reduction.
`M28-09` should:

- Keep bot explanation functionality.
- Move Bot Plan into Advanced drawer or hide it when no bot trace is active.
- Preserve tests for bot explanation formatting.
- Add a UI surface test that the primary side panel still has Next Action,
  Setup, Battle Flow, Selected Card Preview, Zone Status, and Match log.
- Run compile/EditMode/PlayMode/client smoke and Windows player smoke because
  this changes runtime UI.

Closeout: `docs/history/M28_09_BOT_PLAN_ADVANCED_DRAWER_CLOSEOUT.md`.

## M28-10 Match Log / Preview Density Review

After Bot Plan moves to Advanced, review the remaining side-panel density:

- Selected Card Preview should stay primary.
- Match Log may need a compact summary plus Advanced full log if the side panel
  still feels cramped.
- Prefer a small formatter/layout helper before a larger PlayTable rewrite.

## Guardrails

- No UI, bot, network, or smoke helper may mutate `GameState` directly.
  Gameplay changes must go through `RulesCore.ExecuteOrThrow` or
  `RulesCore.TryExecute`.
- This gate must not weaken hidden-state, no-mutation, or replay determinism
  tests.
- This gate must not introduce comparator assets, icons, data, playmats, or
  code.
