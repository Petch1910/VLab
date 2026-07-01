# M27-08 Windows No Public Release Gate Spec

## Goal

Close the Windows stability pass with an explicit gate that blocks public
release work until the user asks for it.

## Policy

Allowed without further instruction:

- local Unity compile
- Unity EditMode tests
- Unity PlayMode tests
- local Windows build smoke when needed
- docs/status updates

Blocked without explicit user instruction:

- GitHub Release
- public distribution
- release-candidate packaging
- installer packaging
- Android/APK/mobile packaging
- artifact upload for public use

## Verification

Docs-only verification:

- `docs/WINDOWS_LOCAL_BUILD_KNOWN_LIMITATIONS.md` states the release boundary.
- current status docs do not instruct public release work.
- M27 closeout points future agents to wait for explicit user instruction.
