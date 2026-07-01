# Home Settings Screen Spec

Milestone: `M22-03`

## Purpose

Expose a Settings screen from the Windows Home screen using the `PlayerSettings`
model introduced in `M22-01`.

## Scope

`M22-03` is a Windows Home UI slice:

- The existing Home `Settings` button opens a real settings screen/panel.
- The screen displays normalized `PlayerSettings`.
- The screen can adjust session-local preferred format and image cache mode.
- The screen can close back to Home.

## Non-Goals

- No filesystem persistence yet.
- No full settings editor for every field yet.
- No Deck Builder accessories dialog; that is `M22-04`.
- No user-provided asset manifest loading; that is `M22-06`.
- No online payload, Photon change, RulesCore change, deck validation change,
  Android work, app packaging, or release work.

## Acceptance

- Home `Settings` button opens a visible `Settings Screen`.
- The screen summary is generated from normalized `PlayerSettings`.
- Preferred format cycles through `D`, `V`, and `Premium`.
- Image cache mode cycles through `Balanced`, `MemorySaver`, and `HighQuality`.
- Closing the panel returns to the Home screen without destroying the Home
  lobby.
- Formatter tests cover null/default/custom settings and cycling helpers.
