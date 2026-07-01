# RuleSet Profile Spec

## Status

Implemented in `M11-11` as the central format/RuleSet profile scaffold.

## Purpose

Keep Standard, V-Premium, and Premium mechanics out of shared hard-coded core
logic. Runtime systems should ask a `RuleSetProfile` for feature flags instead
of checking raw format strings in UI, bot, ability, or network code.

## Runtime Surface

`RuleSetProfile` stores format-level feature flags:

- ride deck
- persona ride
- over trigger
- front trigger
- stand trigger
- imaginary gift
- stride
- G-Guard
- G zone
- energy module
- nation fight
- clan fight
- extreme fight

`RuleSetProfileCatalog.CreateCoreProfiles()` returns the current core profiles:

- `standard`
- `v_premium`
- `premium`

`RuleSetProfileCatalog.Resolve(format)` and
`RuleSetProfileCatalog.Resolve(gameState)` resolve aliases such as `D`, `V`,
and `P` into a cloned profile without mutating `GameState`.

`RuleSetProfileCatalog.ValidateCatalog(catalog)` rejects duplicate profile ids
or aliases before custom format profiles are allowed to join the runtime.

## Current Core Profiles

Standard:

- enables ride deck, persona ride, over trigger, front trigger, energy module,
  and nation fight
- disables imaginary gift, stride, G-Guard, G zone, stand trigger, and clan
  fight
- energy remains conditional on Energy Generator/crest state; the profile only
  enables the module

V-Premium:

- enables imaginary gift, front trigger, and clan fight
- disables ride deck, persona ride, over trigger, stride, G-Guard, G zone, and
  energy module

Premium:

- enables stride, G-Guard, G zone, stand trigger, front trigger, over trigger,
  imaginary gift, and clan fight
- disables ride deck and energy module for the base profile
- future event/extreme profiles should be overlays, not edits to this base
  profile

## Boundary

This milestone does not:

- enforce every format rule in `RulesCore`
- implement deck construction validation per format
- implement stride, G-Guard, gift, ride deck, or energy execution
- add custom format UI
- claim complete official tournament rule coverage

The profile is the central contract that later M12/M15/M18 work can use to
avoid format-specific hard-coding.

## Verification

EditMode coverage verifies:

- Standard flags are separated from V/Premium-only mechanics
- V-Premium flags are separated from Standard/Premium-only mechanics
- Premium flags expose stride/G-zone/legacy trigger mechanics
- resolving from `GameState.format` does not mutate state
- duplicate profile ids and aliases reject
- catalog/resolution JSON round-trip
- missing and unknown format rejection

## Next Work

`M11-12` closes the RulesCore milestone with regression verification and docs
before structured ability data starts in `M12`.
