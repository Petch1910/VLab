# Windows Reconnect UX Spec

Milestone: `M25-04`

## Purpose

Make reconnect failures understandable in the Windows Online Room without
changing the reconnect protocol.

## Scope

This milestone only improves player-facing status and rejection text for
existing reconnect request/batch handling.

It must not change:

- Photon event codes
- reconnect batch payload shape
- event replay protocol
- hidden-state masking
- room lifecycle
- Start Table session creation semantics

## Required UX

Reconnect text must explain:

- no pending reconnect work
- request sender and requested event cursor
- batch room mismatch
- batch waiting for room state
- batch cursor equal to local cursor and ready to apply
- batch cursor behind local cursor
- batch cursor ahead of local cursor, which would create a replay gap

Start Table rejection text must translate reconnect-specific rejection codes
into an action-oriented explanation.

## Boundary

The current lobby can only hand a reconnect batch to a newly created session
when its `from_event_index` matches the session cursor. It must keep rejecting
cursor gaps to avoid divergent event logs.

## Verification

- Formatter tests cover room mismatch and cursor-gap explanations.
- Start Table rejection formatter covers reconnect cursor mismatch.
- Windows-only verification: Unity compile, EditMode, Windows build, and player
  smoke.
