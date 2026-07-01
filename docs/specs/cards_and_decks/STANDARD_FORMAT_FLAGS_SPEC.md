# Standard Format Flags Spec

## Scope

`M15-06` adds the Standard custom-format preset. It delegates all rules flags to
the existing core `standard` `RuleSetProfile`; it does not duplicate or override
gameplay flags.

## Preset

`CustomFormatProfileCatalog.CreateStandardPreset()` returns:

- `format_id`: `standard`
- `display_name`: `Standard`
- `base_rule_set_profile_id`: `standard`
- aliases: `d`, `standard_d`
- allowed pack ids: `vanguard_th`

## Expected Base Flags

The preset resolves through `RuleSetProfileCatalog.Resolve("standard")`.

Expected enabled flags:

- ride deck
- persona ride
- over trigger
- front trigger
- energy module
- nation fight

Expected disabled flags:

- imaginary gift
- stride
- G-Guard
- G zone
- stand trigger
- clan fight
- extreme fight

## Boundaries

M15-06 does not:

- enforce Standard deck construction
- alter `RulesCore`
- alter the base `RuleSetProfile`
- enable/disable packs automatically
- load custom format files from disk

## Verification

EditMode tests cover:

- Standard preset validates through `CustomFormatProfileCatalog`
- Standard preset resolves to the core `standard` `RuleSetProfile`
- Standard-only flags remain enabled
- V/Premium-only flags remain disabled
