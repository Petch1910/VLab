# Pending Auto Ability Queue Masking Spec

## Status

Implemented in `M10-47`.

## Purpose

Provide a pure masking helper for pending AUTO ability queues before they are
shown in spectator/opponent views or transported across privacy-sensitive
online paths.

This scaffold prevents pending ability prompts from leaking source card
identity when the viewer should not know the card.

## Perspectives

- `TrueState`: preserves all pending ability fields.
- owner `Player` view: preserves pending ability fields where
  `player_index == viewerPlayerIndex`.
- non-owner `Player` view: masks source card identity.
- `Spectator`: masks all source card identity.

## Masked Fields

For masked pending abilities:

- `hides_source_card_identity = true`
- `source_card_instance_id` becomes a deterministic hidden id
- `source_card_id` becomes `GameStateViewFactory.HiddenCardId`
- `pending_id` becomes a deterministic hidden pending id
- `summary` is rebuilt without source card identity

## Boundary

The masker must not:

- mutate the source queue
- mutate `GameState`
- resolve ability text or effects
- pay costs
- consume RNG
- publish network payloads
- decide simultaneous AUTO order

## Acceptance Tests

- true-state view preserves source identity
- owner player view preserves own pending ability and masks opponent entries
- spectator view masks ids, summaries, and source card identity
- masked output is deterministic
- source queue is unchanged

## Future Extensions

- pending ability prompt payload codec
- owner-private transport for ability choice prompts
- PlayTable pending ability sidebar
- RulesCore command for committing pending ability resolution
