# Pending Auto Ability Manual Resolution Apply Preview Flow Verification Spec

Status: Implemented in M10-103.

## Purpose

Lock the preview-only flow for pending AUTO manual decisions before adding
committed queue behavior. This verifies the currently supported path end to end:

```text
queue payload
-> manual decision payload
-> decision payload validation
-> apply preview executor
-> session preview log
```

## Flow Contract

1. The session reads the latest pending AUTO queue payload.
2. The session reads the latest manual resolution decision payload.
3. The decision payload is validated before executor use.
4. The executor previews the queue change on a cloned queue.
5. The session appends a safe apply-preview log entry.
6. Accepted previews may store the preview queue payload for later display.

## Safety Rules

- Preview flow must not publish network payloads.
- Preview flow must not mutate `GameState`.
- Preview flow must not write to `GameState.event_log`.
- Preview flow must not expose hidden source card identifiers in log output.
- Rejected preview flow must append a rejected log entry.
- This milestone does not commit `Skip`, `Defer`, or `Resolve` to gameplay
  state. Queue commit policy starts at M10-104.

## Verification

EditMode coverage must verify:

- valid queue and decision payload produce accepted validation, accepted apply
  preview, and accepted session log with matching pending id/decision type,
- invalid decision payload produces rejected validation, rejected preview, and
  rejected session log with the same rejection reason,
- no preview path sends queue/decision payloads over the transport,
- no preview path mutates `GameState` or `GameState.event_log`.
