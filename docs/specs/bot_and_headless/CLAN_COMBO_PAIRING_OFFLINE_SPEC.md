# Clan Combo Pairing Offline Spec

## Status

`M33-01` implements the first offline clan combo pairing pass for the early
Vanguard card pool requested by the user. `M33-02` extends the same offline
algorithm to named era presets across Link Joker/Legion, G, V, D, and DZ
product ranges.
`M33-03` builds matrix artifacts from those reports so the candidate density can
be inspected by era, clan/nation group, top score, and synergy tag.

After `M34-02`, heuristic pair promotion is deferred until the project has
deck-legality and combo-compatibility gates. Use
`docs/specs/cards_and_decks/RULE_CORPUS_DECK_COMBO_PLAN_SPEC.md` for the
current M34-M41 sequence.

## Scope

The first report analyzes these product ranges:

- Trial Deck: `TD01` through `TD06`
- Booster Pack: `BT01` through `BT09`
- Extra Booster: `EB01` through `EB05`

Cards are grouped by `clan` from the runtime SQLite card pack, then scored as
advisory pair candidates inside each clan.

The era preset extension adds these preset names:

| Preset | Product ranges |
| --- | --- |
| `classic_part1` | `TD01-TD06`, `BT01-BT09`, `EB01-EB05` |
| `link_joker_legion_mate` | `TD07-TD17`, `BT10-BT17`, `EB06-EB12` |
| `g_series_first` | `G-TD01-G-TD09`, `G-SD01-G-SD02`, `G-BT01-G-BT08`, `G-CB01-G-CB04`, `G-TCB01-G-TCB02` |
| `g_next_z` | `G-TD10-G-TD15`, `G-BT09-G-BT14`, `G-CB05-G-CB07`, `G-CHB01-G-CHB03` |
| `v_reboot` | `V-TD01-V-TD09`, `V-BT01-V-BT06`, `V-EB01-V-EB09` |
| `v_shinemon_if` | `V-TD10-V-TD12`, `V-BT07-V-BT14`, `V-EB10-V-EB15`, `V-SS01-V-SS10` |
| `d_overdress` | `D-SD01-D-SD05`, `D-BT01-D-BT05`, `D-LBT01-D-LBT02` |
| `d_willdress` | `D-SD06`, `D-TD01-D-TD03`, `D-BT06-D-BT13`, `D-LBT03-D-LBT04`, `D-SS01-D-SS11` |
| `dz_divinez` | `DZ-SD01-DZ-SD06`, `DZ-BT01-DZ-BT05`, `DZ-SS01-DZ-SS03` |

Group mode defaults to `auto`:

- cards with a real `clan` are grouped by clan
- cards whose `clan` is `N/A` fall back to `nation`
- this keeps D/DZ nation-era reports usable without changing older clan-era
  behavior

## Source Of Truth

Input data comes from:

```text
data/packs/vanguard_th/cards.sqlite
```

Required columns:

- `card_id`
- `name_th`
- `text_th`
- `series_code`
- `clan`
- `grade`
- `power`
- `shield`
- `trigger`
- `type_1`
- `type_2`

The tool does not read or copy comparator app data, game files, icon assets, or
external card databases.

## Runtime Boundary

This is an offline data-mining helper.

Allowed:

- Parse Thai/English card text from our local runtime pack.
- Extract rough semantic tags such as cost, draw, search, superior call, power
  bonus, stand, retire, bounce, soul resource, and Counter Blast support.
- Produce ranked advisory combo pairs for future playbooks and human review.

Forbidden:

- No live match effect resolution.
- No direct `GameState` mutation.
- No RNG consumption.
- No legality claims beyond heuristic pair scoring.
- No hidden-state inspection.
- No replacement for structured ability definitions.

## Algorithm

1. Expand requested set ranges into concrete set codes.
2. Load matching cards from SQLite ordered by clan, set, and card id.
3. Extract `CardFeatures` for each card:
   - ability tags
   - clan references from `<<...>>`
   - named references from quoted card names
   - grade references
   - power bonus values
4. For each clan, score every unique unordered pair.
5. Score both directions because support can flow from either card.
6. Keep pairs whose score is at least `min_score`.
7. Sort by score descending, then pair id.
8. Export the top `top_per_clan` pairs per clan.

## Scoring Signals

The first pass recognizes these advisory synergies:

- `named_reference`
- `clan_reference`
- `grade_target`
- `search_target`
- `call_target`
- `power_pressure`
- `boost_attack`
- `restand_attack`
- `soul_resource`
- `counter_blast_resource`
- `card_advantage_support`
- `reuse_on_call`
- `retire_for_drop_value`
- `hit_pressure_support`

Scores are intentionally explainable rather than perfect. A high score means
"review this pair first", not "this combo is fully legal and optimal".

## Output

Default output path:

```text
outputs/combo_discovery/td01_td06_bt01_bt09_eb01_eb05_clan_combos.json
```

Top-level fields:

- `schema_version`
- `generator`
- `scope`
- `card_count`
- `clan_count`
- `clans`
- `group_count`

Each clan record includes:

- `clan`
- `group`
- `group_field`
- `card_count`
- `set_codes`
- `combo_pair_count`
- `top_pairs`

Each pair includes:

- `pair_id`
- `score`
- `card_a`
- `card_b`
- `synergy_tags`
- `shared_tags`
- `reasons`

## Matrix Outputs

Matrix artifacts are generated from existing combo reports:

```powershell
python tools\combo\build_combo_matrices.py
```

Default output directory:

```text
outputs/combo_discovery/
```

Matrix files:

- `combo_matrix_summary.json`
- `combo_matrix_era_summary.csv`
- `combo_matrix_group_candidates.csv`
- `combo_matrix_group_cards.csv`
- `combo_matrix_top_pair_scores.csv`
- `combo_matrix_synergy_tags.csv`

Matrix meaning:

- `combo_matrix_era_summary.csv`: one row per era preset with card count,
  group count, candidate-pair count, and missing set count.
- `combo_matrix_group_candidates.csv`: group x era matrix where each value is
  thresholded candidate-pair count.
- `combo_matrix_group_cards.csv`: group x era matrix where each value is card
  count.
- `combo_matrix_top_pair_scores.csv`: group x era matrix where each value is
  the highest top-pair score in that group.
- `combo_matrix_synergy_tags.csv`: one row per preset/group with counts of
  synergy tags among stored `top_pairs`.

The matrix is intentionally summary-level. Full card x card matrices are not
written by default because they are large, noisy, and harder to review than
ranked pair lists plus group/tag matrices.

## CLI

```powershell
python tools\combo\discover_clan_combos.py `
  --sets TD01-TD06 BT01-BT09 EB01-EB05 `
  --top-per-clan 20 `
  --min-score 60 `
  --output outputs\combo_discovery\td01_td06_bt01_bt09_eb01_eb05_clan_combos.json
```

Preset example:

```powershell
python tools\combo\discover_clan_combos.py --preset link_joker_legion_mate
```

List supported presets:

```powershell
python tools\combo\discover_clan_combos.py --list-presets
```

## Verification

Python unit tests must cover:

- set-range expansion
- feature extraction for common combo text
- clan-isolated pair ranking
- named-reference pair scoring
- resource-support pair scoring

Report verification must confirm:

- `card_count` matches the selected runtime SQLite pool
- `clan_count` matches the selected runtime SQLite pool
- `missing_set_codes` records requested sets that are not present in the
  runtime SQLite pack
- output is deterministic for the same input and options
- matrix generation produces CSV/JSON artifacts from existing preset reports

## Known Limitations

- Thai text parsing is heuristic and can miss unusual wording.
- The tool does not understand complete timing windows yet.
- The tool does not validate cost availability.
- The tool does not validate board position legality.
- The tool does not create deck-level combo lines yet.
- Later playbook work must review and promote only high-confidence pairs.
- After the 2026-06-29 rule corpus refresh, later playbook work must also pass
  format legality, timing, resource, zone/target, and mechanic-module
  compatibility checks before promotion.
