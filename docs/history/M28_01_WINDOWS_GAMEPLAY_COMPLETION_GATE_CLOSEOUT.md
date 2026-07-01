# M28-01 Windows Gameplay Completion Gate Closeout

## Status

Done.

## Scope

Added a post-M27 Windows gameplay completion smoke gate. This replaces the old
short PlayTable smoke route with a deterministic manual match-readiness route
that exercises setup, ride, call, battle, guard, trigger checks, End phase, and
replay determinism through RulesCore commands.

This is still a Windows-only manual simulator gate. It does not claim full card
ability automation, strict comprehensive-rule completion, ranked-server
security, Android readiness, APK packaging, or public release readiness.

## Code Changes

- Added `WindowsGameplayCompletionVerifier` and
  `WindowsGameplayCompletionReport`.
- Updated `ClientSmokeFlowVerifier` so the PlayTable smoke step now uses the
  gameplay completion gate instead of the older three-event route.
- Added EditMode tests for manual match-loop readiness and report JSON
  round-trip.

## Verified Flow

The M28-01 verifier executes:

1. P1/P2 first Vanguard setup from Ride Deck.
2. P1/P2 keep opening hand through Mulligan command.
3. P1 Stand & Draw, Draw, Ride, Main, call rear-guard, Battle.
4. P1 attack declaration.
5. P2 guard from hand.
6. P1 Drive check and P2 Damage check.
7. P1 End phase.
8. Replay determinism verification over the committed event log.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_01_windows_gameplay_completion.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_01_windows_gameplay_completion_r2.xml`
  passed `1129/1129`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_01_windows_gameplay_completion.xml`
  passed `1/1`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_01_windows_gameplay_completion.log`
  passed with `blockers=[]` and PlayTable smoke:
  `Windows gameplay completion smoke passed with 16 committed event(s).`

## Guardrails Preserved

- No Android, APK, LDPlayer, mobile QA, app packaging, release candidate, or
  public distribution work was run.
- No comparator assets, code, icons, playmats, card data, or pack files were
  copied.
- The smoke route uses `RulesCore.ExecuteOrThrow` and does not mutate
  `GameState` directly from UI/smoke code.
- Replay determinism is verified for the full route.

## Follow-Up

Proceed to `M28-02`: local PlayTable seat toggle. The M28-01 core/smoke route
can control both players, but the local Windows PlayTable UI is still centered
on one player perspective. A seat toggle is needed before a longer UI-level
manual match smoke can honestly cover both players.
