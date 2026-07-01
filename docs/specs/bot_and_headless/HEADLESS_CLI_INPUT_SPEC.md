# Headless CLI Input Spec

## Milestone

`M17-02`

## Goal

Add bounded command-line input handling to the headless runner while preserving
the default M17-01 behavior when no custom input is supplied.

## Supported Arguments

- `-headlessSeed <int>`
  - Overrides the deterministic simulation seed.
  - Invalid integers reject the run before simulation.
- `-headlessRuleset <D|Standard|V|V-Premium|P|Premium>`
  - Normalizes to `D`, `V`, or `Premium`.
  - Current M17-02 behavior records/applies the value to the deck format; full
    RuleSet execution differences remain future work.
- `-headlessDeckCode <VGTH1...>`
  - Imports a deck code and validates it through the existing deck validator.
  - If omitted, the runner keeps the generated default deck from M17-01.
- `-headlessResultPath <path>`
  - Writes the minimal result JSON to a chosen path.

## Non-Goals

- No deck path/file import yet.
- No multiple-player asymmetric deck inputs yet.
- No replay artifact expansion; that remains `M17-03`.
- No batch/self-play execution; that remains `M17-04`.

## Verification

- Default no-argument behavior remains accepted.
- Custom seed/ruleset runs are accepted.
- Valid deck-code input is accepted and reports `deck_source = deck_code`.
- Invalid deck-code input returns a rejected result.
- Invalid CLI seed input returns parser errors without starting simulation.
