# PlayTable Manual Resolution Panel Polish Spec

## Scope

`M16-03` polishes the PlayTable pending AUTO manual resolution summary panel.
It adds a compact formatter that combines latest decision payload state,
validation state, and latest apply-preview log state while preserving the
existing decision list, validation detail, and apply-preview log surfaces.

This is UI/readability work only. It does not commit pending queues, resolve
unsupported effects, publish payloads, or mutate `GameState`.

## Behavior

The PlayTable manual resolution summary surface now uses
`PendingAutoAbilityManualResolutionPanelFormatter`.

The panel summary shows:

- `Manual resolution` header
- decision payload count
- latest decision payload id
- compact decision type/index/pending id/player/timing/source
- validation state
- latest apply preview state

Visible sources are shown as card ids only, without source instance ids.
Hidden sources are redacted as `source hidden`.

## Boundaries

M16-03 does not:

- apply manual decisions to live `GameState`
- publish manual decision payloads
- change decision validation rules
- change apply preview semantics
- reveal hidden source card ids or instance ids
- display free-text apply summaries in the compact panel

## Verification

EditMode tests cover:

- compact no-decision panel message
- valid decision panel without source instance id
- hidden decision redaction
- invalid payload rejection summary
- apply preview summary without free-text leak
- no payload/apply-preview mutation during formatting
- PlayTable manual resolution summary uses the polished panel formatter
