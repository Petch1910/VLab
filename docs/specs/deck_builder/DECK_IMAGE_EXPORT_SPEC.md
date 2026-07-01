# Deck Image Export Spec

Milestone: `M24-08`

## Purpose

Add a first-pass Windows Deck Builder image export so a player can save a PNG
snapshot of the current deck-building screen for local sharing/review.

This closes the Product Spec gap for `export deck image` without starting
release/distribution work.

## UI Entry

Deck Builder -> Deck Tools adds:

- `Export Image`

## Behavior

- Export creates a PNG screenshot of the current Deck Builder screen.
- The Deck Tools dialog closes before capture so the image shows the deck
  builder workspace rather than the modal.
- Export path is under Unity persistent data:

```text
<persistentDataPath>/deck_exports/deck_<safe-deck-name>_<yyyyMMdd_HHmmss>.png
```

- The status text reports the planned path.
- Failed planning must not mutate the deck.

## Boundaries

- No public release artifact.
- No upload/share integration.
- No Android/mobile export path.
- No third-party asset copying.
- No custom card pack mutation.
- No deck legality changes.

## Future Polish

A later UI polish can render a dedicated deck-list image with card thumbnails,
counts, and accessories. This milestone intentionally uses a reliable Windows
screenshot path first.

## Verification

EditMode tests should cover:

- null deck rejection
- safe filename creation
- path stays under export root
- accepted/rejected status formatting

