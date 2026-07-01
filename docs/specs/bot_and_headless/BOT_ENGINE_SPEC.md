# Bot Engine Spec

## Source Of Truth

System-wide architecture:

- `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`

Core/bot/simulation guardrails:

- `docs/CORE_DEVELOPMENT_GUARDRAILS.md`

This file focuses on bot-specific behavior.

## Goal

Build a bot that can play against the user without cheating. Start with
heuristics, then move toward probability-aware planning after RulesCore is
hardened.

## Bot Inputs

- masked bot observation
- bot private hand/deck information
- legal action mask from RulesCore
- card feature vectors
- deck profile
- difficulty profile
- optional opponent model from public behavior

## Bot Must Not

- read hidden opponent hand
- read unrevealed top deck
- mutate game state directly
- invent commands outside the legal action mask
- resolve card abilities outside AbilityCore
- use probability as the actual game outcome

## Current Implemented Baseline

- Easy bot can choose simple legal actions.
- Profile bot supports Aggro/Balanced/Defensive profiles.
- Bot actions route through legal action execution and event logs.
- Heuristic bot v2 ranks legal actions with the board/resource evaluator on
  cloned branch states.
- Heuristic bot v2 masks simulated draw cards before scoring so top-deck stats
  do not leak into decisions.
- Guard decision bot recommends no-guard, guard, perfect-guard preference, or
  cannot-guard from visible shield, damage risk, and trigger odds.
- Trigger-risk attack choice uses exact probability as planning input while
  never applying trigger outcomes.
- Battle sequence search v2 adds guard-pressure scoring and cloned advisory
  attackers over the M9 battle search prototype.
- Snapshot simulation path applies legal action sequences on branch snapshots
  only and returns serialized branch state.
- Playbook integration adds deterministic archetype bias over heuristic bot
  evaluations.
- Offline combo discovery output exports ranked advisory combo lines with
  replay references.
- Bot debug trace exports sanitized ranked decision explanations outside
  `GameState`.
- ISMCTS readiness gate checks advanced-search prerequisites before prototypes.
- Advanced search prototype runs a bounded one-ply search over legal actions
  and snapshot simulation after readiness passes.

## Current Priority

Windows-first `M21-M25` is closed and `M26-01` has reopened the bot/automation
track only under correctness-first guardrails. `M26-02` added the canonical
masked-state/legal-action bot decision context. `M26-03` added a player-readable
bot explanation surface. `M26-04` added the structured ability template test
gate. `M26-05` added the live effect no text parsing guard. `M26-06` added the
Home Solo Practice setup flow for difficulty and bot deck selection without
turn automation. `M26-07` added the no-hidden-leak, simulation no-live-mutation,
and replay determinism regression gate. `M26-08` closed the bot/automation
return gate. The next active project target is `M27-01` Windows stability smoke
coverage; future bot work should continue from legal actions, masked views,
the safety regression gate, and structured ability templates only. Do not
expand to true ISMCTS, RL, self-play, or long rollout loops without a new
explicit milestone.

## MVP Heuristics

- Mulligan for grade curve and ride target.
- Ride the best next grade.
- Call attackers/boosters based on simple power lines.
- Attack vanguard by default.
- Attack rear-guards when the trade is clearly valuable.
- Guard based on damage threshold and hand shield value.

## Near-Future Bot Systems

- board/resource evaluator: implemented as a deterministic score/explanation
  helper
- exact trigger probability engine: implemented as a count-based planning helper
- opponent guard and trigger predictor: guard/shield estimator implemented as a
  masked-view heuristic
- heuristic bot v2: implemented as a deterministic legal-action ranker
- guard decision bot: implemented as an advisory shield/damage/trigger-risk
  helper
- trigger-risk attack choice: implemented as a planning-only probability helper
- battle sequence search: implemented as an advisory visible-attacker ranking
  helper
- battle sequence search v2: implemented as guard-pressure and trigger-risk
  scoring wrapper
- snapshot simulation path: implemented as branch-only legal action sequence
  simulation
- playbook integration: implemented as deterministic bias over heuristic bot
  evaluations
- offline combo discovery output: implemented as ranked advisory combo-line
  export
- bot debug trace: implemented as sanitized ranked decision trace
- ISMCTS readiness gate: implemented as advanced-search checklist
- advanced search prototype: implemented as bounded one-ply readiness-gated
  legal-action search
- battle sequence search
- goal planner
- archetype/rideline playbooks
- archetype/rideline playbooks: model and matching library implemented
- offline combo discovery scaffold: implemented as deterministic report output
- explainability log for bot decisions

## Later Research Systems

- Monte Carlo simulation for complex branches
- self-play runner
- RL-compatible observation/action/reward API
- opponent playstyle profiler
- combo discovery and playbook mining

## Architecture Rule

The bot decides which legal command is worth taking. RulesCore decides what is
legal and what happens.
