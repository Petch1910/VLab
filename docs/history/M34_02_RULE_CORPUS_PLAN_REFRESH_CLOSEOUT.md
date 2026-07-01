# M34-02 Rule Corpus Plan Refresh Closeout

## Summary

Imported and reviewed the 2026-06-29 rule/development corpus from:

```text
C:\Users\Phet\Documents\Codex\2026-06-29\new-chat\outputs
```

Copied snapshot:

```text
outputs/research_2026_06_29_new_chat/
```

Copied files: `31`.

## Key Inputs Reviewed

- `vanguard_development/00_DEVELOPMENT_INDEX.md`
- `vanguard_development/README_TH.md`
- `vanguard_development/rule_engine_spec.md`
- `vanguard_development/implementation_checklist.md`
- `vanguard_development/rule_taxonomy.json`
- `vanguard_development/field_layout_by_era.md`
- `vanguard_development/field_blueprints.json`
- `vanguard_development/source_references.md`
- `vanguard_rules_markdown/manifest.json`
- `vanguard_rules_markdown/mechanic_presence_matrix.md`
- `vanguard_effect_conditions_reference.md`
- `vanguard_rules_source_map.md`
- `vanguard_rules_study_notes.md`

## Findings

- The corpus is useful as an official-source-backed reference layer for rule
  taxonomy, deck legality, timing windows, field/zone support, keyword/module
  coverage, and effect-condition decomposition.
- The latest rule source represented in the corpus is Comprehensive Rules
  `4.55`, dated May 22, 2026.
- The existing M33 combo-pair reports and M34 deck-possibility reports are
  still valid as advisory offline analysis, but they are not enough to prove
  a deck is legal or a combo is executable.
- Combo promotion now needs a rule-aware compatibility step: resource,
  timing, zone/target, format, mechanic-module, and missing-data checks.
- `M33-04` should remain deferred until compatibility gates exist.
- UI work stays paused; this refresh affects deck/rule/combo planning only.

## Plan Update

Added:

```text
docs/specs/cards_and_decks/RULE_CORPUS_DECK_COMBO_PLAN_SPEC.md
```

New high-level sequence:

```text
M34-03 Deck-feasible archetype priority v2
M35    Rule corpus / taxonomy integration
M36    Deck construction legality v2
M37    Card semantic feature / requirement model
M38    Combo compatibility / conflict engine
M39    Archetype deck skeleton builder
M40    Rule fixture / structured ability expansion
M41    Playbook / bot integration gate
```

## Verification

This was a docs/data-intake planning task.

- Source corpus copied into project outputs.
- No Unity/runtime code changed.
- No `GameState` mutation path changed.
- No Python data tooling changed.
- Unity tests are not required for this docs-only update.

## Next Target

`M34-03`: Deck-feasible archetype priority v2.

Use:

- `outputs/deck_possibility/deck_possibility_summary.csv`
- `outputs/combo_discovery/combo_matrix_*.csv`
- `outputs/research_2026_06_29_new_chat/vanguard_development/rule_taxonomy.json`
- `outputs/research_2026_06_29_new_chat/vanguard_rules_markdown/mechanic_presence_matrix.md`

Goal: rank which clan/nation/era groups should be reviewed first, using both
deck feasibility and rule/mechanic complexity.
