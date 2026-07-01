# Guard Decision Bot Spec

## Scope

`M14-02` adds an advisory guard decision model. It recommends whether the
defending bot should guard a declared attack, prefer a perfect guard, accept the
damage, or report that the current visible/estimated shield cannot cover the
attack.

This does not add a `RulesCore` guard command. It is a bot planning helper only.

## Inputs

- visible `GameState` from the defender's perspective
- `GuardDecisionRequest`
  - defender player index
  - attacker player index
  - attack power
  - defending power
  - incoming critical
  - whether the attack can gain trigger pressure
- optional `ICardRepository` for visible shield values
- optional `TriggerProbabilityResult`
- optional guard and shield-estimator options

## Output

`GuardDecisionResult` includes:

- decision: `NoGuard`, `Guard`, `PerfectGuardPreferred`, or `CannotGuard`
- rounded shield needed
- defender damage before and after no-guard
- expected and maximum shield estimates
- trigger risk
- lethal/high-damage/high-trigger flags
- sanitized reason text

## Decision Rules

- Shield needed is the shield required to make defending power greater than
  attack power, rounded up to the configured shield step.
- If no shield is needed, recommend `NoGuard`.
- If the attack is not lethal, not high-damage, and not high-trigger-risk,
  recommend `NoGuard`.
- If guarding is necessary but maximum estimated shield is below needed shield,
  recommend `CannotGuard`.
- If guarding is necessary and the shield need is large or expected shield is
  below needed shield, recommend `PerfectGuardPreferred`.
- Otherwise recommend `Guard`.

## Hidden-State Policy

- The caller must pass a defender-visible state, usually a player view.
- Opponent hand, deck, and private events must stay masked before this helper is
  called.
- The helper never mutates `GameState`.
- Reason strings use only numeric risk fields and never include card ids or
  instance ids.

## Verification

EditMode tests must cover:

- lethal attack recommends guard when shield is available
- low-damage non-lethal attack recommends no guard
- high trigger risk at danger damage recommends guard
- insufficient shield returns cannot-guard
- high shield demand can prefer perfect guard
- no `GameState` mutation and no opponent private id leak
- deterministic repeated decisions

## Non-Goals

- No guard action execution.
- No perfect guard card search by effect text.
- No opponent playstyle profile. That belongs to later M14/M17 work.
