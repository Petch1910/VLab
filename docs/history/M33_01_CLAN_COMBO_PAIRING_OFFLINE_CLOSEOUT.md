# M33-01 Clan Combo Pairing Offline Closeout

## Summary

Implemented the first offline clan combo pairing algorithm for the requested
early Vanguard card pool:

- Trial Deck: `TD01` through `TD06`
- Booster Pack: `BT01` through `BT09`
- Extra Booster: `EB01` through `EB05`

The algorithm groups cards by clan, extracts rough semantic features from the
runtime card pack, scores explainable pair synergies, and exports a deterministic
JSON report for human review and future playbook work.

## Scope Boundary

This is an offline advisory data-mining tool only.

It does not:

- mutate `GameState`
- run inside live matches
- consume RNG
- inspect hidden state
- resolve arbitrary card text at runtime
- claim complete timing, cost, target, or board legality

## Added

- `tools/combo/discover_clan_combos.py`
- `tools/combo/__init__.py`
- `tests/test_clan_combo_discovery.py`
- `docs/specs/bot_and_headless/CLAN_COMBO_PAIRING_OFFLINE_SPEC.md`
- `outputs/combo_discovery/td01_td06_bt01_bt09_eb01_eb05_clan_combos.json`

## Algorithm Notes

The first pass extracts and scores these synergy signals:

- named card references
- clan references
- grade targets
- search/deck reveal targets
- superior call targets
- power/critical pressure
- boost and restand attack pressure
- soul and Counter Blast resource support
- draw/card-advantage support
- bounce/reuse on-call support
- retire/drop-zone value

Default `min_score` is `60` to keep the generated report more reviewable than
the broad raw candidate pool.

## Generated Report

Output:

```text
outputs/combo_discovery/td01_td06_bt01_bt09_eb01_eb05_clan_combos.json
```

Report summary:

- `schema_version`: `1`
- `card_count`: `1052`
- `clan_count`: `21`
- `scope.min_score`: `60`
- total candidate pairs meeting threshold: `6846`
- clans with at least one candidate pair: `20`

Largest candidate pools:

- `โนว่า เกรปเปอร์`: `112` cards, `1326` candidate pairs
- `โอราเคิล ทิงค์ แทงค์`: `103` cards, `875` candidate pairs
- `โกลด์ พาลาดิน`: `60` cards, `847` candidate pairs
- `เกรท เนเจอร์`: `56` cards, `568` candidate pairs
- `รอยัล พาลาดิน`: `62` cards, `441` candidate pairs

## Verification

Commands run:

```powershell
python tools\combo\discover_clan_combos.py --sets TD01-TD06 BT01-BT09 EB01-EB05 --top-per-clan 20 --output outputs\combo_discovery\td01_td06_bt01_bt09_eb01_eb05_clan_combos.json
python -m unittest discover -s tests -p "test_*.py"
```

Results:

- report generation passed with `cards=1052`, `clans=21`
- Python unit tests passed: `59/59`
- deterministic regeneration check passed:
  `a8983806cdf5d27868720e778dbfc4dd923653b9c2cec546dc0f8b63feec8209`

Unity compile/EditMode was not run because this task changed only Python,
generated JSON output, and docs.

## Next Target

Recommended next slice:

```text
M33-02: Review high-confidence clan combo pairs and promote selected pairs into
playbook seed data / structured ability fixture candidates.
```

Do not wire these pairs directly into runtime bot decisions until reviewed and
backed by structured ability data or rule fixtures.
