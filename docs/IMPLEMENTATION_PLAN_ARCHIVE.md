# Implementation Plan Archive (M0-M19)

This file preserves the historical milestone records from the original
`IMPLEMENTATION_PLAN.md`. The active plan is in `IMPLEMENTATION_PLAN.md` (M20+).

## Status Summary Table (M0-M19)

| ID | Status | Summary |
|---:|--------|---------|
| M0-01 | Done | created tools, data pack, client/unity, tests folders |
| M0-02 | Done | Unity Hub, Editor 6000.5.0f1, Android modules, 2D project |
| M0-03 | Done | Unity Test Framework, SQLite runtime, EditMode test dummy |
| M1-01 | Done | `data/packs/vanguard_th/manifest.json` |
| M1-02 | Done | `tools/data/build_vanguard_th_pack.py` and `cards.sqlite` |
| M1-03 | Done | `tools/verification/verify_vanguard_th_pack.py` |
| M2-01 | Done | `SqliteCardRepository` reads manifest/SQLite |
| M2-02 | Done | card browser paged grid with lazy-load images |
| M2-03 | Done | search + series/clan filter dropdowns |
| M2-04 | Done | detail panel with large image, name, text, series, clan, grade, power |
| M3-01 | Done | `VanguardDeck` main/ride/G zones, quantities, JSON round-trip |
| M3-02 | Done | `DeckValidator` counts, unknown cards, copy limits |
| M3-03 | Done | card browser add/remove to deck, live counts/issues |
| M3-04 | Done | `DeckStorage` local JSON, `DeckCodeCodec` `VGTH1.` base64url |
| M4-01 | Done | `GameStateFactory` two-player from decks, shuffle, draw, zones, phase |
| M4-02 | Done | `GameActionService` draw, move, set-phase, event log, undo |
| M4-03 | Done | `PlayTableBootstrap` manual table with zones, buttons, log |
| M4-04 | Done | `GameReplay` / `GameReplayPlayer` event log save/load/step |
| M5-01 | Done | `LegalActionGenerator` draw/move/phase actions |
| M5-02 | Done | `EasyBotController` simple turn via legal actions |
| M5-03 | Done | `ProfileBotController` Aggro/Balanced/Defensive with seed |
| M5.5-01 | Done | `RulesCore` facade, legal actions, command validation |
| M5.5-02 | Done | `SeededRandomService`, `GameStateSnapshot` restore/branch |
| M5.5-03 | Done | `AbilityCore` structured effects, custom registry, manual fallback |
| M5.5-04 | Done | `asset_index.json` SHA-256, manifest cache/hash fields |
| M6-01 | Done | `CardImageCache` bounded thumbnail/full caches, fallback, clear |
| M6-02 | Done | `ResponsiveLayoutProfile` desktop/tablet/phone sizing |
| M7-01 | Done | `CUSTOM_CARD_PACK_SPEC.md`, template, schema validator |
| M7-02 | Done | `import_custom_pack.py` directory/zip import, verify, build |
| M8-01 | Done | `MultiplayerProtocol` room state, event envelopes, reconnect |
| M8-02 | Done | `MockMultiplayerRoom` local sync, pack mismatch reject |
| M8-03 | Done | Photon Realtime selected, payload codec, transport interface |
| M8-04 | Done | Photon SDK 5.1.15, `VANGUARD_PHOTON_REALTIME` define |
| M8-05 | Done | `PhotonRealtimeSmokeTestRunner` live two-client smoke |
| M8-06 | Done | `GameStateViewFactory` player/spectator/true views |
| M8-07 | Done | `MultiplayerLobbyController`, lobby bootstrap, Online Room entry |
| M8-08 | Done | `NetworkReconnectRequest`, Photon payloads, lobby reconnect |
| M8-09 | Done | `PhotonLobbySmokeTestRunner` live lobby smoke |
| M8-10 | Done | `MultiplayerGameSessionController` event sync |
| M8-11 | Done | `PhotonGameSessionSmokeTestRunner` live game session smoke |
| M8-12 | Done | PlayTable online-session, routed actions, disabled undo |
| M8-13 | Done | deck-code room exchange, initial state from room, Start Table |
| M8-14 | Done | deterministic room game ids, event cursor, reconnect handoff |
| M8-15 | Done | deck privacy/commitment spec and ADR |
| M8-16 | Done | deck privacy fields, canonical commitment hashing |
| M8-17 | Done | deck reveal verification service |
| M8-18 | Done | private-event payload spec, gameplay guard |
| M8-19 | Done | `NetworkPublicGameEvent` model, Photon codec |
| M8-20 | Done | `NetworkPublicGameEventFactory` masking conversion |
| M8-21 | Done | `NetworkPublicGameReplay` masked public event logs |
| M8-22 | Done | public-event transport hooks, Photon event-code 5 |
| M8-23 | Done | reveal proof metadata for public reveal events |
| M8-24 | Done | commitment-only gameplay policy |
| M8-25 | Done | deck reveal request/response payloads, transport |
| M8-26 | Done | lobby reveal controls, verification status |
| M8-27 | Done | owner-private commitment-room initialization spec |
| M9-01 | Done | exact trigger probability engine |
| M9-02 | Done | board/resource evaluator |
| M9-03 | Done | battle sequence ranking |
| M9-04 | Done | guard/shield estimator |
| M9-05 | Done | archetype/rideline playbook model |
| M9-06 | Done | offline combo discovery report/runner |
| M10-01..112 | Done | Ability/trigger automation foundation (112 sub-tasks) |
| M11-01..12 | Done | RulesCore completion (timing, facade, replay, snapshot, masking, resource, ruleset) |
| M12-01..12 | Done | Structured ability data (schema, validator, registry, templates, fixture DSL) |
| M13-01..11 | Done | Owner-private rooms, command envelopes, public events, lifecycle, spectator, audit |
| M14-10 | Done | Advanced one-ply search prototype |
| M15-01..10 | Done | Custom pack v2, ability data, pack validation, format profiles |
| M16-01..10 | Done | UI polish: panels, filters, search, fallback, smoke flow |
| M17-01..10 | Done | Headless CLI, batch, dataset, observation API, profiler, worker |
| M18-01..10 | Done | CI pipelines, regression suites, builds, release checklist |
| M19-01..09 | Done | Player experience reset: Home, Deck Builder, PlayTable, icon system |

## Test Count Progression

| Milestone | EditMode Tests |
|-----------|---------------|
| M5.5 | ~300 |
| M10-112 | 539 |
| M11-12 | 607 |
| M12-12 | 657 |
| M13-11 | 692 |
| M14-10 | 736 |
| M15-10 | 762 |
| M16-10 | 806 |
| M17-10 | 839 |
| M18-10 | 851 |
| M19-09 | 904 |

## Key Build Artifacts

- Windows: `client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe`
- Android: `client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk`
- Python tests: 44/44 passed
- Latest EditMode: 904/904 passed

## Guardrails From Historical Phases

These rules remain in effect and are referenced by the active plan:

1. **Comparator study**: use for structure/workflow ideas only. Do not import
   proprietary art, icons, playmats, game packs, or code.
2. **M19 UI rule**: UI work must not change RulesCore behavior, GameState event
   semantics, deck validation, hidden-state masking, or network payloads.
3. **M19 visual baseline**: VangPro-style UX for player flow. Do not copy
   assets, frames, logos, button art, code, or packages.
4. **M19 taxonomy**: Vanguard Area-style clan/nation grouping from local DB.
5. **Icon rule**: Lucide subset per `UI_ICON_SYSTEM_SPEC.md`. Trigger symbols
   default to English text badges. Private icons per `UI_ICON_OVERRIDE_SPEC.md`.
6. **Transport**: Photon Realtime trusted-client rooms. No custom server
   required.
