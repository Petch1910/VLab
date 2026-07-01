# M33-02 Era Combo Preset Expansion Closeout

## Summary

Expanded the offline clan/nation combo pairing tool from the first classic
Vanguard range into the era product ranges supplied by the user:

- Link Joker & Legion Mate
- Vanguard G first arc
- Vanguard G NEXT / G Z
- Vanguard V reboot
- Vanguard Shinemon / if
- Vanguard overDress
- Vanguard will+Dress
- Vanguard Divinez

The reports remain offline advisory artifacts. They do not resolve effects,
mutate `GameState`, consume RNG, or claim full legality.

## Added Tool Support

`tools/combo/discover_clan_combos.py` now supports:

- prefixed range expansion such as `G-TD01-G-TD09`
- `--preset <name>` for named era ranges
- `--list-presets`
- `--group-mode auto|clan|nation`
- `missing_set_codes` in report scope
- auto grouping that uses clan first and nation when clan is `N/A`

## Generated Reports

Generated under:

```text
outputs/combo_discovery/
```

Reports:

- `classic_part1_clan_combos.json`
- `link_joker_legion_mate_clan_combos.json`
- `g_series_first_clan_combos.json`
- `g_next_z_clan_combos.json`
- `v_reboot_clan_combos.json`
- `v_shinemon_if_clan_combos.json`
- `d_overdress_clan_combos.json`
- `d_willdress_clan_combos.json`
- `dz_divinez_clan_combos.json`
- `era_combo_report_summary.json`

## Report Summary

| Preset | Cards | Groups | Candidate pairs | Missing set codes |
| --- | ---: | ---: | ---: | ---: |
| `classic_part1` | 1052 | 21 | 6846 | 0 |
| `link_joker_legion_mate` | 1329 | 23 | 12645 | 0 |
| `g_series_first` | 1328 | 25 | 13010 | 2 |
| `g_next_z` | 1109 | 28 | 8181 | 0 |
| `v_reboot` | 1183 | 26 | 4266 | 0 |
| `v_shinemon_if` | 1055 | 32 | 3320 | 10 |
| `d_overdress` | 848 | 10 | 4958 | 0 |
| `d_willdress` | 808 | 10 | 6457 | 11 |
| `dz_divinez` | 0 | 0 | 0 | 14 |

## Missing Runtime Pack Data

These requested ranges are not fully present in the current runtime SQLite
pack:

- `g_series_first`: `G-SD01`, `G-SD02`
- `v_shinemon_if`: `V-BT13`, `V-BT14`, `V-SS02`, `V-SS03`, `V-SS04`,
  `V-SS05`, `V-SS06`, `V-SS08`, `V-SS09`, `V-SS10`
- `d_willdress`: `D-BT11`, `D-BT12`, `D-BT13`, `D-LBT04`, `D-SS02`,
  `D-SS06`, `D-SS07`, `D-SS08`, `D-SS09`, `D-SS10`, `D-SS11`
- `dz_divinez`: all requested `DZ-SD01-DZ-SD06`, `DZ-BT01-DZ-BT05`,
  and `DZ-SS01-DZ-SS03`

## Verification

Commands run:

```powershell
python -m unittest tests.test_clan_combo_discovery
python tools\combo\discover_clan_combos.py --list-presets
python tools\combo\discover_clan_combos.py --preset classic_part1
python tools\combo\discover_clan_combos.py --preset link_joker_legion_mate
python tools\combo\discover_clan_combos.py --preset g_series_first
python tools\combo\discover_clan_combos.py --preset g_next_z
python tools\combo\discover_clan_combos.py --preset v_reboot
python tools\combo\discover_clan_combos.py --preset v_shinemon_if
python tools\combo\discover_clan_combos.py --preset d_overdress
python tools\combo\discover_clan_combos.py --preset d_willdress
python tools\combo\discover_clan_combos.py --preset dz_divinez
```

Results:

- combo unit tests passed: `7/7`
- all preset report generation completed
- missing set codes are reported explicitly instead of silently dropped

Unity compile/EditMode was not run because this task changed only Python,
generated JSON output, and docs.

## Next Target

```text
M33-03: Review high-confidence clan/nation combo pairs and promote selected
pairs into playbook seed data / structured ability fixture candidates.
```

Do not wire heuristic pairs directly into runtime bot decisions.
