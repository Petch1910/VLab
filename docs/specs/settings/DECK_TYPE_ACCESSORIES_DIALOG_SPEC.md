# Deck Type / Accessories Dialog Spec

Milestone: `M22-04`

## Purpose

Expose deck-level format and appearance metadata from the Windows Deck Builder.
This uses `DeckAppearanceMetadata` from `M22-02`.

## Scope

`M22-04` is a Deck Builder UI slice:

- Add a player-facing `Deck Type / Accessories` button in Deck Builder.
- Open a dedicated dialog separate from Deck Tools import/export.
- Show current deck format and appearance keys.
- Allow cycling session deck format.
- Allow cycling sleeve, card back, playmat, crest, persona shield, gift marker,
  and quick shield keys.
- Keep changes on `activeDeck` only.

## Non-Goals

- No user asset file loading.
- No manifest/hash validation; that is `M22-06`.
- No deck legality changes; that is explicitly guarded by `M22-05`.
- No online payload, Photon change, RulesCore change, Android work, app
  packaging, or release work.

## Acceptance

- Deck Builder shows a `Deck Type / Accessories` button.
- Dialog opens and closes without leaving Deck Builder.
- Summary is generated from normalized `DeckAppearanceMetadata`.
- Cycling format/accessory keys updates `activeDeck` metadata.
- EditMode tests cover formatter summaries, cycling behavior, and deck
  validation not changing when appearance metadata changes.
