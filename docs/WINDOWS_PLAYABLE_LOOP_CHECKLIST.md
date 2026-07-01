# Windows Playable Loop Checklist

## Purpose

This checklist defines the Windows workflow that must become stable before the
project returns to mobile, release, or advanced bot work.

The target loop is:

```text
Home Dashboard -> Card Workshop / Battle Center -> PlayTable -> Battle Center -> Home
```

## Home Dashboard

Required:

- Shows the runtime pack status and card count.
- Shows current profile name and active deck summary + validity status.
- Routes clearly to:
  - 🎴 Card Workshop (Split-screen Deck Builder & Database)
  - ⚔️ Battle Center (Solo Practice, Photon Online Lobby, Replays)
  - ⚙️ System & Support (Settings, Rules Manual)
- Presents player-facing status text by default.
- Keeps debug/runtime payloads hidden from the first screen.

Current next checks:

- Confirm Home Dashboard is the first meaningful screen on Windows.
- Confirm Settings and Manual routes exist under the System & Support Hub.
- Confirm quick navigation transitions to Workshop and Battle Center.

## Card Workshop
 
Required:

- Uses a unified landscape Workshop interface where the Deck Builder (left) and Card Database (right) are visible simultaneously.
- Allows instant click/drag interactions to edit active decks without changing screens or losing editor state.
- Groups search/filter tabs by Nation/Series using `VanguardAreaClanTaxonomy`.
- Displays deck validity, grade ratio, and trigger counts clearly.
- Provides tools (Save, Load, Mismatch UI, Export Image) via a Deck Tools panel/modal.

Current next checks:

- `M24-01` should deliver the split-screen consolidated Workshop.
- `M24-08` should add deck image export from the tools modal.

## PlayTable

Required:

- **Full-Screen Transition:** Starting a match unloads standard navigation panels and launches the board in borderless full-screen mode.
- **Manual Overlay:** Pressing `ESC` or clicking a top icon slides out the manual as an overlay drawer directly over the live board, keeping gameplay connected.
- Board circles feature card thumbnail images/fallback text.
- Selected card preview displays power/shield details and Thai skill text.
- Setup flow (choosing vanguard, placing face-down, draw 5, mulligan) is guided.
- Common actions are phase-enforced (Stand/Draw, Ride, Call, Move, Battle/Attack sequence, End).
- Soul zone is modeled and updates on ride or SoulBlast.
- Log reads in player-readable match statements.

Current status:
- M21 slices aligned to deliver Soul zone, setup flow, phase actions, manual overlay, and full-screen transition.

## Replay

Required:

- Loaded from the Replay File Browser in Battle Center.
- Launches PlayTable in fullscreen spectator mode with masked event playback.

Current next checks:
- `M21-07` rewrites PlayTable event/replay text.

## Online Room (Photon Lobby)

Required:

- **Navigation Lockout:** Once connected to a Photon room, normal back navigation buttons are locked/disabled to prevent accidental disconnects. Leaving requires clicking a dedicated "Leave Room" button.
- Embedded Quick Deck Selector & Quick Edit Modal allow players to adjust their active deck inside the lobby.
- Displays Photon connection status, room ID, players, deck readiness, and hash mismatch info.
- No deck code leaks.

Current next checks:
- `M25-01` through `M25-08` implement connection hardening and Online Room prep usability.
- `M29-01` completes the navigation lockout: `Back Home` is disabled while a
  Photon room is active, and `Leave Room` is the explicit room-exit action.
- `M29-02` completes reconnect request/batch/cursor handoff text and layout
  polish.
- `M29-03` completes the first Quick Deck Selector lobby slice.
- `M29-04` completes the embedded Quick Edit deck-code modal slice.
- `M29-05` audits the Online Room pass and identifies deck readiness as the
  remaining local guard.
- `M29-06` completes the local online deck readiness guard for Host, Join, and
  Reconnect.
- `M30-01` should audit the full Windows playable loop before opening another
  feature track.
- `M30-02` should unlock the Home Replay route into a player-facing replay
  entry/browser screen.
- `M30-03` should let the Replay screen validate a local replay JSON path.
- `M30-04` should launch or preview the loaded replay through a replay viewer.
- `M30-05` exports local PlayTable replay JSON for the Replay screen.
- `M30-06` audited the full Windows playable loop and closed M30 with no
  automated Windows blocker.
- `M31-01` should capture/review current Windows UI evidence before the next
  polish/fix implementation slice.

## Done Rule

Each checklist item is only complete when the related code, tests or
verification, docs update, and result note are all present.
