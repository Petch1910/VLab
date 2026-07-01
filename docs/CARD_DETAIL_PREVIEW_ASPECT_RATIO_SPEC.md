# Card Detail Preview Aspect Ratio Spec

Milestone: `M31-05`

## Purpose

Fix the Card Browser / Deck Builder selected-card detail preview so card images
keep their card-like aspect ratio instead of stretching horizontally.

## Evidence

M31-04 visual evidence:

- `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/deck_builder.png`
- `client/unity/VanguardThaiSim/work/m31_04_visual_evidence/card_browser.png`

Both show the large selected-card preview image stretched into a wide banner,
while card grid thumbnails are acceptable.

## Boundaries

- Do not change card data or image files.
- Do not change search/query behavior.
- Do not change deck validation.
- Do not copy comparator assets/code/data.
- Windows-only verification for this slice.

## Minimum Slice

- Keep the selected-card detail image inside a stable portrait-oriented frame.
- Preserve existing detail text and deck add/remove actions.
- Add layout/helper tests where practical.
- Recapture visual evidence or otherwise verify the image no longer stretches.
