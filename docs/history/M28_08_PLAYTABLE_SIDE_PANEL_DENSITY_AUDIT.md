# M28-08 PlayTable Side-Panel Density Audit

## Scope

This audit reviews the visible PlayTable side panel after M28-05 and M28-07
added player guidance. It is a Windows UI audit/spec only and does not change
runtime code.

## Current Visible Side Panel

The side panel currently exposes these visible sections before the Advanced
drawer:

- Manual Table selection status.
- Next Action.
- Action Groups.
- Setup.
- Battle Flow.
- Bot Plan.
- Manual Notes.
- Selected Card Preview.
- Zone Status.
- Match log.

The Advanced drawer is hidden by default and contains online/debug/automation
surfaces, which is correct.

## Findings

### P1: Bot Plan Should Not Be Primary In Manual Table

`Bot Plan` is useful for M26 bot explanation work, but it is not part of the
core manual match flow. Keeping it visible by default competes with:

- Next Action.
- Setup guidance.
- Battle Flow.
- Selected Card Preview.
- Match log.

Recommendation: move Bot Plan into the Advanced drawer or hide it unless bot
mode is active. This is the safest first density reduction because it does not
remove manual-play information.

### P2: Match Log Height Is Large

The match log is important, but it currently competes with card preview and
guidance. If the side panel still feels cramped after moving Bot Plan, reduce
the default visible log height or add a compact log summary with an Advanced
full log.

Recommendation: defer until after Bot Plan move.

### P2: Selected Card Preview Is Correctly Primary

Selected-card preview is still worth keeping visible. It explains the selected
card, legal action hint, stats, and card text. Do not move it to Advanced.

### P3: Action Groups Legend May Become Redundant Later

The action group legend is useful while buttons are dense. If a later milestone
physically splits or labels button rows, the legend may become unnecessary or
can move into Manual/Advanced.

Recommendation: keep it for now.

## Decision

The next code slice should be:

`M28-09`: move Bot Plan out of the primary side panel into Advanced drawer, or
hide it when no bot trace is active.

This keeps the manual table focused on player flow while preserving bot
debugging work for later.

## Verification

Docs-only audit. Runtime verification remains the M28-07 baseline:

- Unity EditMode `1137/1137`.
- Unity PlayMode `2/2`.
- Client smoke `blockers=[]`.
- Windows build `errors=0`, `warnings=0`.
- Windows player smoke `blockers=[]`.
