# Deck Import Mismatch UI Spec

Milestone: `M24-03`

## Purpose

After a deck code or count-line deck text import succeeds, show player-facing
compatibility warnings before the user starts a game.

## Checks

- Missing card ids from the active card pack.
- Deck pack id mismatch.
- Deck pack version mismatch.
- Pack definition hash mismatch when imported text provides a hash.

## Rules

- Parse failures still reject without mutating the active deck.
- Compatibility warnings do not mutate card data or deck legality rules.
- Missing cards should be shown as warnings/errors in Deck Tools and normal deck
  validation should still show them in the deck panel.
- Compact `VGTH1.` deck code does not currently embed pack definition hash, so
  hash mismatch can only be detected for count-line text with
  `PackDefinitionHash`.

## Acceptance

- Analyzer and formatter cover missing card, pack version, and hash mismatch.
- Applying compact deck code shows compatibility status.
- Applying count-line deck text shows compatibility status including hash when
  present.
- Existing deck import behavior remains compatible.
