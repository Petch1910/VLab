# Eighth-Slice Fixture Scaffold Spec

Milestone: `M64-01`

## Purpose

Define the source-backed fixture policy for the eighth slice before any review
packet, recipe draft, or validator work proceeds.

This scaffold is not a full official legality claim. It defines the local
offline validator contract for the selected Kagero Link Joker / Legion Mate
source slice.

## Inputs

```text
outputs/target_slice/m63_04_eighth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m63_02_eighth_slice_fixture_readiness.json
outputs/target_slice/m63_03_eighth_slice_semantic_compatibility_probe.json
```

Tests may pass in-memory M63-02, M63-03, and M63-04 reports. Real CLI artifacts
remain gated until the real upstream files exist.

## Scaffold Policy

```text
main deck exact: 50
trigger target: 16
required trigger types: Critical, Draw, Heal, Stand
recommended trigger profile: 4 / 4 / 4 / 4
required setup grades: 0, 1, 2, 3
preferred grade profile: G0=17, G1=14, G2=11, G3=8
identity field: clan
copy limit source: runtime SQLite cards.deck_limit
set scope: M63-02 format series scope
```

## Mechanic Scope

- Runtime Lock support: disabled for this scaffold.
- Runtime Legion support: disabled for this scaffold.
- Legion pair validation: deferred.
- Grade 4 cards: advisory/manual-review only until dedicated format support
  exists.
- Imaginary Gift: disabled.
- Ride Deck: disabled.
- G Zone: disabled.
- Stride: disabled.
- Over Trigger: disabled.
- Front Trigger: disabled.
- Order cards: disabled.
- Bloom/token runtime: disabled.
- Lock/Unlock, Legion/Mate, and generation-adjacent Grade 4 text:
  manual-review only until dedicated rules modules exist.

## Validator Contract

The later `M64-04` recipe validator must check:

- main deck count
- trigger count
- required trigger types
- required grade coverage
- selected identity
- set scope
- runtime deck limit
- missing cards
- Grade 4 cards stay out of main-deck recipes until dedicated format support
- manual-review dependencies before any runtime-ready claim

## Runtime Boundary

- Scaffold artifact only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No G Zone, Stride, Lock, Legion, or Bloom/token runtime enablement.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m64_01_eighth_slice_fixture_scaffold.json
outputs/target_slice/m64_01_eighth_slice_fixture_scaffold.md
```

## Verification

```powershell
python -m unittest tests.test_eighth_slice_fixture_scaffold
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M63-02, M63-03, and M63-04 outputs exist:

```powershell
python tools\deck\build_eighth_slice_fixture_scaffold.py
```
