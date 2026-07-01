# M11 RulesCore Completion Closeout

## Status

Closed in `M11-12`.

## Completed Scope

M11 hardened the correctness gates needed before structured ability data and
broader automation expand in `M12`.

Completed capabilities:

- timing/window audit catalog and gap list
- Phase / Timing Matrix scaffold
- RulesCore command facade coverage report
- legal action mask usage report
- reject no-mutation guard snapshots
- event-sourcing coverage report
- replay determinism verifier
- snapshot/rollback verifier
- hidden-state view hardening verifier
- pure resource ledger validation for CB/SB/Energy/once flags
- RuleSet profile catalog for Standard, V-Premium, and Premium feature flags

## Boundaries Still In Force

M11 does not:

- execute arbitrary card text
- enforce every phase/window entry in `RulesCore`
- implement full deck construction validation per format
- implement full cost payment mutation events
- implement stride, G-Guard, persona ride, energy, or gift execution beyond
  existing scaffolds/manual surfaces
- allow UI, bot, network, or ability code to mutate `GameState` directly
- allow hidden opponent hands/deck order to leak into player, spectator, bot, or
  network views

Structured abilities in `M12` must keep using these gates:

- legal actions through `RulesCore`
- no-mutation guarantees on rejection
- `GameEvent`/replay-backed state changes
- snapshot isolation for simulation
- hidden-state views for non-owner consumers
- resource ledger validation before cost-payment commits
- RuleSet profile flags instead of raw format string hard-coding

## Verification

Closeout verification:

- `python tools\verification\verify_vanguard_th_pack.py` passed:
  `10836/10836` cards, `10836/10836` images
- `python tools\data\validate_custom_pack_schema.py data\templates\custom_pack`
  passed with expected fallback-image warnings
- `python tools\data\import_custom_pack.py data\templates\custom_pack --output-dir work\custom_pack_import --overwrite`
  passed
- `python -m unittest discover -s tests -p "test_*.py"` passed: `13/13`
- Unity compile passed with no compiler errors:
  `client/unity/VanguardThaiSim/work/unity_compile_m11_11.log`
- Unity EditMode passed:
  `client/unity/VanguardThaiSim/work/unity_editmode_results_m11_11.xml`
  reported `607/607`

## Next Target

Move to `M12-01` Ability schema v1 as the first task of Structured Card Ability
Data.
