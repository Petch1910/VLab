# Distributed Worker Local Prototype Spec

## Milestone

`M17-10`

## Goal

Implement a local bounded worker prototype that follows the `M17-09`
distributed worker spec. The worker runs inside the current Unity process and
returns safe in-memory artifacts. It is not a cluster, Photon worker, ranked
server, or RL trainer.

## API

`DistributedWorkerPrototype.Run(request, spec = null)`

- validates the worker spec before running
- validates run count against the spec limit
- runs `HeadlessBatchSimulationRunner`
- optionally exports `HeadlessDatasetExport`
- optionally runs `HeadlessPerformanceProfiler`
- returns a single JSON-friendly result object

## Request

`DistributedWorkerPrototypeRequest`

- `worker_id`
- `batch_request`
- `export_dataset`
- `include_profile`

## Result

`DistributedWorkerPrototypeResult`

- `schema_version`
- `accepted`
- `rejection_reason`
- `worker_id`
- `worker_policy`
- `spec_validation`
- `batch_result`
- `dataset_export`
- `performance_profile`

## Worker Policy

```text
local_bounded_no_network_no_cluster_no_RL
```

## Safety Rules

- Reject invalid worker specs before running.
- Reject run counts above the spec limit before running.
- Do not use Photon or any live network transport.
- Do not start external processes.
- Do not start RL training.
- Do not export hidden state, card ids, or card instance ids through dataset
  artifacts.
- Do not migrate packed state.

## Verification

- Worker runs a bounded batch and dataset export.
- Worker rejects invalid spec before running.
- Worker rejects run count outside spec limit before running.
- Optional profile returns timing summary without changing simulation behavior.
