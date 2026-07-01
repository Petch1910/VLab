# Architecture

## Source Of Truth

Detailed core/bot/research architecture lives in:

- `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`

Implementation guardrails live in:

- `docs/CORE_DEVELOPMENT_GUARDRAILS.md`

This file is the compact map.

## Current System Shape

```text
Card Pack Data
  -> SQLite / Manifest / Image Files
  -> Unity Data Access
  -> Card Browser
  -> Deck Builder
  -> Manual Play Table
  -> Action Log / Replay
  -> Baseline Bot
```

## Target System Shape

```text
Vanguard Core Engine
  -> RulesCore Command/Query API
  -> Legal Action Mask
  -> Hidden State Manager
  -> Phase / Window Controller
  -> AbilityCore
  -> Trigger Resolver
  -> Power / Continuous Effect Tracker
  -> Event Log / Snapshot / Rollback
  -> Headless Simulation API

AI Platform
  -> Card Feature Extractor
  -> Board Evaluator
  -> Exact Probability Engine
  -> Monte Carlo Simulator
  -> Battle Search / Goal Planner
  -> Opponent Profiler
  -> Combo Discovery / Playbooks

Clients And Services
  -> Unity PC UI
  -> Mobile UI
  -> Replay Viewer
  -> Future Online Server
  -> Future Tournament Audit Tools
```

## Core Boundary

The core owns:

- rules
- legal actions
- phase and battle windows
- hidden information masking
- state mutation
- RNG outcomes
- trigger resolution
- ability resolution
- event logs
- replay and rollback data

The core does not own:

- bot strategy
- Monte Carlo search policy
- opponent profiling
- combo mining
- UI animation
- online matchmaking
- AR/card camera recognition

## Boundary Rules

- UI displays state and sends commands; UI does not own rules.
- Bot chooses from legal actions; bot does not mutate state directly.
- Replay uses the same event/reducer path as live gameplay.
- Simulation clones state before hypothetical actions.
- Data import tools produce versioned pack files, not runtime hacks.
- Probability informs planning; it never replaces actual RNG outcomes.
