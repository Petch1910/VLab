# Fifth-Slice Fixture Scaffold Spec

Milestone: `M52-01`

## Purpose

Define the source-backed fixture policy for the fifth slice before any review
packet, recipe draft, or validator work proceeds.

This scaffold is not a full official legality claim. It defines the local
offline validator contract for the selected Gold Paladin Link Joker / Legion
Mate era source slice.

## Inputs

```text
outputs/target_slice/m51_04_fifth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m51_02_fifth_slice_fixture_readiness.json
outputs/target_slice/m51_03_fifth_slice_semantic_compatibility_probe.json
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
set scope: M51-02 format series scope
```

## Mechanic Scope

- Stride runtime: disabled.
- G Zone runtime: disabled.
- Imaginary Gift: disabled.
- Ride Deck: disabled.
- Over Trigger: disabled.
- Front Trigger: disabled.
- Order cards: disabled.
- Legion text: manual-review only until dedicated rules modules exist.
- Lock/Unlock text: manual-review only until dedicated rules modules exist.

## Validator Contract

The later `M52-04` recipe validator must check:

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
- No G Zone or Stride runtime enablement.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m52_01_fifth_slice_fixture_scaffold.json
outputs/target_slice/m52_01_fifth_slice_fixture_scaffold.md
```

## Verification

```powershell
python tools\deck\build_fifth_slice_fixture_scaffold.py
python -m unittest tests.test_fifth_slice_fixture_scaffold
python -m unittest discover -s tests -p "test_*.py"
```
