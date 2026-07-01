# Windows Local Build Known Limitations

This document describes the current local Windows build after M27-08. It is
for internal development and personal testing only.

## Product Scope

- Windows-first only. Android, LDPlayer, APK, mobile layout QA, app packaging,
  release-candidate packaging, and public distribution remain deferred.
- The current build is a manual simulator/workbench, not a finished commercial
  product.
- Comparator products are UX/system references only. Do not copy assets, code,
  or data from VangPro, Vanguard Area, CGS, Cardfight Connect, Dear Days, or
  other products.

## Card Data

- Source of truth remains the local KK Card Fight Thai export and the runtime
  pack under `data/packs/vanguard_th/`.
- Current verified Vanguard TH card/image count is `10,836`.
- Missing image handling uses the local fallback texture and player-facing
  fallback text; it does not auto-download replacement images.
- Custom pack import is local validation only. Public CGS/VangPro-style
  auto-download is not enabled.

## PlayTable

- The PlayTable is board-first and usable for manual play, but not all Vanguard
  rules are automated.
- Card movement, phase buttons, draw/check helpers, manual notes, trigger draft
  surfaces, pending AUTO surfaces, and replay/event panels exist.
- Full structured card ability automation is intentionally limited to tested
  templates. Unsupported abilities must use manual resolution/fallback.
- The first PlayMode test covers Home -> smoke deck -> PlayTable -> Stand ->
  Draw -> Ride phase -> End phase. It is not a full end-to-end match test.
- `M28-01` adds a longer gameplay completion smoke for setup, ride, call,
  battle, guard, Drive/Damage checks, End phase, and replay determinism. This
  still does not mean every card skill or comprehensive-rule edge case is
  automated.

## Online

- Photon is the current transport.
- Online room mode is trusted-client/casual. It is not ranked-secure and does
  not provide an authoritative anti-cheat custom server.
- Public/masked event delivery, reconnect status, and deck/pack hash guards are
  foundation-level features, not a tournament-grade backend.

## Bot / Automation

- Bot and automation work must use legal action masks and masked state only.
- Advanced bot search, ISMCTS, RL, or runtime text parsing of live card effects
  must not expand until correctness-first gates remain stable.
- Current bot surfaces are explanation/safety/foundation tools, not a strong
  CPU opponent.

## Performance

- M27-04 gates data-path timing, image-cache retention, cache clear behavior,
  and bounded headless timing.
- M27-04 does not capture GPU frame time, interactive scroll frame drops, or
  exact texture memory bytes.
- PlayTable target remains `30fps`, but real frame profiling still needs a
  later profiler/manual smoke pass if the UI feels slow.

## Release Boundary

- Do not create public release artifacts unless the user explicitly asks.
- Windows build artifacts may be used locally for smoke testing only.
- Keep status docs, closeouts, and test artifacts updated before handoff.
