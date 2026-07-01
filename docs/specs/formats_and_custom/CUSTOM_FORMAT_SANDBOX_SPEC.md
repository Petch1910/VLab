# Custom Format Sandbox Spec

## Scope

`M15-09` adds a read-only sandbox preview for custom format catalogs. The
sandbox answers whether a requested format id or alias can resolve to an
existing base `RuleSetProfile`, and whether an optional pack id is allowed by
that format profile.

This is still a data/validation layer. It does not enforce gameplay, deck
construction, phase rules, card legality, or online room policy.

## Inputs

`CustomFormatSandbox.Preview()` accepts:

- `CustomFormatProfileCatalogDefinition`
- optional `RuleSetProfileCatalogDefinition`
- requested format id or alias
- optional requested pack id

The sandbox clones the format catalog before validation and resolution. It must
not mutate the source catalog, source profile lists, or gameplay state.

## Accepted Result

An accepted `CustomFormatSandboxResult` includes:

- requested format
- resolved `format_id`
- display name
- base `RuleSetProfile` id
- requested pack id
- pack check status
- allowed pack count
- enabled feature names from the resolved base `RuleSetProfile`
- a cloned `RuleSetProfile` summary

The result is JSON round-trip safe with Unity `JsonUtility`.

## Rejections

The sandbox rejects with stable reason codes:

- `CUSTOM_FORMAT_SANDBOX_FORMAT_MISSING`
- `CUSTOM_FORMAT_SANDBOX_CATALOG_INVALID`
- `CUSTOM_FORMAT_SANDBOX_FORMAT_UNKNOWN`
- `CUSTOM_FORMAT_SANDBOX_BASE_RULE_SET_REJECTED`
- `CUSTOM_FORMAT_SANDBOX_PACK_NOT_ALLOWED`

Catalog validation failures also preserve the underlying
`CustomFormatProfileCatalog` rejection reason in `catalog_rejection_reason`.

## Pack Rules

If the requested pack id is empty, no pack check is performed.

If `allowed_pack_ids` is empty, the sandbox treats the format as open to any
pack for preview purposes.

If `allowed_pack_ids` is non-empty, the requested pack id must match one of the
allowed ids after normalization.

## Boundaries

M15-09 does not:

- alter `RulesCore`
- alter `GameState`
- start or block game rooms
- load format catalogs from disk
- enforce deck construction or card legality
- execute Standard, V-Premium, or Premium-specific gameplay modules

## Verification

EditMode tests cover:

- core preset catalog resolves format aliases
- base `RuleSetProfile` features are surfaced
- unknown format rejection does not mutate the source catalog
- disallowed pack rejection does not mutate the source catalog
- invalid catalog rejection preserves the catalog reason
- empty allowed pack list accepts any pack for preview
- sandbox result JSON round-trip
