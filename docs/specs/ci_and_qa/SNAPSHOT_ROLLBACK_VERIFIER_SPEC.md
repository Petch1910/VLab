# Snapshot Rollback Verifier Spec

## Status

Implemented in `M11-08`.

## Purpose

Verify that snapshot/rollback and branch simulation isolate hypothetical
actions from the live `GameState`. This is required before advanced battle
search, combo discovery, and future bot planning can safely run look-ahead
simulation.

## Behavior

`SnapshotRollbackVerifier.VerifyBranchIsolation(liveState, branchAction)`:

1. Captures the normalized live-state hash.
2. Captures a `GameStateSnapshot`.
3. Clones a branch from the snapshot.
4. Executes the supplied branch action through `RulesCore` on the branch only.
5. Restores the snapshot into a fresh state.
6. Verifies:
   - live state hash did not change
   - restored hash matches live-before hash
   - branch hash changed

The verifier rejects:

- missing live state
- missing branch action
- rejected branch action
- live state mutation
- restore mismatch
- branch action that does not change the branch

## Boundary

The verifier must not:

- mutate the live state
- execute actions outside `RulesCore`
- treat rejected branch actions as successful rollback verification
- include unsupported pending AUTO queue commits until they become replayable
  state events

## Verification

EditMode coverage verifies:

- branch actions mutate branch only
- illegal branch actions reject without live mutation
- missing input rejection paths
- snapshot restore remains independent after live state mutates later
- result JSON round-trip

## Next Work

`M11-09` strengthens hidden-state player/spectator/bot views against hidden
hand, deck, and source leaks.
