# M27-05 Windows Graceful Error Handling Spec

## Goal

Make Windows failure states readable and recoverable instead of silent or
developer-only.

## Scope

M27-05 covers:

- Card database / runtime pack load failure text with an explicit retry action.
- Missing image fallback status with a player-facing tip.
- Unhandled exception formatting that can be surfaced by UI or smoke tooling.

## Runtime Policy

- No mutation of `GameState`.
- No network payload changes.
- No Android/mobile/APK verification.
- Existing fallback image behavior remains in `CardImageCache`; M27-05 verifies
  and formats the visible player message.

## Verification

- `GracefulErrorMessageFormatterTests`
- `WindowsGracefulErrorHandlingVerifierTests`
- Unity compile and EditMode
