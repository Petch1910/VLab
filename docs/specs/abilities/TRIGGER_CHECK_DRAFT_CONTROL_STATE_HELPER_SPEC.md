# Trigger Check Draft Control-State Helper Spec

## Status

Implemented in `M10-30`.

## Purpose

Extract PlayTable trigger-check draft control enabled/disabled rules into a pure
UI helper for `TrigLog`, `DraftTrig`, and `ClrDraft`.

This keeps the existing button behavior unchanged while making the rules
reusable for future compact, mobile, or controller-friendly UI.

## Inputs

- online/local mode flag
- whether a trigger-check replay log can be published
- selected card instance id
- selected card id

## Output

- `can_publish_trigger_log`
- `can_publish_manual_draft`
- `can_clear_selection`

## Boundary

The helper must not:

- know about Unity `Button` instances
- know about multiplayer sessions or transports
- send payloads
- move cards
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- local mode disables all controls
- online no-selection state can publish an available trigger log but cannot
  draft or clear selection
- online visible-card selection enables draft and clear
- online hidden-card selection disables draft but allows clear
- missing selected card id disables draft without disabling clear
- PlayTable uses the helper without behavior change
- Unity compile and EditMode tests pass

## Future Extensions

- tooltip/reason text for disabled controls
- compact mobile trigger-check toolbar state model
- shared control state for manual draft and real trigger-check UI
