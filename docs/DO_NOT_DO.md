# Do Not Do

## Legal And Asset Boundaries

- Do not copy code from VangPro, Dear Days, Dear Days 2, Vanguard EX, Master
  Duel, or any commercial game.
- Do not extract, redistribute, or publish proprietary assets without rights.
- Do not restore quarantined files or run suspicious binaries for research.
- Do not run/debug/patch commercial game executables for this project.
- Safe research is limited to metadata, file inventory, hashes, headers, and
  high-level static strings when explicitly needed.

## Data Safety

- Do not delete `outputs/kk_cardfight_export/data/` without explicit approval.
- Do not overwrite the main card database without backup/versioning.
- Do not commit the full card image dump directly into a normal source repo.
- Do not mix user saves into generated card pack folders.

## Architecture

- Do not change the main stack from Unity + C# + SQLite without an ADR.
- Do not build online/multiplayer before local RulesCore and replay are stable.
- Do not bind bot logic directly to UI.
- Do not let UI mutate `GameState` directly.
- Do not let AI mutate `GameState` directly.
- Do not bypass the Legal Action API.
- Do not hard-code one format's rules into shared core.

## Runtime AI And Rules

- Do not let bot see hidden opponent hands.
- Do not let bot see unrevealed top deck order.
- Do not use probability averages as actual trigger outcomes.
- Do not run Monte Carlo search inside the strict live core.
- Do not parse card text live during a match to resolve effects.
- Do not let LLMs resolve live card effects.
- Do not resolve simultaneous AUTO abilities outside the pending queue.
- Do not let simulation mutate live state.
- Do not add custom format behavior that bypasses validation.

## Development Process

- Do not skip tests because a change feels small.
- Do not run Android build, APK generation, LDPlayer smoke, ADB/emulator smoke,
  Android/mobile layout QA, app packaging, release-candidate packaging, or
  public distribution while the active plan is Windows-first program completion
  unless the user explicitly re-enables that track.
- Do not do large unrelated refactors while implementing a narrow task.
- Do not use token-saving communication styles to shrink source-of-truth specs,
  architecture decisions, acceptance criteria, test plans, or handoff context
  below executable clarity.
- Do not add a new architecture direction without updating:
  - `docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md`
  - relevant subsystem spec
  - `docs/IMPLEMENTATION_PLAN.md`
  - an ADR if the decision is structural
- Do not duplicate full architecture content in multiple docs. Link to the
  summary and keep subsystem docs focused.
