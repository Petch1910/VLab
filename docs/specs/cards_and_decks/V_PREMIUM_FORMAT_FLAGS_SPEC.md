# V-Premium Format Flags Spec

## Scope

`M15-07` adds the V-Premium custom-format preset. It delegates all rules flags
to the existing core `v_premium` `RuleSetProfile`; it does not duplicate or
override gameplay flags.

## Preset

`CustomFormatProfileCatalog.CreateVPremiumPreset()` returns:

- `format_id`: `v_premium`
- `display_name`: `V-Premium`
- `base_rule_set_profile_id`: `v_premium`
- aliases: `v`, `v-premium`, `vpremium`
- allowed pack ids: `vanguard_th`

## Expected Base Flags

The preset resolves through `RuleSetProfileCatalog.Resolve("v_premium")`.

Expected enabled flags:

- imaginary gift
- front trigger
- clan fight

Expected disabled flags:

- ride deck
- persona ride
- over trigger
- stride
- G-Guard
- G zone
- energy module
- nation fight
- extreme fight

## Boundaries

M15-07 does not:

- enforce V-Premium deck construction
- alter `RulesCore`
- alter the base `RuleSetProfile`
- execute gift behavior beyond existing manual actions
- load custom format files from disk

## Verification

EditMode tests cover:

- V-Premium preset validates through `CustomFormatProfileCatalog`
- V-Premium preset resolves to the core `v_premium` `RuleSetProfile`
- V-Premium flags remain enabled
- Standard/Premium-only flags remain disabled
