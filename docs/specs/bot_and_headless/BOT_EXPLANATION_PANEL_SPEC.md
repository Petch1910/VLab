# Bot Explanation Panel Spec

Milestone: `M26-03`

## Purpose

Expose bot decisions as player-readable guidance instead of developer debug
traces. The panel should help a player understand the bot's chosen action
without leaking hidden information, raw scores, private ids, or implementation
payloads.

## Rules

- Use `BotDebugTrace` as input only after it has been generated from masked
  legal-action bot context.
- Show a short selected action, a simple reason, optional play style, and a
  small list of top options.
- Do not show raw `base=`, `bias=`, `total=`, `score=`, `playbook=`, card
  instance ids, hidden card ids, top-deck identities, or opponent private hand
  identities.
- The panel is read-only and must not mutate `GameState`.

## Current Scope

M26-03 adds a formatter and a PlayTable side-panel surface. It does not add new
bot intelligence or execute bot decisions automatically.

## Verification

- Formatter tests cover empty state, selected action formatting, option limits,
  and raw developer detail redaction.
- Unity compile, EditMode, Windows build, and Windows player smoke remain green.
