# PlayTable Trigger Panel Polish Spec

## Scope

`M16-02` polishes the PlayTable trigger panel summary. It adds a compact
formatter that combines trigger log status and current manual draft state while
preserving the existing trigger log and draft diagnostic formatters.

This is UI/readability work only. It does not change trigger RNG outcomes,
trigger replay payloads, trigger allocation, or `GameState`.

## Behavior

The PlayTable trigger log summary surface now uses
`TriggerCheckPanelFormatter`.

The panel summary shows:

- `Trigger panel` header
- trigger log count
- latest trigger check source/index/type/status/modifier count
- current manual trigger draft metadata
- selected card/zone summary

The latest trigger line intentionally avoids checked card ids and instance ids.
Detailed diagnostics remain in the trigger replay payload and existing debug
formatters.

## Boundaries

M16-02 does not:

- perform trigger checks
- choose or replace RNG outcomes
- publish trigger logs
- apply trigger modifiers
- mutate `GameState`
- expose hidden checked-card identities

## Verification

EditMode tests cover:

- compact local no-log panel
- online draft and selected-card summary
- latest trigger log summary without checked-card identity
- invalid payload panel message
- no payload mutation during formatting
- PlayTable trigger summary surface uses the polished panel formatter
