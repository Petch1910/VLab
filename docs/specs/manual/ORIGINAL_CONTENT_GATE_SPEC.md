# Original Content Gate Spec

Milestone: `M23-04`

## Purpose

Protect the in-app Manual and Tutorial text from drifting into copied
comparator text, raw implementation language, or unsafe hidden-state details.

## Scope

This gate covers:

- `ManualContentCatalog` sections.
- `LoadingTipCatalog` text.

## Rules

- Manual/tutorial text must be original text written for this project.
- Comparator products may inform workflow thinking, but their text, UI copy,
  assets, icons, and data must not be copied into the runtime manual.
- Runtime manual/tutorial text must not include URLs, local absolute paths,
  raw protocol payload language, private card ids, or hidden state details.
- Cardfight!! Vanguard gameplay terms are allowed where needed to explain the
  game at a high level.

## Automated Guard

`ManualContentOriginalityGuard` scans the runtime manual and loading tips for:

- empty section fields
- missing required manual categories
- banned comparator/source names
- URL-like strings
- implementation/private-state wording

The guard is intentionally conservative. If a future manual section needs a
word that is currently banned, update this spec first and explain why the word
is safe for player-facing text.

## Acceptance

- Original content guard exists.
- EditMode tests pass for current Manual and loading tips.
- Docs and status identify the guard as the M23-04 closeout evidence.
