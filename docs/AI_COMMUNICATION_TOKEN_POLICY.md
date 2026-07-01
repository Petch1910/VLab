# AI Communication Token Policy

## Purpose

Use concise communication patterns, including caveman-style compression, as a
team communication aid only.

The goal is to reduce token cost in chat, handoff, PR comments, and status
updates without weakening the project source of truth.

## Allowed Uses

- Short team status updates.
- Task handoff summaries after the canonical docs are updated.
- PR/code review comments where the line, issue, and fix are obvious.
- Commit messages and changelog bullets.
- Chat replies where the user asks for a brief answer.

## Forbidden Uses

Do not use heavy compression for:

- Product specs.
- Architecture decisions.
- Core/rule/timing/format references.
- Acceptance criteria.
- Test plans.
- Data provenance and verification notes.
- Any document that another AI must execute without extra context.

## Rule

Source-of-truth docs must remain explicit enough that another AI can continue
work without guessing. If compression removes a boundary, invariant,
rejection reason, file path, command, test result, or milestone dependency, it
is too compressed.

## Recommended Format For Short Handoffs

```md
## Done
- M10-XX: one clear result

## Changed
- `path/to/file`: exact reason

## Verified
- command: result

## Next
1. next concrete task

## Risks
- only real blocker/risk, or `None`
```

## Caveman-Style Guidance

Good:

```text
M10-76 done. Added Photon event 11 wrapper. EditMode 420/420 passed.
Next: M10-77 transport hook. Do not resolve abilities yet.
```

Bad:

```text
Photon done. Next hook.
```

The bad version loses event code, test proof, and boundary.
