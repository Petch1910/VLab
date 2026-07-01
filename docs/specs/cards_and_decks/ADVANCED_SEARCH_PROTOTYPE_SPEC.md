# Advanced Search Prototype Spec

## Scope

`M14-10` adds a small readiness-gated advanced search prototype. It is bounded,
deterministic, and one-ply only.

This is not full ISMCTS, not RL, and not self-play.

## Inputs

- `GameState`
- player index
- optional `ICardRepository`
- optional `IsmctsReadinessReport`
- optional heuristic and prototype options

## Output

`AdvancedSearchPrototypeResult` includes:

- accepted/rejected status
- readiness flag
- search id
- considered action count
- selected action summary
- selected score
- ranked candidates

Each candidate includes:

- rank
- selected flag
- sanitized action summary
- heuristic score
- snapshot branch accepted flag
- branch event count
- rejection reason

## Search Rules

- Reject if the readiness gate blocks advanced search.
- Score legal actions with `HeuristicBotV2`.
- Sort accepted actions by heuristic score.
- For the bounded top candidates, run `SnapshotSimulationPath.SimulateSingle`
  to verify the branch can execute without mutating live state.
- Select rank 1.

## Safety Rules

- No rollout loop.
- No random simulation.
- No trigger outcome application.
- No live `GameState` mutation.
- Output summaries omit card ids and instance ids.

## Verification

EditMode tests must cover:

- default readiness allows one-ply search
- blocked readiness rejects search
- source `GameState` is unchanged
- opponent private and top-deck ids do not leak
- JSON round-trip
- deterministic output

## Non-Goals

- No true ISMCTS tree policy.
- No playout policy.
- No transposition table.
- No distributed/self-play integration.
