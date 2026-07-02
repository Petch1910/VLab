# Ninth-Slice Human Repair Review Packet Spec

Milestone: `M69-01`

## Purpose

`M69-01` converts `M68-06` repair candidates and the `M68-closeout`
decision into a compact human/team review packet. The packet lets a reviewer
choose one ninth-slice recipe/repair candidate for later selection, acceptance,
and G Zone/Stride plus Aqua Force battle-order decision work.

This milestone does not record selection, does not record acceptance, does not
record a G Zone decision, does not record a Stride decision, does not record an
Aqua Force battle-order decision, and does not promote runtime decks.

## Inputs

- `outputs/target_slice/m68_closeout_ninth_slice_runtime_readiness.json`
- `outputs/target_slice/m68_06_ninth_slice_blocker_repair_candidates.json`
- `outputs/target_slice/m68_03_ninth_slice_recipe_draft_model.json`

Tests may pass in-memory M68 reports until real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m69_01_ninth_slice_human_repair_review_packet.json`
- `outputs/target_slice/m69_01_ninth_slice_human_repair_review_packet.md`
- `outputs/target_slice/m69_01_ninth_slice_human_repair_review_packet.csv`

## Review Rules

- Include all repair candidates that are ready for human repair review.
- Preserve pair context from the recipe draft.
- Preserve manual substitution previews from M68-06.
- Preserve grade-profile additions/removals from M68-06.
- Preserve G Zone deferred system-work context from M68-06.
- Preserve Stride deferred system-work context from M68-06.
- Preserve Aqua Force battle-order deferred system-work context from M68-06.
- Provide decision options only; do not choose an option automatically.
- Mark `ready_for_m69_02=true` only if `M68-closeout` allows human selection
  review and every review item is ready.

## Runtime Boundary

This milestone must not:

- modify M68 recipe drafts
- record human selection
- record human acceptance
- record G Zone decision
- record Stride decision
- record Aqua Force battle-order decision
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable bot/playbook integration
- mutate `GameState`

## Current Evidence

The current in-memory report exports `25` review items. All `25` are ready for
human repair review and have complete manual repair previews. `23` have complete
grade-profile repair previews, while `2` do not need grade repair. All `25`
preserve G Zone deferred, Stride deferred, and Aqua Force battle-order deferred
decision context. Runtime promotion remains disabled.

## Verification

```powershell
python -m unittest tests.test_ninth_slice_human_repair_review_packet
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M68-closeout, M68-06, and M68-03 outputs exist:

```powershell
python tools\deck\build_ninth_slice_human_repair_review_packet.py
```

## Done Rule

`M69-01` is done when:

- all `25` M68 repair candidates are exported as review items
- manual repair, grade repair, G Zone, Stride, and Aqua Force battle-order
  contexts are preserved
- no human selection, acceptance, or system decision is recorded
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m69_02=true`
