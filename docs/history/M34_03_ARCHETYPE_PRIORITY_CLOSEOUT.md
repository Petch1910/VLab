# M34-03 Archetype Priority Closeout

## Summary

Implemented Phase A1 of the Hybrid Vertical-Slice Strategy: rank clan/nation
groups by deck feasibility, combo density, and mechanic complexity before
choosing the first deck/combo slice.

## Outputs

Generated:

```text
outputs/archetype_priority/archetype_priority_ranking.csv
outputs/archetype_priority/archetype_priority_ranking.json
```

Source inputs:

```text
outputs/deck_possibility/deck_possibility_summary.csv
outputs/combo_discovery/combo_matrix_group_candidates.csv
outputs/combo_discovery/combo_matrix_synergy_tags.csv
outputs/research_2026_06_29_new_chat/vanguard_rules_markdown/mechanic_presence_matrix.md
```

## Result

- Total groups ranked: `45`
- Deck-feasible groups: `37`
- Top ranked group in the generated report:
  `เบอร์มิวด้า ไทรแองเกิล`

The ranking is advisory. A high rank means “review this slice first,” not “this
deck is complete or combo-legal.”

## Guardrails

- No `GameState` mutation.
- No Unity/runtime change.
- No hidden-state access.
- No live effect parsing.
- No playbook/bot promotion yet.

## Verification

Covered by:

```text
tests/test_archetype_priority.py
```

Full Python test verification should include:

```powershell
python -m unittest discover -s tests -p "test_*.py"
```

## Next Target

`M35-A2`: First target slice selection + format policy + taxonomy gap report.

This should decide whether the first vertical slice is Classic Core,
Standard/DZ, or another format based on M34-03 evidence and user/team priority.
