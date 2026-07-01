# M41 Repair Second-Slice Trigger/Profile Candidates Closeout

## Result

`M41-repair` creates advisory repair candidates for the trigger blocker found
in `M41-03`.

Generated artifacts:

- `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.json`
- `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.md`

## Repair Summary

The report contains:

- `3` candidate packages
- `3` complete candidates
- `3` candidates ready for human review
- `0` runtime-promotion candidates

The balanced package `m41_repair_pkg_001` restores:

```text
Trigger: Critical=4, Draw=4, Heal=4, Stand=4
Grade: G0=17, G1=14, G2=11, G3=8
Main deck: 50
```

`ready_for_repair_acceptance=true`.

## Boundary

Still blocked:

- repair acceptance recording
- M41-02 accepted artifact mutation
- runtime fixture creation
- saved-deck injection
- UI deck-list publication
- bot/playbook promotion
- direct `GameState` mutation

## Verification

```powershell
python tools\deck\build_second_slice_trigger_profile_repair_candidates.py
python -m unittest tests.test_second_slice_trigger_profile_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

Results:

- Targeted generator passed with `ready_for_repair_acceptance=True`,
  `candidates=3`, and `complete=3`.
- Targeted tests passed: `6/6`.
- Full Python unittest discovery passed: `469/469`.

## Next Target

`M41-repair-accept`: Second-slice trigger repair acceptance artifact.
