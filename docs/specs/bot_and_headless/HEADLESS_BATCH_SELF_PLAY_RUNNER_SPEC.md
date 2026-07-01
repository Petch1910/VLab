# Headless Batch Self-Play Runner Spec

## Milestone

`M17-04`

## Goal

Run multiple bounded headless simulations from local deterministic inputs and
produce a compact summary. This prepares for later dataset export without
starting RL, advanced self-play training, or distributed workers.

## Scope

- Batch request:
  - `start_seed`
  - `run_count`
  - `ruleset`
  - optional `deck_code`
- Batch result:
  - accepted/rejected status
  - accepted count
  - blocked count
  - per-run minimal `HeadlessSimulationResult` entries
- CLI command:
  `VanguardThaiSim.EditorTools.HeadlessBatchSimulationCliRunner.RunFromCommandLine`

## CLI Arguments

- `-headlessBatchCount <int>`
- `-headlessStartSeed <int>`
- `-headlessRuleset <D|Standard|V|V-Premium|P|Premium>`
- `-headlessDeckCode <VGTH1...>`
- `-headlessBatchResultPath <path>`

## Guardrails

- `run_count` must be between `1` and `50`.
- No per-run replay files are written in M17-04.
- No observation/action/reward API yet.
- No RL training loop.
- No distributed worker.

## Verification

- Default batch runs three accepted simulations with sequential seeds.
- Custom batch count/start seed/ruleset is applied.
- Out-of-range count rejects before simulation.
- CLI parser validates count/seed/ruleset/deck-code/result-path inputs.
- Unity compile and full EditMode suite pass.
