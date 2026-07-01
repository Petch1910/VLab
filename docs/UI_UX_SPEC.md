# UI UX Spec

## Target Platforms

- Active: Windows desktop.
- Deferred: Android mobile and iOS until the user explicitly reopens
  mobile/app work.

Current active direction is Windows-first program completion. Do not make
Android/mobile layout, APK, app packaging, or release-candidate work part of the
active UI queue until that track is explicitly reopened.

## Main Screens

### 1. Home Dashboard ( LOBBY-FIRST )
- User status (Guest/Profile)
- Selected deck summary + validation status
- Quick links to the 3 main menus: Card Workshop, Battle Center, and System Hub

### 2. Card Workshop ( Consolidated Split-Screen )
- Left half: Deck Builder active deck list, trigger/grade counters, and save/export controls.
- Right half: Searchable, filterable Card Database (browsing all cards by Nation/Series).
- Instant add/remove interactions (drag-and-drop or simple click) without state loss.
- Custom Pack validation and image export inside Deck Tools modal.

### 3. Battle Center ( Play Prep & Matchmaking )
- Solo Practice vs CPU (select deck, bot deck, bot difficulty).
- Online Room (Photon matchmaking lobby). Includes a **Navigation Lockout** where back navigation is disabled until the player explicitly clicks "Leave Room" to disconnect cleanly.
- Quick Deck Selector and Quick Edit modal embedded directly in prep lobbies.
- Replay File Browser to load local logs.

### 4. Play Table ( Gameplay )
- Full-Screen Transition when starting any game/replay to maximize display space.
- Board zones (Vanguard, Front/Back rows) first, visual card thumbnails on board circles.
- Local hand strip and selected card detail with Thai card text.
- Phase Timing Matrix enforcement and attack declaration flow.
- Replay/Match Log with player-readable statements.
- **Manual Slide-out Drawer Overlay:** Open rules/manual over the live board (using ESC or top icon) without interrupting the Photon connection.

### 5. System Hub ( Options & Help )
- Options: player name, scale, clear image caches.
- Manual: Vanguard rules basics, format details, and app guide.

## Input

- Mouse drag/click on PC
- Touch tap/drag on mobile (deferred)
- Avoid small controls on mobile
- Android phone/tablet responsive profiles must keep interactive controls at
  least `48` pixels tall and pass `ResponsiveLayoutQaVerifier` reference
  viewport checks before mobile smoke work proceeds.

## Visual Rule

Clarity beats decoration. Card text, zones, and legal actions must be readable first.

## Player Experience Reset

The UI must follow `docs/UI_EXPERIENCE_REDESIGN_SPEC.md` and `docs/ui_overview.md` before any further
feature expansion that touches player-facing screens.

Current direction:
- Start from a Home Dashboard screen, not raw card search.
- Keep Card Workshop, Battle Center, Play Table, and System Hub as distinct modes.
- Product decision 2026-06-27: use Vanguard Area-style clan/nation grouping as
  the active card taxonomy baseline for Deck Builder and Card Browser filters.
- Keep the Play Table zone-first. Normal players should not see debug/network
  or pending-automation controls unless they open an Advanced panel.
- Use VangPro and Vanguard Area only as UX references. Do not copy their code,
  assets, exact frames, button art, backgrounds, or proprietary layouts.
