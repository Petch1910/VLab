# M39-04 Second-Slice Recipe Scale Decision

## Summary

- Target slice: `Classic Core` / `โอราเคิล ทิงค์ แทงค์`
- Decision ready: `True`
- Offline recipe pipeline allowed: `True`
- Runtime deck promotion allowed: `False`
- Bot playbook promotion allowed: `False`
- Blocking issues: `0`
- Ready for M40: `True`

## Evidence

- First fixture headless consumed: `True`
- Second-slice fixture ready: `True`
- Classic Core policy reusable: `True`
- Second-slice probe ready: `True`
- Probe cards: `103`
- Probe edges: `2660`
- Candidate edges: `259`
- Manual-review cards: `7`

## Decision

First fixture consumption passed and Oracle Think Tank has reusable Classic Core fixtures plus a passed semantic/compatibility probe.

Allowed next work: offline review packets, advisory recipe drafts, validators, and repair candidates.

Still blocked: saved-deck injection, UI publication, runtime deck promotion, and bot/playbook promotion.

## Proposed M40 Queue

- `M40-01`: Second-slice review packet - Export Oracle Think Tank candidate edges, manual-review cards, and fixture notes for review.
- `M40-02`: Second-slice recipe draft model - Create advisory recipe drafts only; no saved deck injection.
- `M40-03`: Second-slice recipe validator - Validate count, trigger, grade, clan identity, copy limits, and missing cards.
- `M40-04`: Second-slice combo-to-recipe consistency - Check selected combo lines are present and not blocked by manual review.
- `M40-05`: Second-slice blocker repair candidates - Generate source-backed repair candidates for blocked recipes.
- `M40-closeout`: Second-slice runtime readiness decision - Decide whether any recipe can later enter a human-acceptance/runtime fixture gate.

## Issues

- None

## Next

`M40-01`: Second-slice review packet
