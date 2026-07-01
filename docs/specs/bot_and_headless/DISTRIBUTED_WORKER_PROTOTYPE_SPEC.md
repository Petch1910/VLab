# Distributed Worker Prototype Spec

## Milestone

`M17-09`

## Goal

Define the worker contract before implementing any worker in `M17-10`. This
milestone is spec-only and validation-only. It must not start a distributed
cluster, Photon worker, RL trainer, or ranked server.

## Prototype Status

```text
spec_only
```

## Input Contract

The future worker may accept only bounded headless inputs:

- `HeadlessBatchSimulationRequest`
- `HeadlessSimulationRequest`
- seed range
- ruleset
- optional deck code

## Output Contract

The future worker may produce:

- `HeadlessBatchSimulationResult`
- `HeadlessDatasetExport`
- `HeadlessPerformanceProfileResult`
- worker status summary

## Artifact Policy

Worker artifacts must be JSON-first and safe to inspect:

- `json_artifacts_only`
- `no_card_instance_ids`
- `no_hidden_state`
- `no_full_replay_by_default`

## Safety Limits

- `max_worker_run_count_50`
- `no_live_network_transport`
- `no_GameState_mutation_outside_headless_runner`
- `no_packed_state_migration`

## Non-Goals

- `no_RL_training`
- `no_distributed_cluster`
- `no_Photon_room_worker`
- `no_live_UI`
- `no_ranked_secure_server`

## Validation

`DistributedWorkerPrototypeSpec.Validate` must reject a spec when:

- schema version is not `1`
- milestone is not `M17-09`
- prototype status is not `spec_only`
- run count exceeds the current headless limit
- hidden-state/no-card-instance policies are missing
- live network or packed-state migration limits are missing
- RL/cluster non-goals are missing

## Handoff To M17-10

`M17-10` may implement only a local bounded worker prototype that follows this
spec. It must not add live Photon workers, cloud orchestration, training loops,
or packed-state migration.
