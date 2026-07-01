# M32-04 Playmat Slot Layout Closeout

Date: 2026-06-28

## Summary

M32-04 replaced the PlayTable's bar-like rear-guard presentation with a
playmat-style slot skeleton. The field now shows separate opponent/local front
and back RG slots, distinct VG slots, and compact pile markers around the
field. Comparator playmat assets were not imported; the referenced field image
was used only for layout direction.

## Changed

- Added playmat slot constants to `PlayTableZoneFirstLayoutFormatter`.
- Added visible local/opponent RG slot skeletons and VG slot surfaces in
  `PlayTableBootstrap`.
- Moved field markers toward a Vanguard playmat mental model:
  Damage/Order left, Deck/Drop/Bind/Ride/Trigger/Gift right, Soul off the VG
  centerline.
- Changed compact pile surfaces so they do not print overflowing card ids or
  counts into neighboring zones.
- Added EditMode assertions that the runtime PlayTable creates the key slot
  objects.

## Verification

- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m32_04_playmat_slots_r9.xml`
  passed `1179/1179`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m32_04_playmat_slots_r9.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m32_04_playmat_slots_r9.json`
  passed with `blockers=[]`.
- Visual evidence:
  `client/unity/VanguardThaiSim/work/m32_04_playmat_slots_visual_evidence_r9/play_table.png`
  passed capture with no reported visual-evidence issues.

## Remaining Gap

The hand strip still reads too much like compact UI buttons and not enough like
cards in hand. Compact pile markers also need an expanded/overlay interaction
path for manual selection. These are the next target: `M32-05`.
