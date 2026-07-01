# Trigger Modifier Cleanup Integration Spec

Status: Implemented in M10-109.

## Purpose

Connect committed trigger allocation ledgers to cleanup timing previews without
mutating live `GameState`.

This is the integration bridge between M10-108 trigger allocation commit and the
existing combat modifier cleanup previewer.

## API

```csharp
TriggerModifierCleanupIntegrationResult TriggerModifierCleanupIntegration.Cleanup(
    TriggerAllocationCommitResult commitResult,
    CombatModifierExpiration cleanupTiming)
```

## Behavior

- Accepted commit result + cleanup timing returns a cleanup preview.
- `EndOfBattle` removes only `EndOfBattle` modifiers.
- `EndOfTurn` removes only `EndOfTurn` modifiers.
- `Manual` and `Permanent` modifiers remain unless their exact cleanup timing is
  requested.
- The source committed ledger remains unchanged.

## Rejections

- `TRIGGER_MODIFIER_CLEANUP_COMMIT_RESULT_MISSING`
- `TRIGGER_MODIFIER_CLEANUP_COMMIT_RESULT_REJECTED`
- `TRIGGER_MODIFIER_CLEANUP_LEDGER_MISSING`

## Safety Rules

- Do not mutate source ledger.
- Do not normalize a null modifier list on the source ledger.
- Do not mutate `GameState`.
- Do not append `GameState.event_log`.
- Do not publish network payloads.

## Verification

EditMode coverage verifies:

- EndOfBattle cleanup,
- EndOfTurn cleanup,
- Manual/Permanent modifiers remain under other cleanup timings,
- rejected paths,
- null source modifier list no-normalization,
- source ledger no-mutation.
