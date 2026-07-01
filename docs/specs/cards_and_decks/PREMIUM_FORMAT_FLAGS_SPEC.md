# Premium Format Flags Spec

## Scope

`M15-08` adds the Premium custom-format preset. It delegates all rules flags to
the existing core `premium` `RuleSetProfile`; it does not duplicate or override
gameplay flags.

## Preset

`CustomFormatProfileCatalog.CreatePremiumPreset()` returns:

- `format_id`: `premium`
- `display_name`: `Premium`
- `base_rule_set_profile_id`: `premium`
- aliases: `p`, `premium-clan`
- allowed pack ids: `vanguard_th`

## Expected Base Flags

The preset resolves through `RuleSetProfileCatalog.Resolve("premium")`.

Expected enabled flags:

- stride
- G-Guard
- G zone
- stand trigger
- front trigger
- over trigger
- imaginary gift
- clan fight

Expected disabled flags:

- ride deck
- energy module
- nation fight
- extreme fight

## Boundaries

M15-08 does not:

- enforce Premium deck construction
- implement stride execution
- implement G-Guard execution
- alter `RulesCore`
- alter the base `RuleSetProfile`
- load custom format files from disk

## Verification

EditMode tests cover:

- Premium preset validates through `CustomFormatProfileCatalog`
- Premium preset resolves to the core `premium` `RuleSetProfile`
- Premium flags remain enabled
- Standard-only flags remain disabled
