# M26-06 Solo Play Home Entry Flow Spec

## Goal

Add a Windows-first Solo Practice entry flow from Home before true CPU automation
is expanded:

```text
Home -> Solo Play setup -> choose difficulty -> choose bot deck mode -> PlayTable
```

This is a product-flow milestone, not an advanced bot milestone.

## Scope

- The Home `Solo Play` button opens a setup panel instead of starting
  immediately.
- The setup panel exposes:
  - difficulty: `Easy`, `Normal`, `Hard`
  - bot deck mode: mirror player deck, random saved deck, or a specific saved
    deck when saved decks exist
- The flow validates the player deck and selected opponent deck before entering
  PlayTable.
- Starting Solo Practice clones both decks and passes a player-facing mode
  summary into PlayTable.
- PlayTable shows the Solo Practice summary in local mode text only.

## Non-Goals

- Do not execute bot turns automatically in this slice.
- Do not add ISMCTS, RL, rollout search, or new CPU strength behavior.
- Do not mutate `GameState` from Home UI.
- Do not parse live card text or automate unsupported abilities.
- Do not run Android/mobile/app packaging verification.

## Rules

- The flow is a pure setup layer before `GameStateFactory.CreateTwoPlayerGame`.
- Validation uses the existing `DeckValidator` and `PlayTableSetupReadiness`.
- Random bot deck selection filters to playable saved decks.
- Decks handed to PlayTable must be cloned so UI setup cannot mutate the
  original saved/draft deck.
- Difficulty is stored as player-facing setup metadata until later bot gates
  connect it to actual CPU turn execution.

## Verification

- EditMode tests cover difficulty cycling, opponent deck selection, rejection,
  clone behavior, and Home setup-panel navigation.
- Unity compile must pass.
- Unity EditMode must pass.
- Windows player smoke must pass because Home/PlayTable runtime flow changed.
