# Third Slice Fixture Scaffold Spec

Milestone: `M44-01`

## Purpose

Define the source-backed fixture policy for the third slice before any review
packet, recipe draft, or validator work proceeds.

This scaffold is not a full official legality claim. It defines the local
offline validator contract for the selected Link Joker/Legion Mate era slice.

## Inputs

```text
outputs/target_slice/m43_04_third_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m43_02_third_slice_fixture_readiness.json
outputs/target_slice/m43_03_third_slice_semantic_compatibility_probe.json
```

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
set scope: TD07-TD17, BT10-BT17, EB06-EB12
source series present: EB06, EB10
```

## Mechanic Scope

- Stride: disabled.
- Imaginary Gift: disabled.
- Ride Deck: disabled.
- Over Trigger: disabled.
- Front Trigger: disabled.
- Order cards: disabled.
- Legion/Lock-like text: manual-review only until a dedicated rules module
  exists.

## Validator Contract

The later `M44-04` recipe validator must check:

- main deck count
- trigger count
- required trigger types
- required grade coverage
- selected identity
- set scope
- runtime deck limit
- missing cards
- manual-review dependencies before any runtime-ready claim

## Runtime Boundary

- Scaffold artifact only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m44_01_third_slice_fixture_scaffold.json
outputs/target_slice/m44_01_third_slice_fixture_scaffold.md
```

## Verification

```powershell
python tools\deck\build_third_slice_fixture_scaffold.py
python -m unittest tests.test_third_slice_fixture_scaffold
python -m unittest discover -s tests -p "test_*.py"
```
