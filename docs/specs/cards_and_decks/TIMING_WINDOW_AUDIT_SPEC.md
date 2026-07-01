# Timing Window Audit Spec

## Status

Implemented in `M11-01`.

## Purpose

Inventory the timing/window concepts that exist before building the formal
Phase / Timing Matrix in `M11-02`.

This audit is read-only. It does not enforce rules, change legal actions, or
introduce a final timing-window model.

## Runtime Audit Catalog

`TimingWindowAuditCatalog.CreateCurrentReport()` records:

- `GamePhase`: `Mulligan`, `StandAndDraw`, `Ride`, `Main`, `Battle`, `End`
- `GameActionType`: `Draw`, `MoveCard`, `SetPhase`, `AddGiftMarker`
- `AbilityTiming`: `Manual`, `OnPlay`, `OnAttack`, `OnBoost`, `EndPhase`
- pending AUTO timing strings from `AbilityTriggerEventCollector`:
  - `OnDraw`
  - `OnMoveCard`
  - `OnSetPhase`
  - `OnAddGiftMarker`
- `TriggerCheckSource`: `Manual`, `Drive`, `Damage`
- `CombatModifierExpiration`: `Manual`, `EndOfBattle`, `EndOfTurn`,
  `Permanent`

The report round-trips through JSON for handoff/debug use.

## Known Gaps For M11-02

- `TypedTimingWindowEnumMissing`: no authoritative `TimingWindow` enum exists
  yet; pending AUTO timing currently uses strings.
- `PhaseTimingMatrixMissing`: no matrix maps commands/resolver windows to
  phases yet.
- `BattleStepWindowsMissing`: attack, guard, drive, damage, battle resolution,
  and close-step windows are not modeled yet.
- `CleanupNotPhaseIntegrated`: `EndOfBattle` and `EndOfTurn` cleanup previews
  are not wired to a phase/window controller.
- `TriggerCheckSourceNotWindow`: `Drive`, `Damage`, and `Manual` trigger check
  sources are context labels, not legal timing windows.

## Boundary

M11-01 must not:

- change `RulesCore` legal action behavior
- add or remove gameplay commands
- mutate `GameState`
- append to `GameState.event_log`
- publish network payloads
- implement phase/window enforcement

## Verification

EditMode coverage verifies:

- the audit includes every current enum value for phases, actions, ability
  timings, trigger check sources, and modifier expirations
- pending AUTO timing entries match the current collector mappings
- known M11 gaps are present and marked as gaps
- the audit report JSON round-trips
- no duplicate category/identifier pairs exist

## Next Work

`M11-02` converts this audit into the first Phase / Timing Matrix and starts
testing representative legal/rejected combinations.
