# Manual Search And Category Filter Spec

Milestone: `M23-05`

## Purpose

Let players quickly narrow the in-app Manual without leaving Home or PlayTable.

## Scope

The first filter pass includes:

- Search text input.
- Category cycle control.
- Filtered manual body rendering.
- Empty-result fallback text.

## Design Decision

Use a category cycle button instead of a Unity `Dropdown` in this milestone.
The cycle button avoids dropdown-template complexity and is easier to smoke on
Windows. A richer category menu can be added later if needed.

## Rules

- Filtering is read-only and must not mutate `GameState`.
- Filtering must use `ManualContentCatalog` as the only runtime content source.
- Search matches section title, body, category, and related screen.
- Category options start with `All`, then the categories present in the manual.
- Empty results must show a player-facing fallback message.

## Acceptance

- Manual overlay exposes `Manual Search Input`, `Manual Category Button`, and
  `Manual Body`.
- Search narrows content.
- Category cycle narrows content.
- Empty search shows fallback text.
- Tests cover the pure filter helper and overlay controls.
