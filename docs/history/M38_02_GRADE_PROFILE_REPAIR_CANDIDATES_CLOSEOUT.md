# M38-02 Grade Profile Repair Candidates Closeout

## Summary

`M38-02` generated source-backed grade-profile repair candidates for accepted
seed recipe `recipe_003`.

The current repaired seed has grade profile `G0=16, G2=6, G3=28`. The target
classic profile is `G0=17, G1=14, G2=11, G3=8`, so the repair candidates add
`20` cards and remove `20` cards as substitution previews.

The candidates are advisory only. No recipe draft was mutated, no human
acceptance was recorded, and runtime promotion remains disabled.

## Results

- Repair candidates: `2`
- Complete candidates: `2`
- Grade deficit total: `20`
- Grade surplus total: `20`
- Runtime promotion allowed: `false`
- Ready for M38-03: `true`

## Candidate Profiles

- `m38_02_grade_pkg_001`: Classic target profile repair
- `m38_02_grade_pkg_002`: EB04-local target profile repair

Both candidates end at:

```text
G0=17, G1=14, G2=11, G3=8
```

## Files

- Spec: `docs/specs/cards_and_decks/GRADE_PROFILE_REPAIR_CANDIDATES_SPEC.md`
- Tool: `tools/deck/build_grade_profile_repair_candidates.py`
- Tests: `tests/test_grade_profile_repair_candidates.py`
- Output: `outputs/target_slice/m38_02_grade_profile_repair_candidates.json`
- Output: `outputs/target_slice/m38_02_grade_profile_repair_candidates.md`

## Verification

```powershell
python tools\deck\build_grade_profile_repair_candidates.py
python -m unittest tests.test_grade_profile_repair_candidates
python -m unittest discover -s tests -p "test_*.py"
```

Final full-suite result after this closeout:

```text
Ran 353 tests
OK
```

Unity was not run because this milestone changed only Python offline tooling,
generated reports, and documentation.

## Next Target

`M38-03`: Human-accepted recipe artifact.

