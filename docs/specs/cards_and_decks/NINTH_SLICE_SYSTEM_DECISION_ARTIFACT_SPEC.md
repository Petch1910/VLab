# Ninth-Slice G Zone / Stride / Aqua Force Decision Artifact Spec

Milestone: `M69-04`

## Purpose

`M69-04` records the explicit system-boundary decision for the repaired
ninth-slice recipe accepted in `M69-03`.

The ninth slice is Aqua Force / G-era flavored, so the selected recipe can
carry three deferred system areas:

- G Zone deck slots and G-unit visibility
- Stride declaration, cost, heart-card, return, and Generation Break support
- Aqua Force battle-order semantics such as battle-count tracking,
  attack-order predicates, and multi-attack labels

This milestone records how those deferred areas are handled for the next
validation rerun. It does not implement any of those runtimes.

## Inputs

- `outputs/target_slice/m69_03_ninth_slice_human_accepted_repair_artifact.json`
- CLI/user decision option for G Zone
- CLI/user decision option for Stride
- CLI/user decision option for Aqua Force battle-order

Tests may pass in-memory M69-03 evidence until real upstream artifacts exist.

## Outputs

- `outputs/target_slice/m69_04_ninth_slice_system_decision_artifact.json`
- `outputs/target_slice/m69_04_ninth_slice_system_decision_artifact.md`

## Supported Decision Options

### G Zone

- `main_deck_only_review_no_runtime_promotion`
- `defer_until_g_zone_runtime_exists`

### Stride

- `main_deck_only_review_no_runtime_promotion`
- `defer_until_stride_runtime_exists`

### Aqua Force Battle Order

- `manual_semantic_review_only_no_runtime_promotion`
- `defer_until_aqua_force_battle_order_runtime_exists`

## Decision Rules

- The selected options must be present in the M69-03 accepted artifact.
- Unsupported option ids are rejected.
- `ready_for_m69_05=true` only when:
  - M69-03 is ready for M69-04
  - human selection is recorded
  - human repair acceptance is recorded
  - repair is accepted
  - G Zone, Stride, and Aqua Force deferred contexts are present
  - repair application issue count is `0`
  - all three selected decisions allow main-deck-only validation
  - runtime promotion is still disabled
- If any selected decision chooses a defer-until-runtime option, the artifact
  stays advisory and routes to `M69-closeout`.

## Validation Rerun Policy

When `ready_for_m69_05=true`, M69-05 may rerun validation in
`main_deck_only` scope and suppress only these deferred review issue codes:

- `g_zone_support_deferred`
- `stride_support_deferred`
- `aqua_force_battle_order_support_deferred`

M69-05 must still enforce normal deck issues such as:

- `main_deck_count`
- `missing_card`
- `copy_limit`
- `trigger_count`
- `heal_trigger_limit`
- `grade_profile_outside_target`
- `manual_review_overlap`
- `clan_or_format_mismatch`
- `human_selection_missing`
- `human_acceptance_missing`

## Runtime Boundary

This milestone must not:

- record human selection
- record human repair acceptance
- declare the repaired recipe valid
- modify M68/M69 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- allow Grade 4 or G-units in the main deck
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_ninth_slice_system_decision_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M69-03 output exists:

```powershell
python tools\deck\build_ninth_slice_system_decision_artifact.py `
  --g-zone-option main_deck_only_review_no_runtime_promotion `
  --stride-option main_deck_only_review_no_runtime_promotion `
  --aqua-force-option manual_semantic_review_only_no_runtime_promotion
```

## Done Rule

`M69-04` is done when:

- G Zone, Stride, and Aqua Force decisions are recorded explicitly
- deferred/runtime option ids are validated against M69-03 evidence
- ready decisions open only M69-05 validation rerun, not runtime promotion
- defer decisions keep the recipe advisory
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- targeted and full Python tests pass
- roadmap/current-status docs point to `M69-05`
