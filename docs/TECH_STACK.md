# Tech Stack

## Current Client

- Unity
- C#
- Target platforms:
  - Windows first
  - Android next
  - iOS later

## Current Local Data

- SQLite for structured card data.
- JSON manifest for pack metadata.
- Filesystem image cache.
- Python data/verification tools.

## Current Core Direction

- C# pure logic inside the Unity project first.
- Keep game logic separable from Unity UI.
- Evolve toward a headless C# core before considering native/WASM rewrites.
- Do not rewrite to Rust/C++/WASM until profiling proves it is necessary.

## AI And Simulation Direction

- Heuristic bot first.
- RulesCore legal action mask before smarter planning.
- Exact probability engine before heavy Monte Carlo.
- Monte Carlo/search outside live core.
- Headless CLI runner now exists for deterministic local simulation.
- Headless batch simulation and summary dataset export now exist.
- Headless observation/action/reward contracts now exist, but they are not an
  RL training loop yet.
- Headless performance profiling now exists as timing-only diagnostics.
- Packed-state work is gated by a decision report; readable `GameState` remains
  the source of truth.
- Distributed worker work is spec-first and remains local-only.
- The distributed worker prototype now exists as local in-memory orchestration
  only; no cluster, Photon worker, or RL trainer exists yet.
- RL frameworks only after deterministic core, masking, replay, and action masks exist.

Potential later tools:

- Python for offline analysis, combo discovery, and dataset processing.
- Stable-Baselines3 / PettingZoo style interfaces for later RL experiments.
- Docker only after local headless batch simulation exists.

## Future Backend

Online transport:

- Photon Realtime first for room matchmaking and reliable `GameEvent` delivery.
- Keep custom server paused until ranked/tournament integrity needs it.
- Preserve the M8 event-envelope protocol so a future server can replace Photon
  without rewriting the game core.

Profiles/decks option:

- Firebase Auth
- Firestore/Realtime Database for profiles/decks
- Cloud Storage or Cloudflare R2 for assets

Long-term authoritative option:

- Custom API
- PostgreSQL
- WebSocket room server

Online play must keep local RulesCore, replay, and hidden information boundaries
stable; transport code must not become a second game engine.

## Build And CI

- GitHub Actions.
- GameCI Unity Builder later.
- Python unit tests in CI via `.github/workflows/python-tests.yml`.
- Data validation in CI via `.github/workflows/data-validation.yml`.
- Unity compile in CI via `.github/workflows/unity-compile.yml` on a
  self-hosted Windows/Unity runner.
- Unity EditMode tests in CI via `.github/workflows/unity-editmode.yml`.
- Core regression suite inventory exists as `CoreRegressionSuiteReportBuilder`
  and is covered by EditMode tests.
- Ability regression suite inventory exists as
  `AbilityRegressionSuiteReportBuilder` and is covered by Python plus EditMode
  tests.
- Multiplayer payload/no-leak suite inventory exists as
  `MultiplayerPayloadNoLeakSuiteReportBuilder` and is covered by EditMode
  tests.
- Windows build artifact runner exists as
  `VanguardThaiSim.EditorTools.WindowsBuildArtifactRunner.RunFromCommandLine`
  and writes to `client/unity/VanguardThaiSim/build/windows/latest/`.
- Android build artifact runner exists as
  `VanguardThaiSim.EditorTools.AndroidBuildArtifactRunner.RunFromCommandLine`
  and writes to `client/unity/VanguardThaiSim/build/android/latest/`.
- Local release candidate evidence is tracked in
  `docs/RELEASE_CANDIDATE_CHECKLIST.md`.
- `ClientSmokeFlowRunner` for pre-build Windows/Android readiness smoke.
- Git LFS only for selected project assets, not the full card image dump.

## AI/Agent Workflow

- `AGENTS.md`
- `docs/AI_CONTEXT_BRIEF.md`
- `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`
- `docs/CORE_DEVELOPMENT_GUARDRAILS.md`
- subsystem-specific specs in `docs/`
