# Android Install Smoke Helper Spec

## Status

Implemented after the post-online-ui Android rebuild, extended with ADB
auto-probing after the taxonomy-refresh APK, and extended again with runtime
pack push/force-stop launch support for the JSON catalog Android fallback.

## Purpose

The Android APK build can complete without an emulator, but install smoke needs
an attached ADB device or emulator. This helper makes the device-dependent gate
repeatable and auditable instead of relying on an informal manual command.

## Tool

```text
tools/smoke/android_install_smoke.py
```

Default APK:

```text
client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk
```

Default report:

```text
client/unity/VanguardThaiSim/work/android_install_smoke_report.json
```

Current auto-ADB report:

```text
client/unity/VanguardThaiSim/work/android_install_smoke_auto_adb.json
```

Current LDPlayer install/push/launch report:

```text
client/unity/VanguardThaiSim/work/android_install_smoke_m19_09_icon_override_loader.json
```

## Behavior

The tool:

- verifies the APK exists
- detects the Android package id with `aapt dump badging` when `aapt` is
  available, using `--package` only as an override
- runs `adb devices`
- supports `--adb auto`, which probes common ADB paths such as Google Platform
  Tools and `C:\LDPlayer\LDPlayer9\adb.exe`, then selects the first path that
  reports an active device
- writes a JSON report with `status`, `steps`, `blockers`, device list, and
  bounded command output, package-id detection metadata, and ADB probe results
- returns `waiting` with blocker `NO_ADB_DEVICE` when no attached device is in
  `device` state
- installs with `adb -s <serial> install -r <apk>` when a device is available
- can push `data/packs/vanguard_th` into
  `/sdcard/Android/data/<package>/files/data/packs/vanguard_th` with
  `--push-pack`; this is required for current Android local smoke because the
  APK does not bundle the 16 MB JSON catalog or 166 MB SQLite database
- can optionally launch the detected or supplied package with:
  `adb -s <serial> shell monkey -p <package> 1`
- can force-stop before launch with `--force-stop-before-launch` so a freshly
  pushed runtime pack is picked up by the app process

No Android package name is hard-coded in the helper. The current APK report
detects `com.DefaultCompany.VanguardThaiSim` from the built artifact.

## Commands

No-device audit command:

```powershell
python tools\smoke\android_install_smoke.py `
  --adb auto `
  --output client\unity\VanguardThaiSim\work\android_install_smoke_auto_adb.json
```

Install once LDPlayer/device appears in `adb devices`:

```powershell
python tools\smoke\android_install_smoke.py `
  --adb auto `
  --output client\unity\VanguardThaiSim\work\android_install_smoke_auto_adb.json
```

Optional launch smoke, using the package id detected from the APK:

```powershell
python tools\smoke\android_install_smoke.py `
  --adb auto `
  --launch `
  --output client\unity\VanguardThaiSim\work\android_install_smoke_auto_adb.json
```

Optional launch smoke with an explicit package override:

```powershell
python tools\smoke\android_install_smoke.py `
  --adb auto `
  --package <android.package.id> `
  --launch `
  --output client\unity\VanguardThaiSim\work\android_install_smoke_auto_adb.json
```

Install, push the runtime pack, force-stop, and launch on LDPlayer/device:

```powershell
python tools\smoke\android_install_smoke.py `
  --adb auto `
  --push-pack `
  --force-stop-before-launch `
  --launch `
  --timeout 600 `
  --output client\unity\VanguardThaiSim\work\android_install_smoke_m19_09_icon_override_loader.json
```

## Non-Goals

- no Unity code changes
- no APK rebuild
- no package-id hard-coding
- no ranked/security validation
- no mobile performance profiling

## Verification

- `python -m unittest discover -s tests -p "test_*.py"` covers parser, no-device
  waiting report, missing APK failure, install command, launch command,
  package-required guard, package autodetect, `--adb auto` candidate probing,
  pack push ordering, force-stop-before-launch ordering, and missing pack source
  failures.
- Latest LDPlayer run wrote
  `android_install_smoke_m19_09_icon_override_loader.json` with `status=passed`,
  selected `C:\LDPlayer\LDPlayer9\adb.exe`, installed the APK, pushed seven
  runtime pack files including `card_catalog.json`, force-stopped the package,
  and launched `com.DefaultCompany.VanguardThaiSim`.
