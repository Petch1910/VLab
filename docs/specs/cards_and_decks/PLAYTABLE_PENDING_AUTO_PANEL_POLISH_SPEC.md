# PlayTable Pending AUTO Panel Polish Spec

## Scope

`M16-01` polishes the first PlayTable pending AUTO surface. It adds a compact
panel summary formatter for the queue state while preserving the existing
diagnostic item list and selection detail surfaces.

This is UI/readability work only. It does not change pending AUTO queue
semantics, network payloads, manual resolution decisions, or `GameState`.

## Behavior

The PlayTable `Pending Ability Summary` surface now uses
`PendingAutoAbilityPanelFormatter`.

The panel summary shows:

- `AUTO queue` header
- payload count
- latest payload id
- queue id
- pending item count
- selected pending item summary, or `none`

Hidden source cards must remain redacted as `source hidden`.

## Boundaries

M16-01 does not:

- resolve pending abilities
- publish or mutate network payloads
- commit pending queues
- change selection semantics
- mutate `GameState`
- reveal hidden source card ids or instance ids

## Verification

EditMode tests cover:

- compact no-payload panel message
- valid queue panel summary
- selected hidden source redaction
- invalid payload panel message
- no payload or selection mutation during formatting
- PlayTable summary surface uses the polished panel formatter
