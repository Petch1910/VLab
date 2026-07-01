# Bot / Automation Return Audit Spec

Milestone: `M26-01`

## Purpose

Before expanding bot or automation work again, verify that the Windows-first
product pass (`M21-M25`) is closed and that the bot track will resume inside
the correctness-first guardrails.

This milestone is an audit gate. It must not make the bot smarter, add new
search algorithms, add RL/self-play behavior, or change live ability resolution.

## Required Audit Items

The audit must confirm:

- `M21` PlayTable Windows UX is closed.
- `M22` Settings / Deck Type / Accessories is closed.
- `M23` Manual / Tutorial is closed.
- `M24` Deck Builder / Import / Custom Pack UX is closed.
- `M25` Windows Online Room Usability is closed.
- Bot work remains limited to legal actions from `RulesCore`.
- Bot observations must use masked/player-legal state views.
- Snapshot simulation must not mutate live state.
- Probability remains planning-only and cannot replace actual RNG outcomes.
- Runtime card effects must use structured ability data/manual fallback, not
  LLM/live text parsing.

## Decision Rule

If every audit item is ready:

- `M26-02` may proceed.
- Advanced bot expansion, ISMCTS expansion, RL/self-play, and LLM-based live
  effect resolution remain blocked.

If any audit item is blocked:

- M26 implementation must stop and fix the prerequisite first.

## Verification

- EditMode tests must cover default pass, blocked prerequisite rejection,
  JSON round-trip, deterministic output, and the explicit block on advanced
  search/RL/LLM live effect resolution.
