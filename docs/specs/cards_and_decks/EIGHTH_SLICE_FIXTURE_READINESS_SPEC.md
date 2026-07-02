# Eighth-Slice Fixture / Format Readiness Spec

Milestone: `M63-02`

## Purpose

`M63-02` verifies whether the `M63-01` selected target has enough source-backed
runtime card data to enter an eighth-slice semantic/compatibility probe.

The milestone is an offline readiness check only. It does not create a deck,
runtime fixture, saved deck entry, UI entry, bot playbook, G Zone runtime,
Stride runtime, Bloom/token runtime, or `GameState` mutation.

## Inputs

- `outputs/target_slice/m63_01_eighth_target_slice_selection.json`
- `data/packs/vanguard_th/cards.sqlite`
- Era set presets from `tools/combo/discover_clan_combos.py`

Tests may pass an in-memory `M63-01` selection report built from an in-memory
`M62-04` scale decision. Real CLI artifacts remain gated until the real
`M63-01` output exists.

## Checks

The readiness report must verify:

- the `M63-01` selection is ready for `M63-02`
- the selected era preset has a known set scope
- the selected clan/nation has cards in the runtime SQLite pack for that scope
- grades 0, 1, 2, and 3 are present
- G-series scopes have at least one Grade 4/G-unit source card, but G Zone and
  Stride runtime remain disabled
- trigger capacity can support a 16-trigger main deck
- the selected era has required trigger families available
- total main-deck capacity can support a 50-card fixture candidate

If any check fails, the report must route to `M63-repair` and list repair
reasons.

## Trigger Requirements

- `classic_part1`, `link_joker_legion_mate`, `g_series_first`, and `g_next_z`:
  Critical, Draw, Heal, and Stand.
- `v_*`: Critical, Draw, Heal, and either Front or Stand.
- `d_*` and `dz_*`: Critical, Draw, Front, Heal, and Over or Over Trigger.

These checks are data-readiness checks only; they are not official legality
rules.

## Non-Mutation Boundaries

`M63-02` must keep all of these disabled:

- deck creation
- runtime fixture creation
- runtime pack mutation
- saved deck injection
- UI deck publication
- bot/playbook promotion
- G Zone runtime
- Stride runtime
- Bloom/token runtime
- `GameState` mutation

## Verification

Targeted tests:

```powershell
python -m unittest tests.test_eighth_slice_fixture_readiness
```

Full Python regression:

```powershell
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after `M63-01` output exists:

```powershell
python tools\deck\build_eighth_slice_fixture_readiness.py
```

## Done

`M63-02` scaffold work is ready when tests prove that the in-memory eighth
selection can be checked against SQLite, readiness routes to `M63-03` only when
all source/grade/trigger/capacity checks pass, broken selections route to
`M63-repair`, output round-trips as JSON/Markdown, and all runtime/UI/bot/G
Zone/Stride/Bloom-token/GameState boundaries remain disabled.
