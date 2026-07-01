# ISMCTS Readiness Gate Spec

## Scope

`M14-09` adds a checklist artifact that decides whether the advanced search
prototype may begin. It is a gate, not the advanced search algorithm itself.

## Default Requirements

- bot uses `RulesCore` legal actions
- hidden-state boundaries are preserved
- snapshot simulation uses cloned branch state only
- trigger probability is planning-only
- guard and battle advisors are deterministic
- debug traces are sanitized
- no-mutation tests exist for current bot/search helpers

## Output

`IsmctsReadinessReport` includes:

- gate id
- `advanced_search_allowed`
- ready and blocked counts
- summary
- checklist items

Each item includes:

- requirement id
- title
- ready flag
- evidence

## Decision Rule

Advanced search is allowed only when at least one requirement exists and every
requirement is ready.

## Verification

EditMode tests must cover:

- default gate allows the M14-10 prototype
- a blocked requirement prevents advanced search
- JSON round-trip
- deterministic output

## Non-Goals

- No ISMCTS implementation.
- No rollout policy.
- No neural/RL integration.
