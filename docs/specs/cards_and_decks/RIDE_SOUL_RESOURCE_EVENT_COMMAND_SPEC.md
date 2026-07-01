# Ride And Soul Resource Event Command Spec

## Status

Active for `M21-04d`.

## Purpose

Close the core correctness gap found during M21 PlayTable work: the model has a
Soul zone, but ride-to-soul and live Soul resource effects must use the same
RulesCore command/event path as draw, move, replay, undo, bot simulation, and
online sync.

## Scope

This slice keeps the implementation conservative:

- Reuse `GameActionType.MoveCard` instead of adding a new action enum.
- Treat a `MoveCard` into `GameZone.Vanguard` as a ride/replacement move.
- Record displaced Vanguard card instance ids in `GameEvent.card_instance_ids`.
- Move displaced Vanguard cards to `GameZone.Soul`.
- Undo restores the new Vanguard card to its original source zone and moves the
  displaced cards from Soul back to Vanguard.
- SoulCharge is `MoveCard` from `Deck` to `Soul`.
- SoulBlast is `MoveCard` from `Soul` to `Drop`.

## Hidden State Rule

SoulCharge legal actions must not expose the top deck card id through labels or
public action summaries. The command may leave `card_instance_id` empty until
execution, and the committed `GameEvent` records the actual card id after the
card is revealed/moved.

## Acceptance Criteria

- Moving a card to Vanguard with an existing Vanguard moves the old Vanguard to
  Soul in the same committed event path.
- Undo restores both the new Vanguard source zone and the old Vanguard zone.
- Replay determinism accepts a ride-to-soul script.
- Structured `soul_charge` moves the top deck card to Soul through RulesCore.
- Structured `soul_blast` moves a Soul card to Drop through RulesCore.
- Preview remains no-mutation.
- Rejected Soul resource effects leave source state unchanged.
- No UI, bot, or structured ability path mutates `GameState` directly.

## Non-Goals

- Full Ride Phase legality.
- Persona Ride.
- Ride deck discard cost.
- Grade comparison.
- Circle selection.
- Private online commitment enforcement for newly revealed SoulCharge cards.

Those belong to later M21/M11-style phase/format slices.
