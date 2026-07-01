# ADR-0003: Manual Simulator Before Automatic Effect Engine

## Status

Accepted

## Context

Vanguard card text is large, multilingual, and complex. Attempting full automatic skill resolution from the start would block the project and create high bug risk.

## Decision

Build a manual simulator first. Support common manual actions, action log, undo, replay, and bot heuristics. Add automatic effect templates gradually after the core engine is stable.

## Consequences

Positive:

- Faster playable MVP
- Easier debugging
- Replay and bot can be developed early

Tradeoffs:

- Early gameplay needs manual player input for effects
- Bot will initially ignore or approximate many complex skills
- Effect automation requires a later dedicated phase

