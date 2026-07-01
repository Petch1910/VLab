# Cosmetic Metadata / Deck Legality Separation Spec

Milestone: `M22-05`

## Purpose

Lock the rule that deck cosmetics and accessory metadata do not affect deck
legality validation.

## Scope

`M22-05` is a guardrail and regression-test slice:

- Validate that changing cosmetic metadata does not change `DeckValidator`
  output.
- Validate that `DeckValidator` does not directly reference cosmetic fields or
  cosmetic model types.
- Keep appearance/accessory metadata player-facing only until a later explicit
  rules milestone says otherwise.

## Non-Goals

- No format legality expansion.
- No user asset file loading.
- No Deck Builder UI change.
- No online payload, Photon change, RulesCore change, Android work, app
  packaging, or release work.

## Acceptance

- Functional test compares validation results before and after cosmetic changes.
- Static source guard checks `DeckValidator` does not reference appearance or
  cosmetic models.
- Existing deck validation tests continue passing.
