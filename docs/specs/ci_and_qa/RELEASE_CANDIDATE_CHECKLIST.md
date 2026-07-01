# Release Candidate Checklist

## Milestone

`M18-10`

## Status

Ready for local handoff. This checklist does not publish a GitHub Release and
does not upload artifacts.

## Required Verification

| Gate | Status | Evidence |
| --- | --- | --- |
| Python unit tests | Passed | `python -m unittest discover -s tests -p "test_*.py"` passed `44/44` after runtime JSON catalog export and Android install-smoke `--push-pack` tests |
| Ability schema validation | Passed | `python tools\data\validate_ability_schema.py data\packs\vanguard_th\abilities\structured_ability_pack_m12_10.json` passed with `20` abilities, `0` errors, `0` warnings |
| Data validation CI contract | Ready | `.github/workflows/data-validation.yml` covers pack verification, custom pack validation/import smoke, and structured ability validation |
| Unity compile | Passed | `client/unity/VanguardThaiSim/work/unity_compile_m19_09_icon_override_loader.log` has no compiler errors after M19-09 icon override loader work |
| Unity EditMode | Passed | `client/unity/VanguardThaiSim/work/unity_editmode_m19_09_icon_override_loader.xml` shows `892/892` passed |
| Core regression suite | Passed | `CORE_REGRESSION_SUITE_SPEC.md` and `CoreRegressionSuiteReportBuilder` |
| Ability regression suite | Passed | `ABILITY_REGRESSION_SUITE_SPEC.md` and `AbilityRegressionSuiteReportBuilder` |
| Multiplayer payload/no-leak suite | Passed | `MULTIPLAYER_PAYLOAD_NO_LEAK_SUITE_SPEC.md` and `MultiplayerPayloadNoLeakSuiteReportBuilder` |
| Windows build | Passed | `client/unity/VanguardThaiSim/work/windows_build_m19_09_icon_override_loader.log` rebuilt the artifact after M19-09 icon override loader work with `errors=0`, `warnings=0` |
| Android build | Passed | `client/unity/VanguardThaiSim/work/android_build_m19_09_icon_override_loader.log` rebuilt `client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk` with `errors=0`, `warnings=1` |
| Android install smoke | Passed | `tools/smoke/android_install_smoke.py --adb auto --push-pack --force-stop-before-launch --launch --timeout 600` wrote `client/unity/VanguardThaiSim/work/android_install_smoke_m19_09_icon_override_loader.json` with detected package `com.DefaultCompany.VanguardThaiSim`, LDPlayer ADB selected, install/push/force-stop/launch passed, and no blockers |
| Android visual smoke | Passed with image fallback | LDPlayer screenshot `android_ldplayer_m19_09_icon_override_loader_home.png` shows Home pack status `Cards 10836` and semantic trigger badges; images are fallback because the external 2.16 GiB image dataset is not provisioned on Android |
| Runtime UI input | Passed | `ProjectSettings.activeInputHandler` is `Both` so current `StandaloneInputModule` UI receives clicks in player builds |
| Windows player smoke | Passed | Rebuilt `VanguardThaiSim.exe -vanguardPlayerSmoke` wrote `client/unity/VanguardThaiSim/work/player_smoke_m19_09_icon_override_loader.json` with 4 smoke steps and `0` blockers |

## Build Artifacts

Windows:

```text
client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe
```

Windows build log:

```text
client/unity/VanguardThaiSim/work/windows_build_m18_08_a.log
client/unity/VanguardThaiSim/work/windows_build_post_online_ui.log
client/unity/VanguardThaiSim/work/windows_build_taxonomy_refresh.log
client/unity/VanguardThaiSim/work/windows_build_home_lobby_layout_polish.log
client/unity/VanguardThaiSim/work/windows_build_m19_09_icon_override_loader.log
```

Windows runtime pack:

```text
client/unity/VanguardThaiSim/build/windows/latest/data/packs/vanguard_th/manifest.json
client/unity/VanguardThaiSim/build/windows/latest/data/packs/vanguard_th/cards.sqlite
```

Windows player smoke report:

```text
client/unity/VanguardThaiSim/work/player_smoke_manual_test.json
client/unity/VanguardThaiSim/work/player_smoke_post_online_ui.json
client/unity/VanguardThaiSim/work/player_smoke_taxonomy_refresh.json
client/unity/VanguardThaiSim/work/player_smoke_home_lobby_layout_polish.json
client/unity/VanguardThaiSim/work/player_smoke_m19_09_icon_override_loader.json
```

Android:

```text
client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk
```

Android build log:

```text
client/unity/VanguardThaiSim/work/android_build_m18_09_b.log
client/unity/VanguardThaiSim/work/android_build_post_online_ui.log
client/unity/VanguardThaiSim/work/android_build_taxonomy_refresh.log
client/unity/VanguardThaiSim/work/android_build_json_catalog_fallback.log
client/unity/VanguardThaiSim/work/android_build_json_catalog_fallback_b.log
client/unity/VanguardThaiSim/work/android_build_m19_09_icon_override_loader.log
```

Android install-smoke report:

```text
client/unity/VanguardThaiSim/work/android_install_smoke_post_online_ui.json
client/unity/VanguardThaiSim/work/android_install_smoke_taxonomy_refresh.json
client/unity/VanguardThaiSim/work/android_install_smoke_auto_adb.json
client/unity/VanguardThaiSim/work/android_install_smoke_json_catalog_push_launch.json
client/unity/VanguardThaiSim/work/android_install_smoke_json_catalog_push_launch_b.json
client/unity/VanguardThaiSim/work/android_install_smoke_m19_09_icon_override_loader.json
client/unity/VanguardThaiSim/work/android_ldplayer_json_catalog_launch_screenshot.png
client/unity/VanguardThaiSim/work/android_ldplayer_json_catalog_card_browser_screenshot.png
client/unity/VanguardThaiSim/work/android_ldplayer_json_catalog_launch_screenshot_b.png
client/unity/VanguardThaiSim/work/android_ldplayer_json_catalog_card_browser_screenshot_b.png
client/unity/VanguardThaiSim/work/android_ldplayer_m19_09_icon_override_loader_home.png
```

## Known Constraints

- Online play is still trusted-client/casual, not ranked authoritative server
  play.
- Photon custom server is intentionally paused.
- Android APK is a local artifact, not a store-signed Play Store release.
- Android local smoke currently requires `--push-pack` to provision
  `data/packs/vanguard_th` into app external files. The APK does not yet bundle
  the runtime database/catalog internally.
- Android visual smoke uses JSON catalog data and shows image fallback until
  the external 2.16 GiB image dataset is provisioned.
- Full automatic card text parsing is not complete; unsupported structured
  abilities still use manual fallback bridges.
- The Windows artifact bundles the runtime SQLite/card pack metadata, but not
  the full external 2GB card-image dump. Local dev builds can still resolve
  images from the workspace image root when available.
- Runtime UI currently uses UGUI `StandaloneInputModule`; keep Active Input
  Handling set to `Both` until the UI is migrated to `InputSystemUIInputModule`.
- `build/` outputs are local generated artifacts and are ignored by git.
- Unity licensing log lines about unavailable access tokens appeared during
  batchmode verification but builds/tests exited successfully.

## Do Not Do In This Release Candidate

- Do not upload to GitHub Releases without explicit user approval.
- Do not move generated build artifacts into source-controlled directories.
- Do not include the full card image dump in git.
- Do not claim ranked/anti-cheat security.

## Next Steps

- Manual Windows smoke on the generated executable.
- For Android local retest, run `python tools\smoke\android_install_smoke.py --adb auto --push-pack --force-stop-before-launch --launch --timeout 600 --output client\unity\VanguardThaiSim\work\android_install_smoke_m19_09_icon_override_loader.json`.
- Create release notes and package external card/image data only when a
  distribution channel is chosen.
