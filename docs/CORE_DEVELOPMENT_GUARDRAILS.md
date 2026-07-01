# Core Development Guardrails

## Role

This file is a short checklist for any change touching the Vanguard game core,
bot decision path, simulation, probability, or ability resolution.

Primary architecture source of truth:

- `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`
- `docs/VANGUARD_CORE_RULE_ARCHITECTURE_REFERENCE.md` for the expanded rule,
  format, timing-window, fixture, and official-source checklist

This file should stay concise. Do not duplicate the full architecture here.

## Non-Negotiable Rules

- Core is the judge, not the bot brain.
- UI and bot must use the Legal Action API.
- UI and bot must not mutate `GameState` directly.
- Live gameplay must use real RNG/deck order, not probability averages.
- Probability is planning information, not an actual game outcome.
- Bot observations must hide unrevealed hands, top decks, and private choices.
- Every command must be validated before it changes state.
- Every state change must produce an event log entry.
- Random events must be replayable through seed and/or recorded outcomes.
- Simultaneous AUTO effects must enter a pending resolution queue.
- Runtime must use structured ability definitions, not live card text parsing.

## Core Modules Checklist

Before adding a core feature, decide which module owns it:

- `RulesCore Command API`
- `Strict Rule Enforcer`
- `Phase / Window Controller`
- `Hidden State Manager`
- `Actual RNG Resolver`
- `Trigger Resolver`
- `Pending Resolution Queue`
- `Resource Ledger`
- `Continuous Effect / Power Tracker`
- `Snapshot / Rollback`
- `Event Log / Serialization`
- `Ability / Keyword Registry`
- `RuleSet / Format Profile`

If the feature is search, risk scoring, combo discovery, opponent profiling, or
Monte Carlo planning, it belongs outside core and should call core APIs.

## AI Planning Boundary

AI planning may use:

- masked observations
- legal action masks
- card feature vectors
- exact probability estimates
- cloned simulation states
- replay/event logs
- opponent model summaries from public behavior

AI planning must not use:

- true hidden hand contents
- true top deck order
- direct state mutation
- unsupported commands
- LLM-generated live effect resolution

## Probability Rules

Correct:

```text
AI sees trigger chance = 33%.
AI uses that risk number to choose a legal action.
Actual game still resolves a real drive/damage check.
```

Incorrect:

```text
trigger chance = 33%
apply +3300 expected power to the attack
```

Exact probability is for planning. RNG is for the actual game.

## Simulation Rules

Simulation must:

- clone or snapshot state before applying hypothetical commands
- use a separate simulation RNG stream
- never mutate live state
- return event logs or scored outcomes
- respect hidden information by sampling only from legal unknown pools

## Required Test Fixtures

Add or update tests when touching these areas:

- ride is legal only in the correct phase
- call target circle is valid
- first player attack restriction follows RuleSet
- attack/guard math resolves correctly
- drive check trigger allocation changes power/critical correctly
- damage check heal condition is correct
- over trigger follows RuleSet and once-per-game behavior
- temporary buffs clean up at the correct timing
- bot view cannot see opponent hidden cards
- simulation clone does not affect live state
- replay from event log reaches the same final state
- invalid command is rejected without state corruption
- simultaneous AUTO abilities enter pending queue

## Merge Checklist

For every core/bot/simulation change, verify:

- layer ownership is correct: Core, AI Planning, Offline Tool, UI, or Data Tool
- command validation runs before state mutation
- hidden information is masked in player/bot views
- RNG stream is correct for live, replay, simulation, or test
- event log includes command/effect/random result details
- rollback/snapshot does not corrupt live state
- pending resolutions are queued and chosen in legal order
- temporary power/critical effects clean up correctly
- RuleSet/format flags are respected
- tests cover important edge cases

## Forbidden Runtime Behavior

- bot reads full true state
- UI mutates game state directly
- probability replaces actual random outcomes
- Monte Carlo search runs inside the strict live core
- card text is parsed live during a match
- one format's rules are hard-coded into shared core
- AUTO effects resolve without the pending queue
- simulation mutates live state
- custom format bypasses legal validation

## Short Memory Hook

```text
Core decides legality and outcomes.
RNG decides actual random reveals.
AI chooses among legal actions using risk.
Probability is a map, not the future.
Hidden state separates smart bots from cheating bots.
```
