# M32-02b Letsplay Layout Research Closeout

Date: 2026-06-28

## Scope

Close the public letsplay research alignment slice for the M32 PlayTable reset.

This was a layout-only research task. It did not implement runtime UI changes.

Included:

- Field and playmat layout.
- Card placement and zone placement.
- Hand strip presentation.
- Phase/turn status.
- Guard, trigger check, drive check, damage check, and contextual action
  surfaces.
- Online table HUD and waiting/status presentation.
- Deck-builder structural references.

Excluded:

- Story scenes.
- Character portraits.
- Dialogue UI.
- Campaign progression.
- Proprietary assets, official icons, playmats, card art, fonts, frames, logos,
  code, or extracted game data.

## Evidence

`yt-dlp` was installed and verified:

- Version: `2026.06.09`
- Executable:
  `C:\Users\Phet\AppData\Roaming\Python\Python314\Scripts\yt-dlp.exe`

Metadata output:

- `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/yt_dlp_metadata/summary.json`

Captured-frame manifest:

- `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/representative_frame_manifest.json`

Research summary:

- `outputs/vanguard_video_game_ux_research/letsplay_research_2026-06-28/LETSPLAY_LAYOUT_RESEARCH_SUMMARY.md`

Indexed public source coverage:

- Dear Days 2 playthrough: 22 entries.
- Dear Days: 100 flat metadata entries.
- Ride to Victory: 100 flat metadata entries.
- Lock on Victory: 11 entries.
- Stride to Victory: 100 flat metadata entries.
- Vanguard EX: 38 entries.
- Cardfight Online: 2 single-video references.

## Decision

The next implementation slice should not be a narrow inspect text-density pass.
It should be a PlayTable de-dashboard field/HUD pass:

- Remove or collapse the permanent right Inspect column.
- Promote field zones, bottom hand strip, phase rail, and contextual action bar.
- Move logs/setup/zone status/bot/online/trigger/ability diagnostics into
  Advanced or collapsible surfaces.
- Keep all gameplay actions routed through existing session/RulesCore paths.
- Keep story, character, dialogue, campaign, copied asset, and extracted data
  scope out of M32.

## Verification

Docs/research-only change.

- Unity compile: not run.
- Unity EditMode: not run.
- Windows player smoke: not run.

No runtime code, data pack, or Unity scene files were changed.
