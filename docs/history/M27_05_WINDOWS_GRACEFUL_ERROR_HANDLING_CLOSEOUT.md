# M27-05 Windows Graceful Error Handling Closeout

## Status

Complete.

## What Changed

- Added `GracefulErrorMessageFormatter` for recoverable runtime errors and
  unhandled exception messages.
- Updated card pack load failure text to include an explicit retry action.
- Added `WindowsGracefulErrorHandlingVerifier` to check:
  - card pack failure text
  - missing image fallback status
  - unhandled exception formatting
- Added EditMode coverage for formatter behavior and verifier JSON round-trip.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m27_05_graceful_error_handling.log`
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m27_05_graceful_error_handling_r2.xml`
  passed `1127/1127`

Windows player smoke was not rerun because M27-05 only changed failure-state
formatting and did not change the normal Home / Deck Builder / PlayTable flow.

## Next Target

`M27-06`: Integration / PlayMode test.
