# Second Slice Selection Spec

Milestone: `M35-E1`

## Purpose

Select the next deck/combo vertical slice after the first selected slice closes
through `M35-D4`.

The output is planning evidence only. It does not create player decks, mutate
runtime card packs, or publish bot/runtime playbook hints.

## Inputs

```text
outputs/archetype_priority/archetype_priority_ranking.json
outputs/target_slice/m35_a2_first_target_slice_report.json
outputs/target_slice/m35_d4_first_slice_reviewed_playbook_seed.json
outputs/deck_possibility/classic_part1_deck_possibility.json
```

## Selection Policy

For `M35-E1`, continue the current Classic Core clan-era scope before broad
format scale-out.

The selector must:

- exclude groups already closed by the first selected slice
- use M34-03 priority ranking as the ordering source
- require the candidate to be feasible
- require the candidate to exist in the matching deck possibility report
- choose the highest-ranked remaining candidate in the selected era preset

## Selected Result

The generated output selects the next Classic Core group after the completed
Nova Grappler first slice:

```text
Classic Core / Oracle Think Tank
M34-03 rank: 10
Priority score: 63.74
```

## Runtime Boundary

- Advisory selection only.
- No player-deck mutation.
- No runtime card pack mutation.
- No bot/runtime playbook publication.
- No claim that the second slice is semantically compatible yet.

## Outputs

```text
outputs/target_slice/m35_e1_second_target_slice_report.json
outputs/target_slice/m35_e1_second_target_slice_report.md
```

## Verification

```powershell
python tools\deck\select_second_target_slice.py
python -m unittest tests.test_second_target_slice_selection
python -m unittest discover -s tests -p "test_*.py"
```
