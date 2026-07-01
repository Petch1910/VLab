# ADR-0001: Use Unity For The Main Client

## Status

Accepted

## Context

The app must run on Windows and mobile, and eventually needs a playable card table with drag/tap interactions, animations, touch input, bot play, and possible multiplayer.

## Decision

Use Unity + C# as the main app/client technology.

## Consequences

Positive:

- Builds to Windows and Android from one project
- Better fit for game table interactions than a pure desktop web wrapper
- Bot/game logic can live close to gameplay

Tradeoffs:

- Requires Unity project discipline
- UI and data logic must be carefully separated
- CI builds need Unity licensing setup

