# Ninth-Slice Fixture Scaffold Spec

Milestone: `M68-01`

## Purpose

Define the source-backed fixture policy for the ninth slice before any review
packet, recipe draft, or validator work proceeds.

This scaffold is not a full official legality claim. It defines the local
offline validator contract for the selected Aqua Force G-series source slice.

## Inputs

```text
outputs/target_slice/m67_04_ninth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m67_02_ninth_slice_fixture_readiness.json
outputs/target_slice/m67_03_ninth_slice_semantic_compatibility_probe.json
```

Tests may pass in-memory M67-02, M67-03, and M67-04 reports. Real CLI artifacts
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
set scope: M67-02 format series scope
G Zone boundary: required, review-only
```

## Mechanic Scope

- Runtime G Zone support: disabled for this scaffold.
- Runtime Stride support: disabled for this scaffold.
- Grade 4/G-unit cards: advisory/manual-review only until dedicated G Zone
  and Stride support exists.
- Generation Break, G Guardian, heart-card, and stride-cost text: manual-review
  only until dedicated rules modules exist.
- Aqua Force battle-count / attack-order text: manual-review only until
  battle-order and timing modules can validate it.
- Imaginary Gift: disabled.
- Ride Deck: disabled.
- Over Trigger: disabled.
- Front Trigger: disabled.
- Order cards: disabled.
- Bloom/token runtime: disabled.
- Lock/Unlock and Legion/Mate runtime: disabled.

## Validator Contract

The later `M68-04` recipe validator must check:

- main deck count
- trigger count
- required trigger types
- required grade coverage
- selected identity
- set scope
- runtime deck limit
- missing cards
- Grade 4/G-unit cards stay out of main-deck recipes until dedicated format
  support exists
- G Zone cards remain in a review boundary and cannot imply runtime readiness
- manual-review dependencies before any runtime-ready claim

## Runtime Boundary

- Scaffold artifact only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No G Zone, Stride, Bloom/token, Lock/Unlock, or Legion/Mate runtime
  enablement.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m68_01_ninth_slice_fixture_scaffold.json
outputs/target_slice/m68_01_ninth_slice_fixture_scaffold.md
```

## Verification

```powershell
python -m unittest tests.test_ninth_slice_fixture_scaffold
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M67-02, M67-03, and M67-04 outputs exist:

```powershell
python tools\deck\build_ninth_slice_fixture_scaffold.py
```
