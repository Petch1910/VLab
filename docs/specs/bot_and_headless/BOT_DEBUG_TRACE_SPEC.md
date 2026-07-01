# Bot Debug Trace Spec

## Scope

`M14-08` adds a compact debug trace artifact for bot decisions. The trace is
separate from `GameState`, replay logs, and network payloads.

## Inputs

- `GameState`
- player index
- `ICardRepository`
- `BotPlaybookLibrary`
- optional heuristic and playbook integration options
- max line count

## Output

`BotDebugTrace` includes:

- trace id
- player index
- playbook id
- selected action summary
- selected score
- candidate count
- sanitized flag
- explanation
- ranked trace lines

Each line includes:

- rank
- selected flag
- action summary
- base score
- playbook bias
- total score

## Hidden-State Policy

- Trace generation delegates decision scoring to `PlaybookIntegratedBot`.
- Summary strings must not include card ids or instance ids.
- Opponent private card ids and top-deck ids must not appear in trace JSON.
- Trace generation never mutates `GameState`.

## Verification

EditMode tests must cover:

- trace includes selected action and ranked lines
- JSON round-trip
- no source `GameState` mutation
- no opponent private/top-deck/priority-card id leaks
- deterministic repeated trace output

## Non-Goals

- No in-game debug UI yet.
- No network publishing.
- No persistent trace file writer yet.
