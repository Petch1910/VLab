# M50-03 Fourth Headless Fixture Load Smoke Closeout

## Summary

`M50-03` verifies that the fourth offline runtime/test fixture can be consumed
through deck-code headless loading without promoting it into saved decks, UI
deck lists, bot playbooks, G Zone runtime, or Stride runtime.

## Outputs

- `tools/deck/build_fourth_headless_fixture_load_smoke.py`
- `tests/test_fourth_headless_fixture_load_smoke.py`
- `docs/specs/cards_and_decks/FOURTH_HEADLESS_FIXTURE_LOAD_SMOKE_SPEC.md`
- `outputs/target_slice/m50_03_fourth_fixture_deck_code.txt`
- `outputs/target_slice/m50_03_fourth_fixture_load_smoke.json`
- `outputs/target_slice/m50_03_fourth_fixture_load_smoke.md`
- `outputs/target_slice/m50_03_fourth_fixture_unity_result.json`
- `outputs/target_slice/m50_03_fourth_fixture_unity_replay.json`

## Result

- Offline load ready: `true`
- Deck code created: `true`
- Unity headless result provided: `true`
- Unity headless smoke passed: `true`
- Blocking issues: `0`
- Main deck count: `50`
- Unique cards: `14`
- G Zone count: `0`
- Unity deck source: `deck_code`
- Unity actions/events: `4/4`
- Ready for `M50-04`: `true`

## Boundary

- No saved deck mutation.
- No UI deck library mutation.
- No bot playbook enablement.
- No G Zone or Stride runtime enablement.
- Python smoke does not mutate `GameState`.

## Verification

```powershell
python tools\deck\build_fourth_headless_fixture_load_smoke.py
python tools\deck\build_fourth_headless_fixture_load_smoke.py --unity-result outputs\target_slice\m50_03_fourth_fixture_unity_result.json --unity-replay outputs\target_slice\m50_03_fourth_fixture_unity_replay.json
python -m unittest tests.test_fourth_headless_fixture_load_smoke
```

Result:

- Generator passed before and after Unity evidence.
- Unity headless CLI accepted the generated deck code.
- Targeted Python tests passed `9/9`.
- Full Python unittest discovery passed `870/870` after the M50 queue.

## Next

`M50-04`: Four-fixture scale decision.
