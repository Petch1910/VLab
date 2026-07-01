# Android Touch/Layout QA Spec

## Milestone

`M16-09`

## Goal

Lock the existing responsive UI profiles against Android-sized viewport
regressions without redesigning screens or changing gameplay behavior.

## Scope

- Add deterministic QA checks for Android phone portrait, phone landscape, and
  tablet viewports.
- Require Android phone/tablet profiles to expose at least `48` pixels of touch
  target height.
- Verify Card Browser and Play Table toolbar width budgets fit inside each
  profile reference resolution.
- Verify card tile image/label space, detail/deck panel width, play side panel
  width, and play table vertical budget do not collapse.

## Non-Goals

- No gameplay, bot, network, replay, card data, or image data changes.
- No new runtime telemetry.
- No scene redesign or control reordering.

## Implementation

- `ResponsiveLayoutProfile` keeps the existing profile model.
- Phone landscape and tablet touch target heights are hardened to `48`.
- `ResponsiveLayoutQaVerifier` runs pure layout checks against fixed Android
  reference viewports and returns a report with issue codes.

## Verification

- EditMode tests validate all Android reference viewports pass.
- Negative tests verify collapsed touch targets and toolbar overflow produce
  deterministic issue codes.
- Unity compile and full EditMode suite must pass after the change.
