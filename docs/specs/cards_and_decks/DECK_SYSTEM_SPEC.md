# Deck System Spec

## Deck Model

```json
{
  "deck_id": "string",
  "name": "string",
  "format": "D",
  "card_pack_id": "kk-vgth-251",
  "main": {},
  "ride": {},
  "g": {},
  "cosmetics": {
    "sleeve": null,
    "playmat": null,
    "crest": null,
    "persona_shield": null
  }
}
```

## Validator

Validator must check:

- main deck count
- ride deck count
- G deck count when format requires
- copy limits
- banned/restricted cards
- trigger rules
- format legality
- missing card ids
- card pack version mismatch

## Deck Code

Recommended format:

```text
VGTH1.<base64url(compressed-json)>
```

Deck code must include:

- format
- card pack version
- card counts
- optional cosmetics

## UX Requirements

- Show counters live
- Show invalid reason clearly
- Let user export/import code
- Let user save multiple decks
- Deck Builder filter/status text is formatted through
  `DeckBuilderFilterPanelFormatter` so card pool filters, deck counts, issue
  totals, and playable state stay deterministic and test-covered.
