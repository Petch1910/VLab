# Snapshot Simulation Path Spec

## Scope

`M14-05` adds a pure helper for bot/search code to try legal action sequences on
a cloned branch state. It returns accepted/rejected branch results and serialized
branch state without mutating the live state.

## Inputs

- live `GameState`
- ordered legal-action candidates

## Output

`SnapshotSimulationPathResult` includes:

- accepted/rejected status
- rejection reason
- snapshot id
- requested and accepted action counts
- branch event count
- live-before/live-after hashes
- live unchanged flag
- branch state JSON
- per-action results
- summary

The caller can restore the branch with `RestoreBranchState()`.

## Rules

- Capture `GameStateSnapshot` from the live state.
- Apply each action with `RulesCore.TryExecute` on the cloned branch.
- Stop at the first null or rejected action.
- Never mutate the live state.
- Report if the live state changed unexpectedly.
- Keep branch output separate from `GameState.event_log` on the live state.

## Verification

EditMode tests must cover:

- accepted action mutates branch only
- sequential accepted actions apply in order on the branch
- rejected action reports a rejected branch result without live mutation
- missing inputs reject cleanly
- result JSON round-trip and branch restore

## Non-Goals

- No Monte Carlo loop.
- No packed/flat state.
- No hidden-state policy changes. Callers still choose the visible or true
  state boundary before simulation.
