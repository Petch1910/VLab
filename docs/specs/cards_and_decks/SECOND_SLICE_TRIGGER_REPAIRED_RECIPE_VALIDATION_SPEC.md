# Second-Slice Trigger-Repaired Recipe Validation Spec

Milestone: `M41-repair-validate`

## Purpose

`M41-repair-validate` reruns the second-slice repaired recipe validator after
the accepted trigger/profile repair package is applied.

This milestone decides whether the repaired Oracle Think Tank recipe may move
to the next promotion gate. It does not create a runtime fixture and does not
publish saved deck, UI deck list, bot/playbook, or `GameState` changes.

## Inputs

- `outputs/target_slice/m41_repair_accept_second_slice_trigger_repair_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m41_repair_validate_second_slice_repaired_recipe.json`
- `outputs/target_slice/m41_repair_validate_second_slice_repaired_recipe.md`

## Validation Rules

- Human acceptance must be recorded in the trigger repair acceptance artifact.
- Main deck count must be `50`.
- Classic trigger count must be `16`.
- Heal trigger count must not exceed `4`.
- Grade profile must remain `G0=17/G1=14/G2=11/G3=8`.
- Every card id must exist in runtime SQLite.
- Every quantity must respect `deck_limit`.
- All cards must remain inside the selected Oracle Think Tank clan.
- Manual-review card overlap must be cleared.

## Boundary

This milestone must not:

- mutate the accepted trigger repair artifact
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- create bot/playbook data
- mutate `GameState`
- promote the repaired recipe by itself

Passing validation only opens `M41-04`, the explicit runtime fixture promotion
gate.

## Verification

```powershell
python tools\deck\validate_second_slice_trigger_repaired_recipe.py
python -m unittest tests.test_second_slice_trigger_repaired_recipe_validator
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M41-repair-validate` is done when:

- validation output reports `validator_passed`
- blocking issue count is `0`
- trigger count is `16`
- grade profile remains target-complete
- manual-review overlap is cleared
- docs point the active queue to `M41-04`
