# Vanguard AI Engine Knowledge Summary

## Purpose

This document is the consolidated knowledge base for the Vanguard Thai
Simulator core, bot, simulation, and future research-grade AI system.

It merges lessons from:

- local safe static inspection of Vanguard Dear Days
- local safe static inspection of Vanguard Dear Days 2
- local safe static inspection of Yu-Gi-Oh! Master Duel
- user-proposed AI architecture ideas
- current project implementation status

This document is intended to keep future AI assistants and developers aligned.
It is not a source for copying proprietary code, assets, card data, or game
internals from commercial games.

## Core Conclusion

The project should not build "a bot that knows shortcuts." It should build a
deterministic, headless, rule-strict game core first.

Everything else should sit on top of that core:

- Unity UI
- mobile UI
- manual play
- bot CPU
- replay
- online mode
- combo search
- self-play training
- tournament audit
- future research tools

The central contract is:

```text
Observation -> LegalActionMask -> Command -> EventLog -> Reducer -> NewState
```

UI and bot must never mutate game state directly.

## Architecture Layers

```text
0. Card Data Pack
1. Domain Model
2. RulesCore Command/Query API
3. AbilityCore
4. Replay / Snapshot / Rollback
5. Feature Extraction / Evaluation
6. Probability / Simulation
7. Planner / Search
8. Bot Policy
9. Offline Knowledge / Combo Discovery
10. Research Platform / Self-Play
11. UI / Online / Tournament Services
```

## Layer 0: Card Data Pack

Responsibilities:

- store card identity, image path, set/series, clan/nation, grade, type, power,
  shield, trigger type, text, and raw source fields
- store structured ability definitions as they are added
- store semantic tags and derived features for AI
- store manifest version and content hashes
- support pack verification and repair

Current status:

- Thai Vanguard cards: 10,836
- card images: 10,836
- runtime SQLite pack exists
- verification script confirms counts and paths

Important rule:

Card text is for display and annotation. Runtime gameplay should use structured
data, not live text parsing.

## Layer 1: Domain Model

Core data structures:

- `GameState`
- `PlayerGameState`
- `CardInstance`
- `Zone`
- `Phase`
- `BattleStep`
- `GameEvent`
- `GameCommand`
- `RuleSet`
- `AbilityDefinition`
- `PendingResolution`

State must distinguish:

- true state known by the core
- public state
- player-specific private state
- bot observation
- training vector

## Layer 2: RulesCore Command/Query API

RulesCore should be the only gameplay mutation boundary.

Minimum API shape:

```text
Reset(ruleSet, seed, deckA, deckB)
GetTrueState()
GetStateView(playerId)
GetObservation(playerId)
GetLegalActions(playerId)
GetLegalActionMask(playerId)
CanExecute(command)
Execute(command)
Step(command)
MovePhase(nextPhase)
Reject(reason)
```

Important design lesson from Master Duel:

The duel/rules layer should expose query and command surfaces such as current
phase, current step, legal command mask, command execution, seeded random,
replay mode, CPU parameters, and effect delegates. Our system should mirror that
architecture pattern without copying implementation.

## Layer 3: AbilityCore

AbilityCore should be structured and explicit.

Pipeline:

```text
Trigger match
-> Pending ability queue
-> Cost validation
-> Cost payment transaction
-> Target selection
-> Effect resolution
-> Continuous recalculation
-> Duration cleanup
```

Ability data should use structured operations:

```text
CounterBlast(1)
SoulBlast(1)
EnergyBlast(3)
Draw(1)
Retire(target)
CallFromDeck(target)
PowerPlus(unit, 10000, untilEndOfTurn)
CriticalPlus(unit, 1, untilEndOfTurn)
Bind(target)
Lock(circle)
OverDress(condition)
DivineSkill(oncePerFight)
```

Do not let runtime AI parse Thai text and decide effects.

Unsupported abilities should fall back to manual resolution.

## Layer 4: Replay / Snapshot / Rollback

The core must be replayable and debuggable.

Required services:

- event log
- state snapshot
- snapshot restore
- forked timeline id
- deterministic replay
- variant seed replay
- undo support
- tournament audit export

Recommended model:

```text
Event Sourcing + Periodic Snapshot + Fork ID
```

Do not start with a full graph database. A simple snapshot tree in files or
SQLite is enough until the system needs larger research tooling.

## Layer 5: Feature Extraction / Evaluation

Card Feature Representation:

- grade
- power
- shield
- critical
- trigger type
- clan/nation
- role: attacker, booster, sentinel, extender, finisher, resource engine
- costs: CounterBlast, SoulBlast, EnergyBlast, discard
- effects: draw, search, retire, bind, stand, call, power gain, critical gain
- timing: ACT, AUTO, CONT, battle, main, ride, attack, boost, end phase
- combo tags

Board Evaluation:

- hand count
- hand shield value
- sentinel count estimate
- field pressure
- rear-guard value
- vanguard pressure
- available CounterBlast
- soul count
- energy count
- damage risk
- lethal chance
- deck-out risk
- combo readiness

This layer scores a state. It does not change the game.

## Layer 6: Probability / Simulation

Use two complementary engines.

### Exact Probability Engine

This replaces the misleading phrase "Quantum-Inspired Probability Matrix."

Better names:

- `ExactProbabilityEngine`
- `TriggerDistributionEngine`
- `RiskDistributionEngine`

Purpose:

- calculate trigger odds from public/known information
- calculate drive check and damage check distributions
- calculate expected shield pressure
- calculate heal/over trigger probability
- support fast in-turn bot decisions

Use math such as:

- hypergeometric distribution
- probability distribution tables
- expected value
- conditional probability

This engine must calculate probability, not reveal the future.

Allowed:

```text
"At least one trigger in two drive checks is X%."
```

Forbidden:

```text
"The next card is a critical trigger."
```

### Monte Carlo Simulation

Use Monte Carlo when exact calculation becomes too complex:

- multi-attack branches
- trigger allocation choices
- opponent guard policy changes
- shuffle/search during battle
- multiple ability chains
- hidden information sampling

Monte Carlo should live in the AI simulation layer, not inside the strict
runtime rules core.

Core should provide:

```text
CloneState()
ApplyCommand()
ResolveVirtualCheck(seed)
EvaluateTerminalState()
```

The planner can call those APIs many times.

## Layer 7: Planner / Search

Planning should be goal-driven and bounded.

Useful goals:

- secure next ride
- build field
- preserve CounterBlast
- deny opponent CounterBlast
- maximize shield drain
- maximize lethal chance
- survive next turn
- set up combo turn
- force Perfect Guard use

Search types:

- battle sequence search
- shallow look-ahead
- bounded state-space search
- risk-aware search
- combo search
- simulation-assisted planning

Search must have time budgets:

```text
Easy: 0-50 ms
Normal: 50-200 ms
Hard: 200-1000 ms
Research/offline: unlimited batch jobs
```

## Layer 8: Bot Policy

Bot policy chooses from legal actions only.

Difficulty model:

- Easy: shallow heuristic, visible mistakes
- Normal: goal-driven heuristic
- Hard: bounded search and probability
- Expert: deeper search plus playbook
- Research: self-play or trained policy

Bot must not:

- see hidden hands
- see top deck
- mutate state directly
- invent actions
- resolve abilities outside the core

Bot should receive:

```text
BotObservation
LegalActionMask
FeatureVector
OpponentModel
SearchBudget
```

## Layer 9: Offline Knowledge / Combo Discovery

This is where heavier AI belongs.

Modules:

- Card Semantic Reasoner
- Combo Discovery System
- Archetype Playbook Builder
- Rideline Recognition
- Deck Matchup Knowledge Base
- Ability Annotation Assistant
- Golden Test Generator

Use cases:

- tag cards
- discover combo lines
- find likely play patterns
- generate bot playbooks
- detect unsupported ability patterns
- generate tests for AbilityCore

This should run offline or in tooling. It should not slow down live gameplay.

## Layer 10: Research Platform / Self-Play

This layer is optional and future-facing.

Required foundation:

- deterministic headless core
- legal action mask
- observation masking
- reward extraction
- replay export
- dataset export
- CLI runner

RL-compatible API:

```text
Reset(seed, deckA, deckB)
GetObservation(player)
GetLegalActionMask(player)
Step(action)
GetReward()
IsDone()
ExportReplay()
```

Future tools:

- batch self-play runner
- Docker worker
- distributed simulation queue
- replay dataset store
- model evaluation harness
- playbook mining

Do not start with Kubernetes or massive RL training. Start with a local headless
CLI runner and benchmark it.

## Layer 11: UI / Online / Tournament Services

Unity UI:

- displays state
- sends legal commands
- shows choices
- plays animations
- never owns rules

Online server:

- should be authoritative
- validates commands
- emits event logs
- hides private information
- records rule version and replay hash

Tournament mode:

- audit log
- rule version
- seed/hash commitment
- replay export
- illegal action rejection reasons
- judge/debug view

## Hidden Information Rules

The core is the source of truth.

Views must be masked:

```text
TrueGameState
-> PublicGameState
-> PlayerStateView
-> BotObservation
-> TrainingVector
```

Opponent hand should be represented as hidden cards unless revealed.

Opponent deck should reveal only:

- count
- known public cards
- cards already seen
- probability estimates from public information

Known-hand tracking is allowed when information was legitimately revealed:

- drive checked card returned to hand
- searched card added to hand
- public ability added a known card to hand

Unknown draws remain unknown.

## RuleSet / Format Support

Use rule sets instead of hard-coded global behavior.

Potential rule sets:

- `StandardD`
- `VPremium`
- `Premium`
- `ThaiCustom`
- `CustomSandbox`

Feature flags:

- ride deck
- persona ride
- over trigger
- gift marker
- quick shield
- energy
- crest
- stride
- G guard
- first-turn attack restriction
- custom deck building rules

Each replay must record:

- rule set id
- rule version
- card database version
- ability database version

## Vanguard-Specific Battle Intelligence

Important AI systems:

- Opponent Guard Prediction
- Trigger Prediction
- Shield Value Estimation
- Guard Baiting
- Battle Order Search
- Trigger Allocation Planning
- Damage Zone Control
- Resource Conservation
- Kill Shot / OTK Mode
- Absolute Defense Mode

These belong mostly in AI planning/evaluation, not inside the strict core.

Core provides legal state and resolves results.

AI evaluates which line is best.

## Opponent Modeling

Opponent profiling should sit outside the core.

Core emits events:

- no guard
- guard amount
- Perfect Guard use
- damage accepted
- early rush
- rear-guard commitment
- CounterBlast usage
- damage denial
- over-guarding

Profiler derives:

- aggressive
- defensive
- control
- damage starvation
- combo setup
- risk taker
- over-guarder

For fair online play, start with match-local profiling only. Long-term profiling
should be opt-in or disabled for competitive modes.

## Ruling Conflict Resolver

Premium and complex formats require a real ruling layer.

Core modules:

- Rule Priority Table
- Specific-over-General Resolver
- Effect Layer System
- Prevention vs Requirement Resolver
- Once-per-turn tracker
- Once-per-fight tracker
- Loop detection
- Manual ruling override database

Example:

```text
General rule: a unit can move zones.
Effect A: move this unit to deck at end phase.
Effect B: this unit cannot leave rear-guard circle.
Resolver: specific prevention effect wins.
```

Every difficult ruling should become a golden test fixture.

## Dynamic Power / Continuous Effect Tracker

Power should be calculated as layers:

```text
base power
+ boost power
+ trigger modifiers
+ ability modifiers
+ marker modifiers
+ continuous modifiers
+ temporary battle modifiers
```

Track duration:

- until end of battle
- until end of turn
- while condition is true
- permanent
- once per turn
- once per fight

Do not recompute the whole game blindly after every action unless performance is
acceptable. Prefer invalidation and targeted recalculation later.

## Resource Ledger

Costs should be transactions.

Examples:

- CounterBlast
- SoulBlast
- EnergyBlast
- discard
- retire own unit
- bind
- remove marker

Transaction rule:

```text
Validate all costs -> reserve resources -> pay costs -> emit event
```

If validation fails, reject the command and leave state unchanged.

## Illegal Action Interceptor

Every command must return one of:

```text
Accepted
Rejected(reason)
NeedsChoice(options)
PendingResolution
```

The core must reject:

- wrong phase actions
- invalid targets
- unavailable costs
- illegal circle placement
- invalid guard timing
- invalid trigger allocation
- unsupported automatic resolution

Rejection should never corrupt state.

## Advanced Ideas: Keep, Defer, Or Reject

### Keep Now

- RulesCore command/query facade
- legal action mask
- hidden state masking
- seeded RNG
- event log
- snapshot/rollback
- AbilityCore foundation
- trigger resolver
- power tracker
- resource ledger
- exact trigger probability engine

### Keep For Near Future

- battle sequence search
- opponent guard prediction
- board/resource evaluator
- rideline recognition
- archetype playbook
- time-travel fork debugger
- golden ruling tests
- headless CLI runner

### Keep For Later

- Monte Carlo batch simulation
- self-play runner
- RL dataset export
- model quantization
- distributed worker pool
- custom format sandbox
- generative card balance testing

### Defer Heavily

- Kubernetes cluster
- graph database for snapshots
- long-term player profiling
- WebAssembly rewrite
- Rust/C++ rewrite
- AR physical-card judge
- full computer vision card recognition

### Reject As Runtime Behavior

- bot seeing hidden card identities
- bot seeing top deck
- probability replacing actual random outcomes
- LLM resolving live card effects
- AI mutating state outside RulesCore
- unsupported custom formats bypassing legal validation

## Exact Probability Engine Notes

The proposed "Quantum-Inspired Probability Matrix" is valid if renamed and
implemented as exact probability math.

It should calculate risk, not determine outcomes.

Live gameplay still uses actual RNG.

Correct use:

```text
AI asks: What is the probability of at least one trigger in two checks?
Core/Probability service answers: 33.1% from known public deck composition.
AI chooses whether to attack based on risk.
Actual check still draws from shuffled deck using RNG.
```

Incorrect use:

```text
Engine averages the trigger result and applies expected power directly.
```

That would damage the game because Vanguard depends on real uncertainty.

## Generative Card Balancing

Useful as a future offline developer tool.

Pipeline:

```text
Prototype card
-> structured ability definition
-> sandbox card pack
-> self-play/batch tests
-> loop detection
-> win-rate report
-> combo risk report
```

Do not make this part of the live game core.

## Edge / Mobile / WASM

Long-term direction:

- headless core first
- CLI runner second
- mobile optimization third
- WASM or native rewrite only if performance requires it

Do not rewrite the current Unity/C# core early. First make the architecture
clean, deterministic, and testable.

## Custom Format Sandbox

Custom rules are useful, but they must still be rule-checked.

Start with feature flags:

- allow mixed clan/nation
- allow or disable ride deck
- allow or disable over trigger
- allow or disable gift marker
- allow or disable stride

Avoid supporting extreme formats such as double vanguard until the standard core
is stable.

## AR Judge Core

AR physical-card judging is a separate future product.

Required components:

- computer vision card recognition
- card orientation detection
- zone recognition
- real-time digital twin sync
- manual correction UI
- judge/ruling overlay

The current project can prepare by keeping RulesCore deterministic and exposing
clear state update APIs, but AR should not be part of the current roadmap.

## Implementation Priority

Recommended next order:

1. RulesCore command/query facade
2. legal action mask
3. hidden state / observation masking
4. seeded RNG service
5. snapshot / rollback
6. phase and battle window controller
7. trigger resolver
8. resource ledger
9. dynamic power tracker
10. AbilityCore foundation
11. exact trigger probability engine
12. board/resource evaluator
13. battle sequence search
14. opponent guard predictor
15. time-travel fork debugger
16. headless CLI runner
17. self-play dataset exporter
18. distributed training tools

## M5.5 Alignment

The current implementation plan now adds `M5.5 Rules Core Hardening`.

M5.5 should cover:

- RulesCore command/query facade
- shared legal command mask
- seeded RNG
- snapshot/restore
- AbilityCore foundation
- content-addressed pack cache hardening

This must happen before making the CPU bot significantly smarter.

## Final System Shape

```text
Vanguard Core Engine
├─ TrueGameState
├─ RuleSet / Phase / Battle Window
├─ Hidden State Manager
├─ LegalActionMask
├─ Command Executor
├─ AbilityCore
├─ Ruling Resolver
├─ Trigger Resolver
├─ Power Tracker
├─ Resource Ledger
├─ Event Log
├─ Snapshot / Rollback
├─ Deterministic RNG
└─ Headless Step API

AI Platform
├─ Observation Encoder
├─ Card Feature Extractor
├─ Board Evaluator
├─ Exact Probability Engine
├─ Monte Carlo Simulator
├─ Battle Search
├─ Goal Planner
├─ Opponent Profiler
├─ Combo Discovery
├─ Playbook Builder
├─ Self-Play Runner
└─ Dataset Exporter

Clients And Services
├─ Unity PC UI
├─ Mobile UI
├─ Online Server
├─ Replay Viewer
├─ Tournament Audit Tools
└─ Future AR Judge
```

The core rule:

```text
Core decides what is legal and what happens.
AI decides which legal command is worth taking.
UI displays state and sends commands.
Research tools analyze event logs and simulations.
```
