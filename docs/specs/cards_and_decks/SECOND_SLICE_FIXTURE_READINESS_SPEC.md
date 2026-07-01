# Second Slice Fixture Readiness Spec

Milestone: `M35-E2`

## Purpose

Check whether the second selected slice can reuse the first-slice Classic Core
fixture policy before semantic/compatibility scale-out.

This is an offline fixture-readiness report. It does not create player decks,
mutate runtime card packs, or publish bot/runtime playbook hints.

## Inputs

```text
outputs/target_slice/m35_e1_second_target_slice_report.json
data/packs/vanguard_th/cards.sqlite
```

## Policy

The readiness tool reuses the narrow `M35-A3` deck fixture builder against the
`M35-E1` selected target.

The check passes only when:

- a 50-card selected-group fixture can be built
- the fixture contains exactly 16 triggers
- non-trigger setup cards for grades 0, 1, 2, and 3 are present
- all cards stay in the selected group and selected set scope
- runtime SQLite `deck_limit` is respected
- negative fixtures reject for the expected reason prefixes

## Selected Result

```text
Classic Core / Oracle Think Tank
Classic Core policy reusable: true
New format/mechanic fixtures required: false
```

## Runtime Boundary

- Offline fixture readiness only.
- No player-deck mutation.
- No runtime card pack mutation.
- No bot/runtime playbook publication.
- No semantic compatibility claim beyond fixture readiness.

## Outputs

```text
outputs/target_slice/m35_e2_second_slice_fixture_readiness.json
outputs/target_slice/m35_e2_second_slice_fixture_readiness.md
```

## Verification

```powershell
python tools\deck\build_second_slice_fixture_readiness.py
python -m unittest tests.test_second_slice_fixture_readiness
python -m unittest discover -s tests -p "test_*.py"
```
