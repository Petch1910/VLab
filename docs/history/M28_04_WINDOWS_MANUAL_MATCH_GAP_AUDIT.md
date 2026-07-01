# M28-04 Windows Manual Match Gap Audit

## Scope

This audit reviews the Windows manual PlayTable after the M28-01 to M28-03
verification gates. It uses current code, tests, and smoke coverage only. It
does not add Android/mobile/release work and does not copy comparator assets,
code, or data.

## What Is Already Proven

- Core manual route works through RulesCore:
  setup, mulligan keep, Stand & Draw, Draw, Ride, Main, rear-guard call,
  Battle, attack, guard, Drive Check, Damage Check, and End.
- Local PlayTable can switch between P1 and P2 without recreating state.
- Online PlayTable remains seat-locked.
- PlayMode can drive the visible runtime UI through a two-seat local match
  smoke.
- Client smoke still reports zero blockers and keeps the M28-01 gameplay gate.

## Remaining Player-Facing Gaps

### P1: Single Next-Action Guidance

Current UI has separate `Setup`, `Battle Flow`, selection preview, zone counts,
and action buttons. Each is useful, but a new player still needs to infer the
next button from multiple panels.

Next action should be shown as one concise player-facing line, for example:

- `Next: choose Ride Deck card, then press VG.`
- `Next: press Stand to begin P1 turn.`
- `Next: select a hand card, then press VG to ride.`
- `Next: select attacker, then press Atk VG or target opponent card.`
- `Next: switch to P2, select a hand card, then press Guard or Damage Check.`

Recommended next task: `M28-05` PlayTable guided next-action panel.

### P1: Action Rows Are Too Dense

The current table exposes many short buttons together:

- Phase buttons: `Stand`, `Draw`, `Ride`, `Main`, `Battle`, `End`.
- Move/check buttons: `VG`, `Rear`, `Drop`, `Damage`, `Drive`,
  `Damage Check`, `Guard`, `Mulligan`, `Atk VG`, `Atk Target`, `Note`.

This is acceptable for a debug-capable manual simulator, but not yet ideal as a
player-facing Windows program. The next-action panel should land before any
large button regrouping so we can measure whether guidance alone is enough.

Recommended later task: split primary turn controls from selected-card actions
only if the guidance panel still leaves the table confusing.

### P2: Guard / Check Handoff Is Not Explicit Enough

After P1 attacks, `Battle Flow` says the opponent guard step is next, but the
UI does not explicitly tell the local user to press `Seat P2` in a two-seat
local game.

Recommended `M28-05` behavior: when an attack is declared in local mode, the
next-action line should mention switching to the opponent seat for guard or
damage check.

### P2: Trigger / Damage Check Semantics Are Still Manual

The current simulator intentionally keeps checks and trigger allocation manual.
This is correct for the current milestone, but the UI should say so plainly in
player-facing wording. Do not imply full automatic trigger application.

Recommended later task: add a concise manual trigger reminder near check
buttons or trigger zone status.

### P3: Event Log Is Readable Enough For Smoke, Not Yet Replay Review

`PlayTableEventLogFormatter` already converts core events into player-facing
sentences. It still hides useful context such as target zone, selected card
name, check source details, or whether the event came from local/online replay.

Recommended later task: improve event lines incrementally after the guided
next-action panel, using tests to avoid leaking hidden card ids.

### P3: Table Navigation Needs Real Windows Smoke Review

Automated PlayMode proves the route is possible. It does not prove visual
comfort in the built Windows player. A later manual Windows smoke should check
whether card buttons, hand strip, side panel, and action rows fit at common
desktop resolutions.

Recommended later task: Windows visual/manual smoke checklist after the guided
action pass.

## Decision

Do not jump to Android, release, advanced bot, or large UI rewrite. The best
next slice is small and high leverage:

`M28-05`: add a pure PlayTable guided next-action formatter and surface it on
the side panel. It should be testable in EditMode and must not mutate
`GameState`.

## Verification

Docs-only audit. Runtime verification is inherited from `M28-03`:

- PlayMode `unity_playmode_m28_03_two_seat_playmode_smoke_r2.xml` passed `2/2`.
- EditMode `unity_editmode_m28_03_two_seat_playmode_smoke.xml` passed
  `1131/1131`.
- Client smoke `client_smoke_m28_03_two_seat_playmode_smoke.log` passed with
  `blockers=[]`.
