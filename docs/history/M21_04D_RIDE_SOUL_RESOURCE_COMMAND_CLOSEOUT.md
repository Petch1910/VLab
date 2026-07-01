# M21-04d Ride And Soul Resource Command Closeout

## Status

`M21-04d` is complete.

## Completed Changes

- Added `docs/specs/cards_and_decks/RIDE_SOUL_RESOURCE_EVENT_COMMAND_SPEC.md`.
- `MoveCard` into `GameZone.Vanguard` now displaces existing Vanguard cards to
  `GameZone.Soul` through the reducer/event path.
- Displaced Vanguard ids are recorded in `GameEvent.card_instance_ids` for
  replay and undo.
- Undo restores the new Vanguard card to its source zone and moves displaced
  cards from Soul back to Vanguard.
- Legal SoulCharge action uses `Deck -> Soul` without exposing top deck card id
  in the legal action label or command id before execution.
- Structured `soul_charge` now moves the top deck card to Soul through
  RulesCore.
- Structured `soul_blast` now moves a Soul card to Drop through RulesCore.
- Manual fallback tests now use the explicit unsupported `manual` effect instead
  of SoulCharge.

## Safety

- No UI, bot, or network code mutates `GameState` directly.
- No new action enum was added; this avoids broad protocol churn.
- Top deck identity is not exposed in the SoulCharge legal action surface.
- Preview paths still clone state before applying structured effects.
- Rejected SoulBlast leaves source state unchanged.
- Replay determinism covers the ride-to-soul command script.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_04d_ride_soul_resource.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_04d_ride_soul_resource.xml`
  passed `926/926`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_04d_ride_soul_resource.log`
  reports `Windows build artifact result: Succeeded`, `errors=0`,
  `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_04d_ride_soul_resource.json`
  reports four steps and `blockers=[]`.

## Next Target

`M21-04c`: render minimal card image/thumbnail or readable card face summary on
board circles. Keep it visual-only and use existing `CardImageCache`; do not
copy comparator assets.
