# Second-Slice Trigger/Profile Repair Candidates Spec

Milestone: `M41-repair`

## Purpose

`M41-repair` creates source-backed trigger/profile repair candidates after
`M41-03` finds that the accepted Oracle Think Tank repair has only `2/16`
triggers.

The candidates are advisory. They do not mutate the accepted artifact, record
acceptance, create runtime fixtures, publish saved decks, or enable bot/playbook
use.

## Inputs

- `outputs/target_slice/m41_02_second_slice_human_accepted_repair_artifact.json`
- `outputs/target_slice/m41_03_second_slice_repaired_recipe_validation_rerun.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.json`
- `outputs/target_slice/m41_repair_second_slice_trigger_profile_candidates.md`

## Repair Rules

Candidates must:

- resolve `trigger_count_mismatch`
- bring trigger count to `16`
- keep grade counts at `G0=17/G1=14/G2=11/G3=8`
- keep main deck count at `50`
- respect card copy limits
- use source-backed cards from SQLite
- remove only available non-trigger grade 0 cards from the accepted repaired
  recipe

## Runtime Boundary

This milestone must not:

- mutate M41-02 accepted artifact
- record human acceptance
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_second_slice_trigger_profile_repair_candidates.py
python -m unittest tests.test_second_slice_trigger_profile_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M41-repair` is done when:

- at least one complete repair candidate is generated
- generated candidates restore trigger count to `16`
- generated candidates preserve target grade counts
- output remains advisory/non-runtime
- project status docs point the active queue to repair acceptance
