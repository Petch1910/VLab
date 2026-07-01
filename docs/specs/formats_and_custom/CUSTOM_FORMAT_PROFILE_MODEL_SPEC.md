# Custom Format Profile Model Spec

## Scope

`M15-05` adds a data model for custom format profiles. It references the
existing `RuleSetProfile` catalog instead of duplicating Standard/V/Premium
rules.

## Model

`CustomFormatProfile` contains:

- `format_id`
- `display_name`
- `base_rule_set_profile_id`
- `aliases`
- `allowed_pack_ids`
- `notes`

`base_rule_set_profile_id` must resolve through `RuleSetProfileCatalog`.

## Validation

`CustomFormatProfileCatalog.Validate(...)` checks:

- catalog exists
- format ids are present and unique
- base ruleset profile is present and known
- aliases do not collide with other aliases or format ids
- allowed pack ids do not repeat within the same format

Stable rejection reasons:

- `CUSTOM_FORMAT_CATALOG_MISSING`
- `CUSTOM_FORMAT_ID_MISSING`
- `CUSTOM_FORMAT_BASE_RULE_SET_MISSING`
- `CUSTOM_FORMAT_BASE_RULE_SET_UNKNOWN`
- `CUSTOM_FORMAT_DUPLICATE_FORMAT`
- `CUSTOM_FORMAT_DUPLICATE_ALIAS`
- `CUSTOM_FORMAT_DUPLICATE_PACK`

## Boundaries

M15-05 does not:

- enforce custom formats in gameplay
- change deck validation
- enable cross-nation/cross-clan behavior
- modify the core Standard/V/Premium `RuleSetProfile` definitions
- load custom format files from custom packs

## Verification

EditMode tests cover:

- valid catalog references an existing base ruleset
- unknown base ruleset rejection
- duplicate format, alias, and pack rejection
- catalog and validation result JSON round-trip
