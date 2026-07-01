# Windows-First Program Completion Spec

## Purpose

This spec resets the active product direction to finish Vanguard Thai Sim as a
usable Windows program before returning to Android, mobile QA, APK packaging,
public release, or distribution work.

The product goal for this phase is:

```text
Home Dashboard -> Card Workshop / Battle Center -> PlayTable -> Battle Center -> Home
```

The Windows program must feel like a player-facing Vanguard simulator, not a
debug harness. Comparator products may inform workflow structure, but the
implementation must use our own UI, code, data contracts, and safe local assets.

## Active Scope

Allowed active work:

- Windows Home Dashboard workflow.
- Windows Card Workshop (split-screen Deck Builder & Database) workflow.
- Windows Battle Center (Solo, Online Room connection protection, Replay browser) workflow.
- Windows PlayTable (full-screen transitions, slide-out Manual drawer overlay) workflow.
- System Hub (Settings options, Manual rules basics).
- Deck tools: export deck image, mismatch UI, custom pack validation.
- Windows-only compile, EditMode, and player smoke verification.

Deferred until explicit user instruction:

- Android build work.
- APK generation.
- LDPlayer or Android emulator smoke.
- Android/mobile layout QA.
- Mobile install smoke.
- App packaging.
- Release candidate packaging.
- Public distribution.

## Hard Gates

- Do not run Android, APK, LDPlayer, mobile, app packaging, or release-candidate
  work unless the user explicitly re-enables that track.
- Do not change Photon to Unity Netcode, UGS, a custom server, or another
  transport without an ADR.
- Do not copy assets, icons, code, data, playmats, pack files, card databases,
  or UI files from VangPro, Vanguard Area, CGS, Cardfight Connect, Dear Days,
  Dear Days 2, official websites, or commercial games.
- Use comparator products as UX/system references only.
- Keep KK Card Fight Thai export and our runtime pack as the card-data source
  of truth.
- Keep cosmetic settings out of deck legality validation.
- UI, bot, network, and automation layers must not mutate `GameState` directly.
- Hidden state must not leak to opponent, spectator, replay, or bot views.

## Active Milestone Order

1. `M20` Windows Product Reset.
2. `M21` PlayTable Windows UX Pass.
3. `M22` Windows Settings / Deck Type / Accessories.
4. `M23` In-App Manual / Tutorial.
5. `M24` Deck Builder / Import / Custom Pack UX.
6. `M25` Windows Online Room Usability.
7. `M26` Bot / Automation Return Gate.
8. `M27` Windows Stability Pass.

## M20 Acceptance Criteria

`M20` is complete when:

- Roadmap/status docs declare Windows-first program completion as active.
- Android/app/release packaging is removed from the current queue and marked as
  deferred, while historical completed work remains documented as historical.
- The Windows playable-loop checklist exists.
- The Windows-only verification profile exists.
- M20 closeout status points to `M21-01` as the next implementation target.

## Next Target

`M21` is complete through `M21-09`. `M22-01` PlayerSettings, `M22-02`
DeckAppearanceMetadata, `M22-03` Home Settings screen, and `M22-04` Deck Type /
Accessories dialog, and `M22-05` cosmetic legality separation are complete.
`M22` Windows Settings / Deck Type / Accessories, `M23-01` Manual content spec,
`M23-02` Manual screen, `M23-03` loading tips, `M23-04` original-content gate,
`M23` In-App Manual / Tutorial, `M24-01` Deck Builder Windows landscape,
`M24-02` count-line deck text, `M24-03` deck import compatibility UI, `M24-04`
CGS-like custom pack adapter spec, `M24-05` VangPro-like custom import spec,
`M24-06` local custom import validator, `M24-07` Pack Validation UI, and
`M24-08` Deck image export, `M24-09` import/custom pack workflow test rollup,
and `M24-10` M24 closeout are complete. `M25-01` Photon trusted-client room
policy, M25-02 lobby flow, M25-03 room status, M25-04 reconnect UX, M25-05
online PlayTable default UI, M25-06 replay sync/status, M25-07 online room
test rollup, and M25-08 closeout are complete. `M26-01` bot/automation return
audit, `M26-02` bot legal-action/masked-state gate, `M26-03` bot explanation
panel, `M26-04` structured ability template gate, and `M26-05` live effect no
text parsing gate are complete. `M26-06` Solo Play entry flow from Home is
complete. `M26-07` no-hidden-leak / simulation / replay regression gate is
complete. `M26-08` Bot / Automation Return Gate closeout is complete. The
`M27-01` through `M27-08` are complete. The active post-M27 pass is `M28`
Windows Gameplay Completion. `M28-01` adds a longer manual match-readiness gate
for setup, ride, call, battle, guard, checks, end phase, and replay
determinism. `M28-02` local PlayTable seat toggle is complete, and `M28-03`
PlayMode two-seat runtime UI smoke is complete. `M28-04` Windows manual match
gap audit is complete. `M28-05` PlayTable guided next-action panel is complete.
`M28-06` Windows built-player smoke is complete. `M28-07` PlayTable action
grouping polish, `M28-08` side-panel density audit, and `M28-09` Bot Plan
Advanced drawer cleanup are complete. `M29-01` Photon lobby navigation lockout,
`M29-02` reconnect flow polish, `M29-03` Quick Deck Selector, `M29-04` Photon
lobby Quick Edit modal, `M29-05` Online Room usability closeout audit, and
`M29-06` Online deck readiness guard are complete. `M28-10` Match Log /
Preview density review is complete. `M30-01` Windows playable loop final audit
is complete and found the Replay Home route was still locked. `M30-02` Windows
Replay entry/browser is complete. `M30-03` Windows Replay local file import is
complete. `M30-04` Windows Replay viewer launch is complete. `M30-05` Windows
PlayTable replay export is complete. `M30-06` Windows playable loop closeout
audit is complete. The active target is now `M31-01` Windows UI evidence
capture and polish audit.
