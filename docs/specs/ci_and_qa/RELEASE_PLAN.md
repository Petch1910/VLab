# Release Plan

## Versioning

Use semantic versioning for app:

```text
0.1.0-card-browser
0.2.0-deck-builder
0.3.0-manual-table
0.4.0-bot-fight
```

Card pack version is separate:

```text
kk-vgth-251
```

## Release Artifacts

- Windows build: `client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe`
- Android APK: `client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk`
- card database pack
- release notes
- verification report

## Pre Release Checklist

- build passes
- M18-08 Windows build runner exits `0` and writes the expected `.exe`
- M18-09 Android build runner exits `0` and writes the expected `.apk`
- M18-10 `RELEASE_CANDIDATE_CHECKLIST.md` is current
- M16-10 client smoke runner passes with no blockers
- data verification passes
- no missing images
- no debug credentials
- changelog updated
- known issues documented

## Distribution

Early builds can be shared manually. Later use GitHub Releases. Large card/image packs should be external storage or separate release assets, not normal repo files.
