# Seventh-Slice Human Repair Review Packet Spec

Milestone: `M61-01`

## Purpose

`M61-01` converts `M60-06` repair candidates and the `M60-closeout` decision
into a compact human/team review packet. The packet lets a reviewer choose one
seventh-slice recipe/repair candidate for later selection, acceptance, and
G Zone/Stride plus Bloom/token decision work.

This milestone does not record selection, does not record acceptance, does not
record a G Zone/Stride decision, does not record a Bloom/token decision, and
does not promote runtime decks.

## Inputs

- `outputs/target_slice/m60_closeout_seventh_slice_runtime_readiness.json`
- `outputs/target_slice/m60_06_seventh_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m60_03_seventh_slice_recipe_draft_model.json`

Tests may pass in-memory M60 reports until real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m61_01_seventh_slice_human_repair_review_packet.json`
- `outputs/target_slice/m61_01_seventh_slice_human_repair_review_packet.md`
- `outputs/target_slice/m61_01_seventh_slice_human_repair_review_packet.csv`

## Review Rules

- Include all repair candidates that are ready for human repair review.
- Preserve pair context from the recipe draft.
- Preserve manual substitution previews from M60-06.
- Preserve grade-profile additions/removals from M60-06.
- Preserve G Zone deferred system-work context from M60-06.
- Preserve Bloom/token deferred system-work context from M60-06.
- Provide decision options only; do not choose an option automatically.
- Mark `ready_for_m61_02=true` only if `M60-closeout` allows human selection
  review and every review item is ready.

## Runtime Boundary

This milestone must not:

- modify M60 recipe drafts
- record human selection
- record human acceptance
- record G Zone/Stride decision
- record Bloom/token decision
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report exports `23` review items. All `23` are ready for
human repair review and have complete manual repair previews. `21` have complete
grade-profile repair previews, while `2` do not need grade repair. All `23`
preserve G Zone deferred and Bloom/token deferred decision context. Runtime
promotion remains disabled.

## Verification

```powershell
python -m unittest tests.test_seventh_slice_human_repair_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M60-closeout, M60-06, and M60-03 outputs exist:

```powershell
python tools\deck\build_seventh_slice_human_repair_review_packet.py
```

## Done Rule

`M61-01` is done when:

- all `23` M60 repair candidates are exported as review items
- manual repair, grade repair, G Zone, and Bloom/token contexts are preserved
- no human selection, acceptance, or system decision is recorded
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m61_02=true`
