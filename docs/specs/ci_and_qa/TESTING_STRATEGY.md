# Testing Strategy

## Active Verification Profile

The active profile is Windows-only while M20-M27 are in progress. Use
`WINDOWS_ONLY_VERIFICATION_PROFILE.md` as the source of truth.

Allowed when relevant:

- Unity compile for C#/Unity changes.
- Unity EditMode for C#/Unity changes.
- Windows player smoke for runtime flow changes.
- Python tests/validators for data, import, schema, or tooling changes.
- Static docs checks for docs-only changes.

Latest M28-01 Windows gameplay completion verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_01_windows_gameplay_completion.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_01_windows_gameplay_completion_r2.xml`
  passed `1129/1129`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_01_windows_gameplay_completion.xml`
  passed `1/1`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_01_windows_gameplay_completion.log`
  passed with `blockers=[]`; PlayTable smoke now reports
  `Windows gameplay completion smoke passed with 16 committed event(s).`

Latest M28-02 local PlayTable seat toggle verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_02_local_seat_toggle.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_02_local_seat_toggle.xml`
  passed `1131/1131`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_02_local_seat_toggle.xml`
  passed `1/1`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_02_local_seat_toggle.log`
  passed with `blockers=[]`.

Latest M28-03 UI-level two-seat match smoke verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_03_two_seat_playmode_smoke.log`
  has no compiler-error markers.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_03_two_seat_playmode_smoke_r2.xml`
  passed `2/2`.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_03_two_seat_playmode_smoke.xml`
  passed `1131/1131`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_03_two_seat_playmode_smoke.log`
  passed with `blockers=[]`.

Latest M28-05 PlayTable guided next-action verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_05_guided_next_action.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_05_guided_next_action.xml`
  passed `1136/1136`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_05_guided_next_action.xml`
  passed `2/2`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_05_guided_next_action.log`
  passed with `blockers=[]`.

Latest M28-06 Windows built-player smoke verification:

- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m28_06_built_player_smoke.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m28_06_built_player_smoke.json`
  passed with `blockers=[]`.

Latest M28-07 PlayTable action group legend verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_07_action_group_legend.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_07_action_group_legend.xml`
  passed `1137/1137`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_07_action_group_legend.xml`
  passed `2/2`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_07_action_group_legend.log`
  passed with `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m28_07_action_group_legend.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m28_07_action_group_legend.json`
  passed with `blockers=[]`.

Latest M28-09 Bot Plan Advanced drawer verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_09_bot_plan_advanced.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_09_bot_plan_advanced.xml`
  passed `1138/1138`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_09_bot_plan_advanced.xml`
  passed `2/2`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_09_bot_plan_advanced.log`
  passed with `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m28_09_bot_plan_advanced.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m28_09_bot_plan_advanced.json`
  passed with `blockers=[]`.

Latest M21-02 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_02_playtable_board_first_c.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_02_playtable_board_first_c.xml`
  passed `902/902`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_02_playtable_board_first_b.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_02_playtable_board_first_b.json`
  passed with `blockers=[]` and the active Windows layout smoke step.

Latest M21-03 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_03_zone_status.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_03_zone_status.xml`
  passed `904/904`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_03_zone_status.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_03_zone_status.json`
  passed with `blockers=[]`.

Latest M21-04 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_04_hand_preview.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_04_hand_preview.xml`
  passed `917/917`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_04_hand_preview.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_04_hand_preview.json`
  passed with `blockers=[]`.

Latest M21-04b Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_04b_soul_status_ledger.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_04b_soul_status_ledger.xml`
  passed `920/920`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_04b_soul_status_ledger.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_04b_soul_status_ledger.json`
  passed with `blockers=[]`.

Latest M21-04d Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_04d_ride_soul_resource.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_04d_ride_soul_resource.xml`
  passed `926/926`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_04d_ride_soul_resource.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_04d_ride_soul_resource.json`
  passed with `blockers=[]`.

Latest M21-04c Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_04c_board_thumbnail.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_04c_board_thumbnail.xml`
  passed `929/929`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_04c_board_thumbnail.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_04c_board_thumbnail.json`
  passed with `blockers=[]`.

Latest M21-05a Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05_action_availability.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05_action_availability.xml`
  passed `934/934`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05_action_availability.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05_action_availability.json`
  passed with `blockers=[]`.

Latest M21-05a2 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b_check_guard_surface.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b_check_guard_surface.xml`
  passed `937/937`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b_check_guard_surface.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b_check_guard_surface.json`
  passed with `blockers=[]`.

Latest M21-05a3 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05c_trigger_source_split.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05c_trigger_source_split.xml`
  passed `941/941`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05c_trigger_source_split.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05c_trigger_source_split.json`
  passed with `blockers=[]`.

Latest M21-05a4 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05d_attack_vanguard_surface.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05d_attack_vanguard_surface.xml`
  passed `945/945`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05d_attack_vanguard_surface.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05d_attack_vanguard_surface.json`
  passed with `blockers=[]`.

Latest M21-05a5 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05e_attack_target_selection.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05e_attack_target_selection.xml`
  passed `949/949`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05e_attack_target_selection.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05e_attack_target_selection.json`
  passed with `blockers=[]`.

Latest M21-05a6 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05f_battle_flow_status.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05f_battle_flow_status.xml`
  passed `955/955`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05f_battle_flow_status.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05f_battle_flow_status.json`
  passed with `blockers=[]`.

Latest M21-05a7 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05g_manual_note_surface.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05g_manual_note_surface.xml`
  passed `960/960`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05g_manual_note_surface.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05g_manual_note_surface.json`
  passed with `blockers=[]`.

Latest M21-05b1 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b1_setup_readiness.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b1_setup_readiness.xml`
  passed `965/965`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b1_setup_readiness.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b1_setup_readiness.json`
  passed with `blockers=[]`.

Latest M21-05b2 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b2_first_vanguard_setup.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b2_first_vanguard_setup.xml`
  passed `969/969`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b2_first_vanguard_setup.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b2_first_vanguard_setup.json`
  passed with `blockers=[]`.

Latest M21-05b3 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b3_mulligan_selected_r2.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b3_mulligan_selected_r2.xml`
  passed `972/972`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b3_mulligan_selected.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b3_mulligan_selected.json`
  passed with `blockers=[]`.

Latest M21-05b4 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b4_phase_actions.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b4_phase_actions.xml`
  passed `973/973`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b4_phase_actions.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b4_phase_actions.json`
  passed with `blockers=[]`.

Latest M21-05b5 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05b5_setup_guidance.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05b5_setup_guidance.xml`
  passed `981/981`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05b5_setup_guidance.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05b5_setup_guidance.json`
  passed with `blockers=[]`.

Latest M21-06 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_06_advanced_debug_cleanup.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_06_advanced_debug_cleanup_r2.xml`
  passed `981/981`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_06_advanced_debug_cleanup.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_06_advanced_debug_cleanup.json`
  passed with `blockers=[]`.

Latest M21-07 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_07_player_event_log.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_07_player_event_log.xml`
  passed `982/982`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_07_player_event_log.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_07_player_event_log.json`
  passed with `blockers=[]`.

Latest M21-08 PlayTable verification roll-up:

- Roll-up closeout:
  `docs/history/M21_08_TESTS_WINDOWS_SMOKE_CLOSEOUT.md`.
- Evidence source: latest M21-07 compile/EditMode/build/smoke artifacts listed
  above.
- Scope: formatter/layout helper tests, hidden Advanced drawer checks,
  no-direct-mutation coverage, and Windows player smoke evidence for the M21
  PlayTable Windows UX pass.

Latest M21-09 PlayTable UX closeout:

- Closeout:
  `docs/history/M21_09_PLAYTABLE_WINDOWS_UX_CLOSEOUT.md`.
- Evidence source: latest M21-07 compile/EditMode/build/smoke artifacts and
  M21-08 test/smoke roll-up.

Latest M22-01 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_01_player_settings.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_01_player_settings.xml`
  passed `988/988`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m22_01_player_settings.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_01_player_settings.json`
  passed with `blockers=[]`.

Latest M22-02 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_02_deck_appearance.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_02_deck_appearance.xml`
  passed `997/997`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m22_02_deck_appearance.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_02_deck_appearance.json`
  passed with `blockers=[]`.

Latest M22-03 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_03_home_settings.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_03_home_settings.xml`
  passed `1002/1002`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m22_03_home_settings.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_03_home_settings.json`
  passed with `blockers=[]`.

Latest M22-04 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_04_deck_accessories.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_04_deck_accessories.xml`
  passed `1006/1006`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m22_04_deck_accessories.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_04_deck_accessories.json`
  passed with `blockers=[]`.

Latest M22-05 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_05_cosmetic_legality.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_05_cosmetic_legality.xml`
  passed `1008/1008`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m22_05_cosmetic_legality.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_05_cosmetic_legality.json`
  passed with `blockers=[]`.

Latest M22-06 Windows-first verification:

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m22_06_user_deck_assets.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m22_06_user_deck_assets.xml`
  passed `1014/1014`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m22_06_user_deck_assets.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m22_06_user_deck_assets.json`
  passed with `blockers=[]`.

Latest M22-07 Settings / Accessories test roll-up:

- Closeout:
  `docs/history/M22_07_SETTINGS_ACCESSORIES_TEST_ROLLUP_CLOSEOUT.md`.
- Evidence source: latest M22-06 compile/EditMode/build/smoke artifacts listed
  above.
- Scope: JSON round-trip, fallback behavior, and deck validation unchanged by
  cosmetic metadata.

Latest M22 closeout:

- Closeout:
  `docs/history/M22_WINDOWS_SETTINGS_ACCESSORIES_CLOSEOUT.md`.
- Evidence source: latest M22-06 compile/EditMode/build/smoke artifacts and
  M22-07 test roll-up.

Latest M23 Manual / Tutorial:

- Closeout:
  `docs/history/M23_01_MANUAL_CONTENT_SPEC_CLOSEOUT.md`.
- Manual screen closeout:
  `docs/history/M23_02_MANUAL_SCREEN_CLOSEOUT.md`.
- Loading tips closeout:
  `docs/history/M23_03_LOADING_TIPS_CLOSEOUT.md`.
- Original content gate closeout:
  `docs/history/M23_04_ORIGINAL_CONTENT_GATE_CLOSEOUT.md`.
- Manual filter closeout:
  `docs/history/M23_05_MANUAL_FILTER_CLOSEOUT.md`.
- Manual tests closeout:
  `docs/history/M23_06_MANUAL_TESTS_CLOSEOUT.md`.
- M23 closeout:
  `docs/history/M23_MANUAL_TUTORIAL_CLOSEOUT.md`.
- M24-01 closeout:
  `docs/history/M24_01_DECK_BUILDER_WINDOWS_LANDSCAPE_CLOSEOUT.md`.
- M24-02 closeout:
  `docs/history/M24_02_COUNT_LINE_DECK_TEXT_CLOSEOUT.md`.
- M24-03 closeout:
  `docs/history/M24_03_DECK_IMPORT_MISMATCH_UI_CLOSEOUT.md`.
- M24-04 docs-only closeout:
  `docs/history/M24_04_CGS_LIKE_CUSTOM_PACK_ADAPTER_SPEC_CLOSEOUT.md`.
- M24-05 docs-only closeout:
  `docs/history/M24_05_VANGPRO_LIKE_CUSTOM_IMPORT_SPEC_CLOSEOUT.md`.
- M24-06 Python-tool closeout:
  `docs/history/M24_06_LOCAL_CUSTOM_IMPORT_VALIDATOR_CLOSEOUT.md`.
- Latest Python unit verification after M24-06: `53/53`.
- M24-07 Pack Validation UI closeout:
  `docs/history/M24_07_PACK_VALIDATION_UI_CLOSEOUT.md`.
- M24-08 Deck image export closeout:
  `docs/history/M24_08_DECK_IMAGE_EXPORT_CLOSEOUT.md`.
- Latest Unity verification after M24-08: EditMode `1051/1051`.
- Latest Windows build/smoke after M24-08: build passed and Windows player smoke
  `blockers=[]`.
- M24-09 custom import workflow test rollup closeout:
  `docs/history/M24_09_CUSTOM_IMPORT_WORKFLOW_TEST_ROLLUP_CLOSEOUT.md`.
- Latest Python unit verification after M24-09: `55/55`.
- M24 closeout:
  `docs/history/M24_DECK_BUILDER_IMPORT_CUSTOM_PACK_UX_CLOSEOUT.md`.
- M25-01 Photon trusted-client room policy closeout:
  `docs/history/M25_01_PHOTON_TRUSTED_CLIENT_ROOM_POLICY_CLOSEOUT.md`.
- Latest Unity verification after M25-01: EditMode `1054/1054`.
- Latest Windows build/smoke after M25-01: build passed and Windows player smoke
  `blockers=[]`.
- M25-02 lobby flow closeout:
  `docs/history/M25_02_LOBBY_FLOW_CLOSEOUT.md`.
- Latest Unity verification after M25-02: EditMode `1061/1061`.
- Latest Windows build/smoke after M25-02: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M25-03 room status closeout:
  `docs/history/M25_03_ROOM_STATUS_CLOSEOUT.md`.
- Latest Unity verification after M25-03: EditMode `1063/1063`.
- Latest Windows build/smoke after M25-03: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M25-04 reconnect UX closeout:
  `docs/history/M25_04_RECONNECT_UX_CLOSEOUT.md`.
- Latest Unity verification after M25-04: EditMode `1066/1066`.
- Latest Windows build/smoke after M25-04: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M25-05 Online PlayTable default UI closeout:
  `docs/history/M25_05_ONLINE_PLAYTABLE_DEFAULT_UI_CLOSEOUT.md`.
- Latest Unity verification after M25-05: EditMode `1067/1067`.
- Latest Windows build/smoke after M25-05: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M25-06 replay sync/status closeout:
  `docs/history/M25_06_REPLAY_SYNC_STATUS_CLOSEOUT.md`.
- Latest Unity verification after M25-06: EditMode `1071/1071`.
- Latest Windows build/smoke after M25-06: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M25-07 online room test rollup closeout:
  `docs/history/M25_07_ONLINE_ROOM_TEST_ROLLUP_CLOSEOUT.md`.
- Latest Unity verification after M25-07: EditMode `1074/1074`.
- Latest Windows build/smoke after M25-07: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M25 Windows Online Room usability closeout:
  `docs/history/M25_WINDOWS_ONLINE_ROOM_USABILITY_CLOSEOUT.md`.
- Latest Unity verification after M25-08: EditMode `1078/1078`.
- Latest Windows build/smoke after M25-08: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M26-01 bot/automation return audit closeout:
  `docs/history/M26_01_BOT_AUTOMATION_RETURN_AUDIT_CLOSEOUT.md`.
- Latest Unity verification after M26-01: EditMode `1082/1082`.
- Latest Windows build/smoke after M26-01: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M26-02 bot legal-action/masked-state gate closeout:
  `docs/history/M26_02_BOT_LEGAL_ACTION_MASKED_STATE_GATE_CLOSEOUT.md`.
- Latest Unity verification after M26-02: EditMode `1086/1086`.
- Latest Windows build/smoke after M26-02: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M26-03 bot explanation panel closeout:
  `docs/history/M26_03_BOT_EXPLANATION_PANEL_CLOSEOUT.md`.
- Latest Unity verification after M26-03: EditMode `1089/1089`.
- Latest Windows build/smoke after M26-03: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M26-04 structured ability template gate closeout:
  `docs/history/M26_04_STRUCTURED_ABILITY_TEMPLATE_GATE_CLOSEOUT.md`.
- Latest Unity verification after M26-04: EditMode `1098/1098`.
- Latest Windows build/smoke after M26-04: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M26-05 live effect no text parsing gate closeout:
  `docs/history/M26_05_LIVE_EFFECT_NO_TEXT_PARSING_GATE_CLOSEOUT.md`.
- Latest Unity verification after M26-05: EditMode `1106/1106`.
- Latest Windows build/smoke after M26-05: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M26-06 Solo Play Home entry flow closeout:
  `docs/history/M26_06_SOLO_PLAY_HOME_ENTRY_FLOW_CLOSEOUT.md`.
- Latest Unity verification after M26-06: EditMode `1115/1115`.
- Latest Windows build/smoke after M26-06: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke `blockers=[]`.
- M26-07 bot automation safety regression gate closeout:
  `docs/history/M26_07_BOT_AUTOMATION_SAFETY_REGRESSION_CLOSEOUT.md`.
- Latest Unity verification after M26-07: EditMode `1119/1119`.
- Windows player smoke was not rerun for M26-07 because it added a pure
  regression gate and no player-facing runtime flow.
- M26-08 bot automation return gate closeout:
  `docs/history/M26_BOT_AUTOMATION_RETURN_GATE_CLOSEOUT.md`.
- M26-08 was docs-only; latest runtime verification remains M26-07 Unity
  compile/EditMode and M26-06 Windows build/player smoke.
- M27-01 Windows stability smoke closeout:
  `docs/history/M27_01_WINDOWS_STABILITY_SMOKE_CLOSEOUT.md`.
- Latest Unity verification after M27-01: EditMode `1119/1119`.
- Latest Windows build/smoke after M27-01: build passed with `errors=0`,
  `warnings=0`, and Windows player smoke passed 8 steps with `blockers=[]`.
- M27-02 Windows smoke blocker review closeout:
  `docs/history/M27_02_WINDOWS_SMOKE_BLOCKER_REVIEW_CLOSEOUT.md`.
- M27-02 was docs-only; it reviewed the M27-01 smoke JSON and found
  `blockers=0`.
- M27-03 Windows performance baseline closeout:
  `docs/history/M27_03_WINDOWS_PERFORMANCE_BASELINE_CLOSEOUT.md`.
- Latest Unity verification after M27-03: EditMode `1121/1121`.
- Baseline artifact:
  `client/unity/VanguardThaiSim/work/windows_performance_baseline_m27_03.json`
  with repository load `36ms`, query `24ms`, detail `5ms`, deck validation
  `99ms`, deck-code round-trip `17ms`, and `blockers=[]`.
- M27-04 Windows performance gate closeout:
  `docs/history/M27_04_WINDOWS_PERFORMANCE_GATE_CLOSEOUT.md`.
- Latest Unity verification after M27-04: EditMode `1123/1123`.
- Gate artifact:
  `client/unity/VanguardThaiSim/work/windows_performance_gate_m27_04.json`
  accepted with `blockers=[]`, cache retention `4/4` thumbnails and `2/2`
  full images, cache clear memory `true`, and headless average `149.75ms`.
- M27-05 Windows graceful error handling closeout:
  `docs/history/M27_05_WINDOWS_GRACEFUL_ERROR_HANDLING_CLOSEOUT.md`.
- Latest Unity verification after M27-05: EditMode `1127/1127`.
- M27-05 added recoverable error formatting, card-pack retry text, missing
  image fallback verification, and unhandled-exception player-facing messages.
- M27-06 Windows PlayMode integration closeout:
  `docs/history/M27_06_WINDOWS_PLAYMODE_INTEGRATION_CLOSEOUT.md`.
- Latest Unity verification after M27-06: compile passed, EditMode
  `1127/1127`, and PlayMode `1/1`.
- The first PlayMode test covers Home -> smoke deck -> PlayTable -> Stand ->
  Draw -> Ride phase -> End phase through runtime buttons.
- M27-07 Windows known limitations closeout:
  `docs/history/M27_07_WINDOWS_KNOWN_LIMITATIONS_CLOSEOUT.md`.
- M27-07 was docs-only; latest runtime verification remains M27-06 compile,
  EditMode `1127/1127`, and PlayMode `1/1`.
- M27-08 no public release gate closeout:
  `docs/history/M27_08_NO_PUBLIC_RELEASE_GATE_CLOSEOUT.md`.
- M27-08 was docs-only; latest runtime verification remains M27-06 compile,
  EditMode `1127/1127`, and PlayMode `1/1`.

Deferred until explicit user instruction:

- Android build.
- APK generation.
- LDPlayer/ADB/emulator smoke.
- Android/mobile layout QA.
- App packaging.
- Release-candidate packaging.
- Public distribution checks.

## Required Baseline Commands

Run when data or Python tooling changes:

```powershell
python tools\verification\verify_vanguard_th_pack.py
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack_v2
python tools\data\import_custom_pack.py data\templates\custom_pack --output-dir work\custom_pack_import --overwrite
python tools\data\import_custom_pack.py data\templates\custom_pack_v2 --output-dir work\custom_pack_v2_import --overwrite
python -m unittest discover -s tests -p "test_*.py"
```

Run Unity compile/EditMode tests when Unity C# logic changes.

## Test Layers

### Data Tests

- card count
- image count
- missing image path
- duplicate card id
- invalid required fields
- pack manifest/version/hash
- custom pack source metadata, v1/v2 schemas, required columns, safe image
  paths, duplicate ids, capabilities, dependency metadata, and deterministic
  hashes

### Deck Tests

- valid deck
- too few or too many cards
- too many copies
- invalid ride deck
- missing card id
- wrong format/rule set
- deck code round-trip

### RulesCore Tests

- every command validates before mutation
- invalid command is rejected without state corruption
- UI/bot paths use legal actions
- event log is emitted for every mutation
- replay reaches the same state as live execution
- snapshot/rollback restores state exactly

### Phase And Battle Window Tests

- timing/window audit report includes current phase/action/timing enums,
  pending AUTO timing strings, trigger check sources, modifier expiration
  timings, known M11 gaps, JSON round-trip, and duplicate detection
- phase/timing matrix includes typed action-to-window mappings,
  representative legal/rejected game action, trigger check, and cleanup
  combinations, JSON round-trip, and duplicate detection
- RulesCore command facade coverage report marks all current `GameActionType`
  commands as generator/executor/matcher covered, keeps `UndoLast` as the
  explicit exception, round-trips JSON, and rejects duplicate command ids
- legal action mask usage report covers UI, bot, session, and ability-core
  paths, marks hardened paths, preserves `PlayTable.Undo` as the only direct
  mutation exception, round-trips JSON, and rejects duplicate path ids
- reject no-mutation tests cover null/illegal RulesCore commands, accepted
  mutation detection, pending AUTO queue commit rejects, null pending-list
  no-normalization, and missing-payload session publish history preservation
- event-sourcing coverage tests verify every current `GameActionType` mutation
  creates a `GameEvent`, appends to `GameState.event_log`, and is replayable
  through `GameEventReducer`; `UndoLast` and pending AUTO timing commit remain
  explicit exceptions
- replay determinism verifier tests cover supported RulesCore command replay,
  divergent final-state rejection, unsupported event rejection, source
  state/event no-mutation, result JSON round-trip, and missing-state rejection
- snapshot rollback verifier tests cover branch-only mutation, illegal branch
  rejection, missing input rejection, restore independence after later live
  mutation, and result JSON round-trip
- hidden-state view hardening tests cover current player/spectator masking
  policy, source no-mutation, direct private-zone/event masking, missing input
  rejection, and result JSON round-trip
- resource ledger tests cover CounterBlast derivation from face-up damage,
  accepted CB/SB/Energy costs, unavailable costs, duplicate once flags,
  negative costs, player mismatch, missing inputs, source no-mutation, and JSON
  round-trip
- RuleSet profile tests cover Standard/V-Premium/Premium flag separation,
  state-based resolution no-mutation, duplicate profile/alias rejection,
  catalog/resolution JSON round-trip, and missing/unknown format rejection
- core regression suite tests cover the required M18-05 inventory categories,
  reject missing categories, reject empty representative test coverage, and
  round-trip the report JSON
- ability regression suite tests cover required schema, validator, runtime
  registry, cost/target/effect/modifier, fixture DSL, pack smoke, manual
  fallback, and custom-pack ability metadata categories; reject missing/empty
  categories; and round-trip the report JSON
- multiplayer payload/no-leak suite tests cover required command envelope,
  owner-private room, public event masking, reconnect, spectator replay,
  trigger-check payload, pending AUTO payload, manual resolution decision
  payload, and session-storage no-mutation categories; reject missing/empty
  categories; and round-trip the report JSON
- ride only in legal phase
- call only to legal circles
- first-player attack restriction follows RuleSet
- attack declaration opens guard window
- guard step closes before drive/damage resolution
- end phase cleanup removes temporary effects

### Hidden Information Tests

- `GameStateViewFactory` keeps true state intact while producing masked clones
- player view can see only the viewer's private hand/ride deck
- spectator view cannot see either player's private hand/ride deck
- deck contents stay hidden even from the deck owner
- face-down cards in public zones are masked
- event log card instance ids are masked when private-zone events would leak card ids
- play table display state uses a masked player view while command execution keeps true state
- bot cannot see opponent hand
- bot cannot see opponent top deck
- known revealed cards remain known when rules allow
- unknown draws remain unknown
- observer/replay views mask private data when required

### RNG And Probability Tests

- seeded RNG reproduces results
- live RNG outcome is logged or replayable
- simulation RNG does not affect live RNG
- exact probability engine does not reveal concrete top-deck identity
- probability estimates are never applied as actual trigger outcomes
- trigger probability engine tests cover one-check, twin-drive, zero-trigger,
  invalid-count, and order-independent card pool calculations

### AbilityCore Tests

- cost validation before payment
- cost payment is transactional
- target validation at resolution time
- simultaneous AUTO abilities enter pending queue
- continuous effects recalculate or invalidate correctly
- once-per-turn and once-per-fight tracking works
- unsupported abilities can fall back to manual resolution

### Power / Trigger Tests

- attack power plus guard shield math is correct
- drive check critical trigger allocation is correct
- drive check draw/front/heal/over trigger behavior follows RuleSet
- damage trigger modifies power correctly
- temporary power/critical cleanup timing is correct
- trigger resolver scaffold returns deterministic structured results for
  critical, draw, front, heal, over, none, and unknown trigger categories
- trigger allocation planner suggests visible targets for power/critical/front
  bonuses, emits draw/heal/over notes, skips hidden units, and does not mutate
  state
- combat modifier ledger summarizes power/critical deltas separately, filters
  expiration timings without mutating the original ledger, and round-trips JSON
- trigger allocation modifier adapter converts power/critical target plans into
  ledger entries, preserves side-effect notes, handles manual plans, and does
  not mutate state
- combat stat projection combines visible unit `power_delta` with ledger
  summaries for one target, rejects missing/hidden targets, round-trips JSON,
  and does not mutate state
- combat modifier cleanup preview reports expired/remaining modifiers by timing,
  returns a remaining ledger, round-trips JSON, is deterministic, and does not
  mutate the original ledger or `GameState`
- trigger check resolution bundle combines check metadata, resolver output,
  allocation plan, and combat modifier ledger, round-trips JSON, is
  deterministic, and does not mutate `GameState`
- trigger check log entries derive from bundles, preserve modifier ids and
  notes, round-trip JSON, are deterministic, and do not mutate `GameState`
- trigger check replay logs append cloned entries in order, round-trip JSON,
  provide cloned visible prefixes, are deterministic, and do not mutate
  `GameState`
- trigger check replay log masking redacts checked card ids, checked instance
  ids, log entry ids, summaries, and modifier ids for spectator/opponent-safe
  views while preserving owner/true-state views and not mutating source logs
- trigger check replay payload codec wraps already-masked logs with protocol,
  room, sender, perspective, and source-log metadata; rejects wrong protocol or
  invalid log JSON; is deterministic; and does not mutate source logs
- trigger check Photon payload wrapper reserves an event code, round-trips the
  trigger check replay payload, rejects wrong event codes and inner protocol
  mismatches, is deterministic, and does not mutate source logs
- trigger check transport hook tests verify fake transport send/receive, Photon
  adapter decode dispatch, null-payload rejection, and no `GameState` mutation
- trigger check session storage tests verify received trigger check replay
  payloads are retained separately from `GameState.event_log`, null payloads are
  ignored, normal game-event sync still works, and `GameState` is not mutated by
  trigger-log receipt
- trigger check PlayTable UI tests verify online trigger-log count/latest
  summary display, local zero-log display, and no `GameState` mutation while
  rendering received trigger diagnostics
- pending AUTO ability manual resolution decision draft factory tests verify
  `Resolve`, `Skip`, and `Defer` payload creation, missing/invalid rejection,
  hidden-source redaction, source request immutability, and no `GameState`
  mutation
- pending AUTO ability manual resolution decision PlayTable draft tests verify
  online `DraftDec` stores a decision payload without publishing, local tables
  do not expose the draft control, hidden-source safety is preserved through
  session-created drafts, and `GameState` is not mutated
- pending AUTO ability manual resolution decision type selector tests verify
  deterministic `Resolve`/`Skip`/`Defer` cycling, invalid-state fallback,
  online-only PlayTable selector exposure, selected-type draft creation, and no
  `GameState` mutation
- pending AUTO ability manual resolution decision draft result formatter tests
  verify accepted, rejected, and null result status text; PlayTable `DraftDec`
  uses the formatter while preserving no-publish and no-`GameState` mutation
- pending AUTO ability manual resolution decision validator tests verify valid
  payload acceptance, invalid payload rejection, unsupported decision type
  rejection, missing pending id rejection, hidden-source redaction in validation
  output, and source payload immutability
- pending AUTO ability manual resolution decision validation result formatter
  tests verify accepted/rejected/null formatting and hidden-source redaction
- pending AUTO ability manual resolution decision PlayTable validation preview
  tests verify local fallback, online valid/invalid preview, hidden-source
  redaction, and no `GameState` mutation
- pending AUTO ability manual resolution apply command contract tests verify
  command/result JSON round-trip, stable rejection reason constants, and
  contract-only behavior before queue mutation exists
- pending AUTO ability manual resolution apply command validator tests verify
  accepted validation, missing queue, missing decision, pending id mismatch,
  unsupported decision type, and source queue/decision immutability
- pending AUTO ability manual resolution apply result formatter tests verify
  accepted/rejected/null formatting before queue mutation is wired into UI
- pending AUTO ability manual resolution apply executor tests verify skip,
  defer, accepted/manual resolve, validator rejection path, and source
  queue/decision immutability
- pending AUTO ability manual resolution session apply preview tests verify
  missing queue/decision rejection, accepted preview queue storage, no transport
  send, and no `GameState` mutation
- pending AUTO ability manual resolution PlayTable apply preview tests verify
  online `ApplyDec` preview, local control absence, missing payload rejection
  status, no transport send, and no `GameState` mutation

### Bot Tests

- bot only chooses legal actions
- bot uses masked observation
- bot cannot mutate state directly
- bot can complete a simple scripted turn/game
- deterministic seed reproduces bot choices
- board/resource evaluator is deterministic, handles hidden/missing card stats,
  and reports explanation terms without mutating state
- battle sequence search is deterministic, skips hidden attackers, uses trigger
  probability as planning signal only, and does not mutate state
- opponent guard estimator treats masked hand cards as unknown shield, uses
  visible card shield only when available, and does not mutate state
- bot playbook matching uses visible public ids, falls back to balanced defaults,
  preserves priority ordering, and round-trips through JSON
- combo discovery scaffold combines playbook, board, guard, trigger, and battle
  search outputs into deterministic JSON without mutating state
- heuristic bot v2 returns an executable legal action, does not mutate source
  state, prefers empty-vanguard setup, masks simulated draw cards before score
  evaluation, avoids private id leaks in reason strings, and is deterministic
- guard decision bot recommends guard/no-guard/perfect-guard/cannot-guard from
  visible shield, damage risk, and exact trigger risk without mutating state or
  leaking opponent private ids
- trigger-risk attack choice uses exact probability as a planning-only signal,
  can change attack ordering, never applies trigger outcome, skips hidden
  attackers, and does not mutate state
- battle sequence search v2 adds guard-pressure and trigger-risk scoring,
  returns cloned visible attackers, skips hidden attackers, and does not mutate
  state
- snapshot simulation path applies legal action sequences to cloned branch state
  only, returns branch JSON, reports rejected actions, and does not mutate live
  state
- playbook integration adds deterministic priority-call and aggro phase bias
  from visible playbook matches without mutating state or leaking private ids
- offline combo discovery output includes combo lines and replay references,
  round-trips JSON, remains deterministic, and does not mutate state
- bot debug trace includes selected action/ranked lines, round-trips JSON,
  remains deterministic, and avoids private id leaks without mutating state
- ISMCTS readiness gate allows advanced prototype only when every required
  checklist item is ready

### Simulation Tests

- cloned state can branch without affecting live state
- Monte Carlo/virtual checks use separate RNG
- simulated event logs are isolated from live event logs

### Multiplayer Protocol Tests

- room readiness rejects pack/version/hash mismatch
- network envelopes serialize the same `GameEvent` used by replay
- event index and previous event id prevent gaps and branch conflicts
- reconnect batch rebuilds a replay from missing events
- mock room syncs a published action into a second client state
- disconnected or mismatched-pack clients cannot publish game events
- Photon Realtime payloads round-trip room state and network event envelopes
- Photon Realtime payloads round-trip reconnect request and reconnect batches
- Photon transport reports SDK bridge availability and rejects missing AppId
- Photon transport dispatches decoded reconnect request/batch payloads
- Photon live smoke runner can create/join a room and deliver one event when
  `VANGUARD_PHOTON_APP_ID` is configured
- Photon lobby smoke runner can connect two controllers, host/join a room,
  propagate room state, send a reconnect request, and deliver a reconnect batch
- multiplayer game-session controller publishes local legal actions, applies
  remote envelopes, creates reconnect batches, auto-responds to reconnect
  requests, and applies received batches
- Photon game-session smoke runner can connect two controllers, host/join a
  room, sync a host action into guest state, and deliver a reconnect batch when
  `VANGUARD_PHOTON_APP_ID` is configured
- online PlayTable mode routes UI button actions through
  `MultiplayerGameSessionController` instead of mutating local state directly
- lobby deck exchange includes local deck code, builds an initial `GameState`
  from host/guest deck codes and room seed, and preserves guest local player
  index as player 1
- lobby-started host and guest sessions use the same deterministic room
  `game_id` so network envelopes do not fail `GAME_ID_MISMATCH`
- lobby Start Table applies compatible pending reconnect batches and rejects
  incompatible event cursor gaps before opening the online table
- deck privacy commitment tests verify canonical deck hashing, commitment
  sensitivity to card/nonce/room/pack changes, and ranked/public room rejection
  when shared deck codes are used
- deck reveal verifier tests accept the original committed deck and reject
  swapped decks or missing reveal nonces
- lobby start tests reject public/ranked shared-deck-code rooms and reject
  non-shared privacy modes until private event payloads or a validator exist
- public event payload tests verify hidden draw events carry count deltas without
  private card ids, and reveal events preserve public card id/proof through the
  Photon codec
- public event factory tests verify true `GameEvent` records become masked
  public events for private draws, reveal card identity when a card enters a
  public zone, and keep non-card actions free of card identity fields
- public replay tests verify masked public replay JSON does not leak private
  draw card ids or true instance ids, and public replay stepping exposes only
  the visible event prefix without reconstructing hidden state
- public delivery tests verify non-shared privacy sessions publish public
  events instead of true envelopes, received public events do not mutate true
  state, and Photon adapter dispatches event-code 5 payloads
- reveal proof tests verify commitment-bound public reveal events get proof
  metadata, unchanged payloads verify, and tampered public card ids are rejected
- deck privacy gameplay policy tests verify server-held rooms require a
  validator and commitment-only rooms remain blocked with an explicit
  client-trust policy reason
- deck reveal request/response tests verify Photon payload round trips, adapter
  dispatch, lobby request sending, response verification, and revealed deck
  code plus nonce matching the stored commitment
- lobby reveal UI tests verify reveal target/nonce inputs plus Request Reveal
  and Send Reveal buttons are created in the runtime lobby surface
- Post-M19 Photon lobby UI tests verify Connection, Room, Safety/Reveal,
  Back Home, Reconnect, and Start Table controls are created with injected fake
  transport; formatter tests verify player-facing status text does not expose
  deck codes or revealed deck codes
- Post-M19 reconnect flow tests verify stale lobby reconnect request/batch state
  clears across join/disconnect, pre-room batches are ignored, and handoff text
  distinguishes event `0` batches from non-zero cursor batches
- Android install-smoke helper tests verify `adb devices` parsing, no-device
  waiting report, missing APK failure, install command construction, optional
  launch command, package autodetection from `aapt dump badging`,
  `--adb auto` candidate selection/probe reporting, package-required guard,
  runtime pack `--push-pack`, `--force-stop-before-launch` ordering, and
  missing pack source failures without invoking real ADB
- runtime JSON catalog tests verify the SQLite-to-catalog exporter and Unity
  `JsonCardRepository` query/detail/nation-filter behavior for Android/mobile
  fallback
- Vanguard Area-style taxonomy tests verify classic clan ordering is not driven
  by raw card counts, D-era nations become nation filters, and null inputs stay
  safe
- M19-09 user icon override tests verify semantic trigger/marker/zone defaults,
  existing `icons` object manifest parsing, missing private icon fallback,
  user icon resolution when a file exists, unknown-key ignore behavior, and
  path traversal rejection
- Android refresh after M19-09 verifies the rebuilt APK installs on LDPlayer,
  pushes `data/packs/vanguard_th`, force-stops, launches, and reaches Home with
  pack status visible in `android_ldplayer_m19_09_icon_override_loader_home.png`
- replay player can return player/spectator masked views for shared replay
  surfaces
- lobby controller host/join/reconnect paths use fake transport in EditMode
- lobby UI can be created with injected fake transport without live Photon
- trigger check manual publish controls are covered in EditMode for online
  button creation, disabled/no-payload behavior, local exclusion, deterministic
  payload sending, and no `GameState` mutation
- manual trigger-check draft payload creation is covered in EditMode for
  spectator masking, owner-player reveal, deterministic payload JSON, missing
  input rejection, unknown-trigger manual-resolution payloads, and no
  `GameState` mutation
- trigger check draft session publishing is covered in EditMode for session
  room/sender context filling, valid send/store behavior, invalid-input no-send
  behavior, deterministic sent payload JSON, and no `GameState` mutation
- trigger check draft PlayTable controls are covered in EditMode for online
  disabled-before-selection behavior, selected-card manual draft publishing,
  masked `Unknown` trigger payloads, local exclusion, and no `GameState`
  mutation
- trigger check draft type selector is covered in EditMode for default
  `Unknown` UI state, cycling to `Critical`, sent payload trigger type, local
  exclusion, and no `GameState` mutation
- trigger check draft source selector is covered in EditMode for default
  `Manual` UI state, cycling to `Drive`, sent payload check source, local
  exclusion, and no `GameState` mutation
- trigger check draft index selector is covered in EditMode for default `Idx 0`,
  cycling to `Idx 1`, sent payload check index, local exclusion, and no
  `GameState` mutation
- trigger check draft summary panel is covered in EditMode for default online
  summary, deterministic updates after type/source/index cycling, no-send
  behavior, local-mode summary, and no `GameState` mutation
- trigger check draft selected-card summary is covered in EditMode for
  no-selection `card none`, selected visible card id display, no-send behavior,
  local-mode summary, and no `GameState` mutation
- trigger check draft selected-zone summary is covered in EditMode for
  no-selection `zone none`, selected card zone display, no-send behavior, and
  no `GameState` mutation
- trigger check draft clear-selection control is covered in EditMode for
  disabled-before-selection behavior, clearing selected card context, disabling
  `DraftTrig`, resetting summary card/zone metadata, no-send behavior, and no
  `GameState` mutation
- trigger check draft selected-card status helper is covered in EditMode for
  none, visible card, hidden card, missing selected-card instance zone labels,
  long id shortening, and PlayTable summary reuse without behavior change
- trigger check draft metadata formatter is covered in EditMode for default
  metadata, cycled metadata, selector button labels, trigger type short labels,
  and PlayTable summary behavior remaining unchanged
- trigger check draft full summary formatter is covered in EditMode for local
  mode, online default summary, online selected-card status, online metadata
  updates, and PlayTable summary behavior remaining unchanged
- trigger check draft control-state helper is covered in EditMode for local
  mode, online no-selection, online visible-card selection, hidden-card
  selection, missing card id, publish-log availability, and PlayTable button
  behavior remaining unchanged
- trigger check draft selection validation helper is covered in EditMode for
  local mode, no selection, hidden card identity, visible-card accepted
  selection, and PlayTable publish rejection behavior remaining unchanged
- trigger check draft publish result message formatter is covered in EditMode
  for successful publish, rejection reason priority, transport error code
  fallback, transport message fallback, null/empty results, and PlayTable
  publish result behavior remaining unchanged
- trigger check draft selector cycle helper is covered in EditMode for the
  full trigger type cycle, check source cycle, check index wrapping,
  negative-index normalization, and PlayTable selector behavior remaining
  unchanged
- trigger check draft status message formatter is covered in EditMode for
  trigger type, check source, check index, and clear-selection messages while
  preserving PlayTable user-facing text
- trigger check draft request factory is covered in EditMode for copied
  player/check/card/trigger fields, fixed spectator/end-of-turn defaults, and
  nullable selected-card ids remaining available for caller validation
- trigger check log publish result formatter is covered in EditMode for the
  existing success message, existing transport failure text, null result
  handling, and empty non-null transport result behavior
- trigger check log summary formatter is covered in EditMode for no logs,
  invalid payloads, empty decoded logs, existing latest summaries, and fallback
  check/source/type text while preserving PlayTable side-panel text
- trigger check log publish online validation helper is covered in EditMode for
  offline/local rejection with the existing message and online acceptance
- PlayTable mode summary formatter is covered in EditMode for local text,
  online status/transport/event/trigger-log count text, reconnect sync text,
  enum status values, and null status/transport fallback behavior
- PlayTable card selection status formatter is covered in EditMode for
  select-first, selected-card, no-card-selected, and null-card-id fallback text
- PlayTable action status formatter is covered in EditMode for draw fallback,
  phase fallback, gift marker fallback, and online undo-disabled text
- PlayTable event log formatter is covered in EditMode for empty logs,
  move-card lines, gift marker lines, phase fallback lines, newest-first
  ordering, and max-entry trimming
- pending auto ability queue model is covered in EditMode for enqueue ordering,
  non-mutating clones, peek/dequeue/clear behavior, null-safe list
  initialization, and JSON round-trip preservation
- pending auto ability GameState integration is covered in EditMode for
  `EnsureLists`, `FromJson`, and `GameState` JSON round-trip preservation
- ability trigger event collector is covered in EditMode for timing-event
  matching, non-matching registrations, deterministic pending ids, copied
  source queues, null inputs, and known `GameActionType` timing mappings
- ability trigger GameState adapter is covered in EditMode for collecting from
  an existing state queue, null state/default queue behavior, non-matching
  registrations, and no source `GameState` mutation
- pending AUTO real timing event integration is covered in EditMode for
  RulesCore-produced timing event collection, unmatched no-op behavior, source
  queue copy-before-assignment, missing-input rejection, no network publish,
  and no `GameState.event_log` append
- pending AUTO manual-resolved event metadata is covered in EditMode for
  accepted manual `Resolve` event creation, JSON round-trip,
  skipped/deferred/rejected paths, missing/mismatched inputs, hidden-source-safe
  ids/reasons, and source queue/decision/commit result no-mutation
- pending auto ability queue masking is covered in EditMode for true-state
  identity preservation, owner-player preservation plus opponent masking,
  spectator masking, deterministic output, null source, and no source mutation
- pending auto ability queue payload codec is covered in EditMode for masked
  queue round-trip, protocol rejection, empty/invalid queue JSON rejection,
  deterministic encoding, source mutation protection, and null pending list
  preservation
- pending auto ability queue Photon wrapper is covered in EditMode for wrapper
  round-trip, wrong event code rejection, inner protocol rejection,
  deterministic wrapper JSON, and source payload mutation protection
- pending auto ability queue transport hook is covered in EditMode for fake
  transport send/receive, Photon adapter decoded dispatch, null-payload
  rejection, and no `GameState` mutation
- pending auto ability queue session storage is covered in EditMode for receipt
  storage outside `GameState`, null-payload ignore, normal game-event sync
  continuity after receipt, and no `GameState` mutation
- pending auto ability PlayTable summary is covered in EditMode for formatter
  zero/invalid/empty/latest output, local zero display, online received payload
  count/latest display, and no `GameState` mutation while rendering
- pending auto ability queue session publishing is covered in EditMode for
  no-payload rejection, latest-payload send, stored payload preservation, and
  no `GameState` mutation
- pending auto ability PlayTable publish control is covered in EditMode for
  disabled no-payload state, online publish through transport, local omission,
  and no `GameState` mutation
- pending auto ability item list formatting is covered in EditMode for
  no-payload, invalid, empty, hidden-source, visible-source, long-list cap, and
  no payload mutation behavior
- pending auto ability PlayTable item list surface is covered in EditMode for
  local zero output, online decoded item metadata, hidden-source output, no
  transport send while rendering, and no `GameState` mutation
- pending auto ability selection state is covered in EditMode for empty/null
  queue rejection, valid copied selection, out-of-range/null-item rejection,
  clear-selection state, and no source queue mutation
- pending auto ability selection status formatting is covered in EditMode for
  null/no-selection, rejection reason, visible selection, hidden-source safety,
  and no selected ability mutation
- pending auto ability PlayTable selection status surface is covered in
  EditMode for local no-selection output, online no-selection output, surface
  existence, no pending queue transport send, and no `GameState` mutation
- pending auto ability PlayTable selection cycle control is covered in
  EditMode for local omission, no-payload disabled state, online selection
  cycling, hidden-source status output, no pending queue transport send, and no
  `GameState` mutation
- pending auto ability PlayTable clear selection control is covered in EditMode
  for local omission, no-selection disabled state, enable-after-selection,
  reset-to-none status, hidden-source cleanup, no pending queue transport send,
  and no `GameState` mutation
- pending auto ability selected resolution request model is covered in EditMode
  for null/no-selection rejection, rejected selection rejection, visible request
  JSON round-trip, hidden-source safety, and no selected ability mutation
- pending auto ability selected resolution request formatter is covered in
  EditMode for null result/request fallbacks, rejected result text, visible
  source formatting, hidden-source safety, and no request mutation
- pending auto ability PlayTable selected resolution request preview is covered
  in EditMode for local no-request text, online surface creation,
  no-selection rejection preview, selection-cycle preview updates,
  hidden-source safety, and no `GameState` mutation or pending queue send
- pending auto ability selected resolution request payload codec is covered in
  EditMode for visible request round-trip, protocol mismatch rejection,
  empty/invalid request JSON rejection, deterministic encoding, hidden-source
  sanitization, and no source request mutation
- pending auto ability selected resolution request Photon wrapper is covered in
  EditMode for wrapper round-trip, wrong event code rejection, inner protocol
  mismatch rejection, deterministic wrapper JSON, and no source payload mutation
- pending auto ability selected resolution request transport hook is covered in
  EditMode for fake transport send/emit, Photon adapter decoded dispatch,
  null-payload rejection, and no `GameState` mutation
- pending auto ability selected resolution request session storage is covered
  in EditMode for receipt storage outside `GameState`, null payload ignore,
  normal game-event sync continuity after receipt, and no `GameState` mutation
- pending auto ability selected resolution request session publish helper is
  covered in EditMode for missing-payload rejection, latest-payload send,
  stored payload preservation, and no `GameState` mutation
- pending auto ability selected resolution request PlayTable publish control is
  covered in EditMode for local omission, disabled no-selection state,
  enable-after-selection, payload send/storage, hidden-source masking, no
  pending queue send, and no `GameState` mutation
- pending auto ability selected resolution request summary surface is covered
  in EditMode for no-payload, invalid-payload, visible latest request,
  hidden-source safety, online received summary, online local publish summary,
  and no `GameState` mutation
- pending auto ability selected resolution request list formatter is covered in
  EditMode for no-payload, newest-first bounded output, invalid payloads,
  hidden-source safety, and no payload mutation
- pending auto ability selected resolution request PlayTable list surface is
  covered in EditMode for local no-list text, online newest-first list after
  multiple received payloads, and no `GameState` mutation
- pending auto ability manual resolution decision model is covered in EditMode
  for missing request, invalid decision type, resolve JSON round-trip,
  skip/defer sanitized reasons, hidden-source safety, request no-mutation, and
  no `GameState` mutation
- pending auto ability manual resolution decision payload codec is covered in
  EditMode for visible round-trip, protocol mismatch, empty payload rejection,
  deterministic encoding, hidden-source sanitization, and no source decision
  mutation
- pending auto ability manual resolution decision Photon wrapper is covered in
  EditMode for wrapper round-trip, wrong event code rejection, inner protocol
  mismatch rejection, deterministic wrapper JSON, and no source payload mutation
- pending auto ability manual resolution decision transport hook is covered in
  EditMode for fake transport send/emit, Photon adapter decoded dispatch,
  null-payload rejection, and no `GameState` mutation
- pending auto ability manual resolution decision session storage is covered in
  EditMode for receipt storage outside `GameState`, null payload ignore, normal
  game-event sync continuity after receipt, and no `GameState` mutation
- pending auto ability manual resolution decision session publish helper is
  covered in EditMode for no-payload rejection, latest-payload send, stored
  payload preservation, and no `GameState` mutation
- pending auto ability manual resolution decision PlayTable publish control is
  covered in EditMode for local omission, online no-payload disabled state,
  latest stored payload publish, stored payload preservation, and no
  `GameState` mutation
- pending auto ability manual resolution decision summary formatter is covered
  in EditMode for no-payload, invalid-payload, visible latest decision,
  hidden-source safety, and no payload mutation
- pending auto ability manual resolution decision PlayTable summary surface is
  covered in EditMode for local zero summary, online received/published
  summaries, hidden-source safety, and no `GameState` mutation
- pending auto ability manual resolution decision list formatter is covered in
  EditMode for no-payload, newest-first bounded output, invalid payloads,
  hidden-source safety, and no payload mutation
- pending auto ability manual resolution decision PlayTable list surface is
  covered in EditMode for local zero list, online newest-first list,
  hidden-source safety, and no `GameState` mutation
- pending auto ability manual resolution decision publish result formatter is
  covered in EditMode for success, transport failure, null-result fallback, and
  PlayTable status message reuse
- pending auto ability manual resolution apply preview log entry is covered in
  EditMode for accepted/rejected JSON round-trip, hidden-source-safe fields,
  rejected field clearing, and source apply result no-mutation
- pending auto ability manual resolution apply preview log formatter is covered
  in EditMode for accepted/rejected/null/list output, newest-first bounded
  lists, hidden free-text summary safety, and source entry no-mutation
- pending auto ability manual resolution apply preview session log storage is
  covered in EditMode through game-session preview tests for accepted and
  rejected log append, latest log access, no network publish, and no
  `GameState` mutation
- pending auto ability manual resolution apply preview PlayTable surface is
  covered in EditMode for local zero-state text, accepted/rejected online log
  display, hidden source safety, no network publish, and no `GameState`
  mutation
- pending auto ability manual resolution apply preview flow verification is
  covered in EditMode for queue payload -> decision payload -> validation ->
  apply preview -> session log, including accepted and validation-rejected paths
  with no network publish and no `GameState` mutation
- pending auto ability queue commit policy is a docs-only gate for M10-105 and
  M10-106; helper/event tests must prove accepted/rejected queue changes,
  input no-mutation, replay metadata, hidden-source masking, and no direct
  UI/bot/network `GameState` mutation
- pending auto ability queue commit helper is covered in EditMode for accepted
  `Skip`, `Defer`, and manual `Resolve`, rejected missing queue/decision,
  invalid type, pending id mismatch, input queue no-mutation, and null pending
  list no-normalization
- pending auto ability queue commit event is covered in EditMode for accepted
  event metadata, JSON round-trip, manual `Resolve` flag preservation,
  missing/rejected commit result handling, hidden-source safety, and no
  queue/result mutation
- trigger check commit event is covered in EditMode for accepted outcome
  metadata, JSON round-trip, masked checked-card identity preservation,
  missing/invalid metadata rejection, and no source log entry mutation
- trigger allocation commit helper is covered in EditMode for accepted modifier
  append, empty accepted ledger, missing/rejected/missing-ledger rejection,
  masked trigger source metadata preservation, and no input ledger/bundle
  mutation
- trigger modifier cleanup integration is covered in EditMode for EndOfBattle
  and EndOfTurn cleanup against committed ledgers, rejected paths, source
  ledger no-mutation, Manual/Permanent retention, and null modifier list
  no-normalization

## Golden Fixtures

Every important mechanic should have a small fixture state with expected output.
When a ruling bug is found, add a fixture before or with the fix.

## CI Direction

- Python data/unit tests first. Done in M18-01 with
  `.github/workflows/python-tests.yml`.
- Unity EditMode tests next.
- Windows build smoke test later.
- Android build smoke test after mobile readiness begins.

Latest M18 CI baseline after `M18-01`:

- Python unit-test workflow exists for push and pull request
- CI command: `python -m unittest discover -s tests -p "test_*.py"`
- Local Python unit tests: `31/31`

Latest M18 CI baseline after `M18-02`:

- Data-validation workflow exists for push and pull request
- CI commands cover pack verification, custom pack schema validation, custom
  pack import smoke, and structured ability validation
- Local data validation commands passed

Latest M18 CI baseline after `M18-03`:

- Unity compile workflow exists for push and pull request
- Workflow expects a self-hosted Windows runner with `Unity` label
- Workflow uses `UNITY_EXE` override or Unity Hub `6000.5.0f1` default path
- Local Unity batchmode compile passed

Latest M18 CI baseline after `M18-04`:

- Unity EditMode workflow exists for push and pull request
- Workflow expects a self-hosted Windows runner with `Unity` label
- Workflow uploads EditMode log and XML result artifacts
- Local Unity EditMode tests passed at `839/839`

Latest M18 QA baseline after `M18-05`:

- Core regression suite report covers RulesCore facade, legal action mask,
  event sourcing/replay, snapshot/rollback, hidden-state masking, resource
  ledger, and RuleSet profile gates
- `CORE_REGRESSION_SUITE_SPEC.md` documents the gate
- Local Unity compile passed
- Local Unity EditMode tests passed at `843/843`

Latest M18 QA baseline after `M18-06`:

- Ability regression suite report covers schema contract, Python validator,
  runtime registry, cost/target templates, effect/resource templates, modifier
  templates, fixture DSL/pack smoke, manual fallback bridge, and custom-pack
  ability metadata gates
- `ABILITY_REGRESSION_SUITE_SPEC.md` documents the gate
- Python unit tests passed at `31/31`
- Structured ability pack validation passed with `20` abilities and no warnings
- Local Unity compile passed
- Local Unity EditMode tests passed at `847/847`

Latest M18 QA baseline after `M18-07`:

- Multiplayer payload/no-leak suite report covers command envelope cursor,
  owner-private room state, public event masking, public reconnect recovery,
  spectator replay sync, trigger-check payload, pending AUTO payload, manual
  resolution decision payload, and session-storage no-mutation gates
- `MULTIPLAYER_PAYLOAD_NO_LEAK_SUITE_SPEC.md` documents the gate
- Local Unity compile passed
- Local Unity EditMode tests passed at `851/851`

Latest M18 build baseline after `M18-08`:

- Windows build runner exists:
  `VanguardThaiSim.EditorTools.WindowsBuildArtifactRunner.RunFromCommandLine`
- Windows build artifact exists:
  `client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe`
- Build log reported `Succeeded`, errors `0`, warnings `0`
- `WINDOWS_BUILD_ARTIFACT_SPEC.md` documents the artifact path and command
- Local Unity compile passed
- Local Unity EditMode tests passed at `851/851`

Latest M18 build baseline after `M18-09`:

- Android build runner exists:
  `VanguardThaiSim.EditorTools.AndroidBuildArtifactRunner.RunFromCommandLine`
- Android APK artifact exists:
  `client/unity/VanguardThaiSim/build/android/latest/VanguardThaiSim.apk`
- Build log reported `Succeeded`, errors `0`, warnings `0`
- `ANDROID_BUILD_ARTIFACT_SPEC.md` documents the artifact path and command
- Local Unity compile passed
- Local Unity EditMode tests passed at `851/851`

Latest M18 release-candidate baseline after `M18-10`:

- `RELEASE_CANDIDATE_CHECKLIST.md` records Python/data/Unity/build evidence
- Windows and Android artifacts exist at the documented paths
- Windows and Android build logs contain `Succeeded`
- Unity EditMode XML contains `total="851" passed="851" failed="0"`
- No GitHub Release or artifact upload was performed

## Latest Closeout Baseline

`M10-112` closeout passed:

- Vanguard TH pack verification: `10836/10836` cards and images
- custom pack schema validation with expected fallback-image warnings
- custom pack import smoke
- Python unit tests: `13/13`
- Unity compile with no compiler errors
- Unity EditMode: `545/545`

Latest M11 RulesCore baseline after `M11-05`:

- Unity compile with no compiler errors
- Unity EditMode: `572/572`

Latest M11 RulesCore baseline after `M11-06`:

- Unity compile with no compiler errors
- Unity EditMode: `578/578`

Latest M11 RulesCore baseline after `M11-07`:

- Unity compile with no compiler errors
- Unity EditMode: `584/584`

Latest M11 RulesCore closeout baseline after `M11-12`:

- Data pack verification: `10836/10836` cards and `10836/10836` images
- Custom pack validation/import passed with expected fallback-image warnings
- Python unit tests: `13/13`
- Unity compile with no compiler errors
- Unity EditMode: `607/607`

Latest M12 structured ability baseline after `M12-01`:

- Python unit tests: `17/17`
- Ability schema structural tests cover top-level shape, required sections,
  template enum coverage, and sample ability shape

Latest M12 structured ability baseline after `M12-02`:

- Ability schema validator CLI accepts `sample_abilities.json`
- Python unit tests: `22/22`
- Validator tests cover valid sample data, duplicate ability ids, missing
  required sections, invalid enums, and once-per-turn key rejection

Latest M12 structured ability baseline after `M12-03`:

- Unity compile with no compiler errors
- Unity EditMode: `612/612`
- Runtime ability registry tests cover card/ability indexing, manual fallback
  preservation, clone-safe reads, duplicate ability id rejection, missing/wrong
  schema rejection, and load result JSON round-trip

Latest M12 structured ability baseline after `M12-04`:

- Unity compile with no compiler errors
- Unity EditMode: `618/618`
- Structured cost template tests cover CB/SB/Energy aggregation, once key
  generation, ResourceLedger validation without mutation, discard manual
  placeholder, negative cost rejection, duplicate once rejection, and JSON
  round-trip

Latest M12 structured ability baseline after `M12-05`:

- Unity compile with no compiler errors
- Unity EditMode: `624/624`
- Structured target template tests cover self/any public target resolution,
  face-down skipping, optional fewer-candidate acceptance, required count
  rejection, hidden/unsupported zone manual placeholders, circle manual
  placeholder, source no-mutation, and JSON round-trip

Latest M12 structured ability baseline after `M12-06`:

- Unity compile with no compiler errors
- Unity EditMode: `630/630`
- Structured effect template tests cover draw preview no-mutation, draw apply
  through RulesCore/event log, move-zone apply through RulesCore, unsupported
  effect manual fallback, invalid move reject no-mutation, and JSON round-trip

Latest M12 structured ability baseline after `M12-07`:

- Unity compile with no compiler errors
- Unity EditMode: `636/636`
- Structured effect resource tests cover CounterBlast apply through
  event-sourced ResourceFlip, CounterCharge preview no-mutation, unavailable
  CounterBlast reject no-mutation, SoulCharge/SoulBlast placeholders as of the
  historical M12-07 baseline, replay determinism, event-sourcing coverage,
  facade coverage, timing audit, pending AUTO timing mapping, and event-log
  formatting. M21-04d supersedes the Soul placeholder behavior with live
  RulesCore-backed SoulCharge/SoulBlast commands.

Latest M12 structured ability baseline after `M12-08`:

- Unity compile with no compiler errors
- Unity EditMode: `642/642`
- Structured modifier effect tests cover PowerPlus preview ledger/projection
  with no source mutation, CriticalPlus apply mutating only the supplied
  `CombatModifierLedger`, unsupported duration manual fallback, missing amount
  rejection, hidden/missing target rejection, and result JSON round-trip

Latest M12 structured ability baseline after `M12-09`:

- Unity compile with no compiler errors
- Unity EditMode: `648/648`
- Structured ability fixture DSL tests cover draw fixture counts, CounterBlast
  damage face-up counts, PowerPlus ledger-only modifier execution, expectation
  mismatch reporting, unsupported manual-effect fallback rejection, source state
  no-mutation, and fixture/result JSON round-trip

Latest M12 structured ability baseline after `M12-10`:

- Python unit/data tests: `24/24`
- Unity compile with no compiler errors
- Unity EditMode: `651/651`
- First structured card pack tests cover Python schema validation for the
  20-ability template smoke pack, supported effect subset, runtime registry
  loading from the actual pack file, and fixture execution for representative
  draw and PowerPlus abilities

Latest M12 structured ability baseline after `M12-11`:

- Unity compile with no compiler errors
- Unity EditMode: `657/657`
- Manual fallback bridge tests cover unsupported fixture rejection to Resolve
  decision, hidden-source masking, manual-fallback-disabled rejection,
  missing-reason rejection, ability/state no-mutation, and result JSON
  round-trip

Latest M12 closeout baseline after `M12-12`:

- Vanguard TH pack verification: `10836/10836` cards and `10836/10836` images,
  all OK
- Custom pack schema validation/import: all OK, with known fallback-image
  warnings for the template pack
- M12 structured ability pack validation: 20 abilities, all OK
- Python tests: `24/24`
- Unity compile with no compiler errors
- Unity EditMode: `657/657`

Latest M13 multiplayer baseline after `M13-01`:

- Unity compile with no compiler errors
- Unity EditMode: `661/661`

M13-01 owner-private room initialization tests cover:

- local true state creation from only the local deck plus commitments
- opponent deck/hand/ride placeholders using hidden card ids and public count
  metadata only
- no opponent real card id leakage in serialized local session output
- local player index 0 and 1 initialization
- commitment mismatch rejection without mutating room/deck inputs
- missing opponent count metadata rejection

Latest M13 multiplayer baseline after `M13-02`:

- Unity compile with no compiler errors
- Unity EditMode: `662/662`

M13-02 client-trust UX tests cover:

- pre-ack commitment-only start rejects with
  `DECK_COMMITMENT_CLIENT_TRUST_POLICY_REQUIRED`
- post-ack commitment-only start still rejects with
  `OWNER_PRIVATE_GAMEPLAY_PATH_INCOMPLETE`
- policy output records the client-trust acknowledgment
- runtime lobby UI includes the `Acknowledge Trust` button

Latest M13 multiplayer baseline after `M13-03`:

- Unity compile with no compiler errors
- Unity EditMode: `666/666`

M13-03 online command envelope tests cover:

- command metadata capture: player id, player index, sequence, room id,
  room game id, and state cursor
- JSON round-trip
- Photon payload round-trip on event code `12`
- wrong event-code rejection
- basic shape validation

Latest M13 multiplayer baseline after `M13-04`:

- Unity compile with no compiler errors
- Unity EditMode: `670/670`

M13-04 command envelope validation tests cover:

- current turn-owner command acceptance
- stale cursor rejection without source `GameState` mutation
- out-of-turn rejection
- action actor mismatch rejection
- player id/index ownership mismatch rejection

Latest M13 multiplayer baseline after `M13-05`:

- Unity compile with no compiler errors
- Unity EditMode: `674/674`

M13-05 public event masking delivery tests cover:

- hidden draw count update without card identity leak
- private-to-public reveal using public identity only
- owner-private session public log/cursor update
- local true-state no-mutation
- invalid actor rejection without public view mutation

Latest M13 multiplayer baseline after `M13-06`:

- Unity compile with no compiler errors
- Unity EditMode: `678/678`

M13-06 public reconnect tests cover:

- public batch slicing from cursor
- Photon payload round-trip for public event batches
- applying public batch to owner-private session view/log/cursor
- cursor mismatch rejection without session mutation
- commitment-mode true reconnect batch emits no true events

Latest M13 multiplayer baseline after `M13-07`:

- Unity compile with no compiler errors
- Unity EditMode: `679/679`

M13-07 start guard tests cover:

- shared deck-code start succeeds with matching canonical deck hash
- shared deck-code start rejects guest deck hash mismatch

Latest M13 multiplayer baseline after `M13-08`:

- Unity compile with no compiler errors
- Unity EditMode: `683/683`

M13-08 room lifecycle tests cover:

- ready transitions use cloned rooms and preserve source room
- start rejects until all connected players are ready
- start/end/rematch happy path
- invalid transition rejection without source mutation

Latest M13 multiplayer baseline after `M13-09`:

- Unity compile with no compiler errors
- Unity EditMode: `688/688`

M13-09 public spectator replay sync tests cover:

- public replay `StepForward()` updates spectator state counts and phase
- public event batch sync appends/applies from the current public cursor
- cursor mismatch rejection without replay state mutation
- invalid public event rejection without replay state mutation
- cloned visible event logs cannot mutate replay history
- hidden opponent card ids/instance ids do not leak into replay JSON or current
  spectator state

Latest M13 multiplayer baseline after `M13-10`:

- Unity compile with no compiler errors
- Unity EditMode: `692/692`

M13-10 tournament audit log tests cover:

- room, pack, player, public event, and result metadata export
- JSON round-trip with list fields
- source room/public event no-mutation
- null public event/result handling
- audit JSON excludes deck code, reveal nonce, hidden card id, hidden instance
  id, and hidden public card payloads

Latest M13 live multiplayer smoke after `M13-11`:

- Unity compile with no compiler errors
- Unity EditMode: `692/692`
- `PhotonGameSessionSmokeTestRunner` live pass with local AppId config

M13-11 live smoke covers:

- two Photon clients connect, host, and join
- room lifecycle state reaches `playing`
- host `Draw` action syncs into guest state
- reconnect request/batch delivery completes
- room lifecycle state reaches `ended`

Latest M14 bot baseline after `M14-01`:

- Unity compile with no compiler errors
- Unity EditMode: `697/697`

M14-01 heuristic bot v2 tests cover:

- selected action is executable through `RulesCore`
- source `GameState` is unchanged after bot thinking
- empty-vanguard setup prefers hand-to-vanguard over rear-guard call
- top-deck card stats do not change draw scoring
- decision reason does not leak top-deck or opponent private card ids
- repeated evaluation is deterministic

Latest M14 bot baseline after `M14-02`:

- Unity compile with no compiler errors
- Unity EditMode: `704/704`

M14-02 guard decision bot tests cover:

- lethal attack recommends guard when shield is available
- low-damage non-lethal attack recommends no guard
- high trigger risk at danger damage recommends guard
- insufficient shield returns cannot-guard
- high shield demand can prefer perfect guard
- source `GameState` is unchanged and opponent private ids do not leak
- repeated decisions are deterministic

Latest M14 bot baseline after `M14-03`:

- Unity compile with no compiler errors
- Unity EditMode: `709/709`

M14-03 trigger-risk attack choice tests cover:

- high trigger risk can change the selected sequence to one starting with the
  vanguard
- probability is planning-only and no trigger outcome is applied
- source `GameState`, event log, and power modifiers are unchanged
- invalid probability falls back to zero trigger risk
- hidden attackers are skipped and hidden ids do not leak
- repeated choices are deterministic

Latest M14 bot baseline after `M14-04`:

- Unity compile with no compiler errors
- Unity EditMode: `713/713`

M14-04 battle sequence search v2 tests cover:

- guard estimate adds positive guard pressure
- trigger risk and trigger pressure are carried into v2 output
- hidden attackers are skipped
- source `GameState` is unchanged
- repeated search output is deterministic

Latest M14 bot baseline after `M14-05`:

- Unity compile with no compiler errors
- Unity EditMode: `718/718`

M14-05 snapshot simulation path tests cover:

- accepted action mutates branch only
- sequential accepted actions apply in order on the branch
- rejected action reports a rejected branch result without live mutation
- missing inputs reject cleanly
- result JSON round-trip and branch restore

Latest M14 bot baseline after `M14-06`:

- Unity compile with no compiler errors
- Unity EditMode: `723/723`

M14-06 playbook integration tests cover:

- priority call bias prefers a playbook card
- no match falls back to default balanced with no priority bias
- source `GameState` is unchanged
- reason text does not leak opponent private ids or priority card ids
- repeated evaluation is deterministic

Latest M14 bot baseline after `M14-07`:

- Unity compile with no compiler errors
- Unity EditMode: `723/723`

M14-07 offline combo discovery output tests cover:

- report includes combo lines and replay references
- JSON round-trip preserves combo line fields
- repeated reports are deterministic
- source `GameState` is unchanged

Latest M14 bot baseline after `M14-08`:

- Unity compile with no compiler errors
- Unity EditMode: `727/727`

M14-08 bot debug trace tests cover:

- trace includes selected action and ranked lines
- JSON round-trip
- source `GameState` is unchanged
- opponent private/top-deck/priority-card ids do not leak
- repeated trace output is deterministic

Latest M14 bot baseline after `M14-09`:

- Unity compile with no compiler errors
- Unity EditMode: `731/731`

M14-09 ISMCTS readiness gate tests cover:

- default gate allows the M14-10 prototype
- a blocked requirement prevents advanced search
- JSON round-trip
- deterministic output

Latest M14 bot baseline after `M14-10`:

- Unity compile with no compiler errors
- Unity EditMode: `736/736`

M14-10 advanced search prototype tests cover:

- default readiness allows bounded one-ply search
- blocked readiness rejects search
- source `GameState` is unchanged
- opponent private and top-deck ids do not leak
- result JSON round-trip
- repeated search output is deterministic

Latest M15 custom pack baseline after `M15-01`:

- v1 custom pack template validation passes
- v2 custom pack template validation passes
- Python unit tests: `29/29`

M15-01 custom pack v2 schema tests cover:

- v1 template remains valid
- v2 template validates with capability metadata
- v2 rejects missing `capabilities.cards`
- v2 rejects unsafe `abilities_file`
- v2 rejects invalid dependency pack ids
- v2 import preserves source schema metadata in the runtime manifest

Latest M15 custom pack baseline after `M15-02`:

- v2 custom pack template validation passes with ability data
- v2 custom pack import passes
- Python unit tests: `31/31`

M15-02 custom pack ability data tests cover:

- ability file passes `ability_schema_v1`
- ability `card_id` must exist in the same custom pack
- invalid ability schema rejects the pack
- unsafe ability path rejects the pack
- runtime manifest preserves ability count and ability data hash

Latest M15 custom pack baseline after `M15-03`:

- Unity compile with no compiler errors
- Unity EditMode: `740/740`

M15-03 pack validation status tests cover:

- default Vanguard TH manifest accepted
- schema v2 capability and ability metadata reported
- missing definition hash blocks the status
- status JSON round-trip

Latest M15 custom pack baseline after `M15-04`:

- Unity compile with no compiler errors
- Unity EditMode: `744/744`

M15-04 pack registry tests cover:

- entry creation from manifest and validation status
- enable/disable returns cloned state
- missing pack rejection preserves source state
- registry JSON round-trip

Latest M15 custom pack baseline after `M15-05`:

- Unity compile with no compiler errors
- Unity EditMode: `748/748`

M15-05 custom format profile tests cover:

- valid catalog references an existing base RuleSet profile
- unknown base RuleSet rejection
- duplicate format, alias, and pack rejection
- catalog and validation result JSON round-trip

Latest M15 custom pack baseline after `M15-06`:

- Unity compile with no compiler errors
- Unity EditMode: `749/749`

M15-06 Standard format flag tests cover:

- Standard preset validates
- preset delegates to core `standard` RuleSet profile
- Standard-only flags are enabled
- V/Premium-only flags are disabled

Latest M15 custom pack baseline after `M15-07`:

- Unity compile with no compiler errors
- Unity EditMode: `750/750`

M15-07 V-Premium format flag tests cover:

- V-Premium preset validates
- preset delegates to core `v_premium` RuleSet profile
- V-Premium flags are enabled
- Standard/Premium-only flags are disabled

Latest M15 custom pack baseline after `M15-08`:

- Unity compile with no compiler errors
- Unity EditMode: `751/751`

M15-08 Premium format flag tests cover:

- Premium preset validates
- preset delegates to core `premium` RuleSet profile
- Premium flags are enabled
- Standard-only flags are disabled

Latest M15 custom pack baseline after `M15-09`:

- Unity compile with no compiler errors
- Unity EditMode: `757/757`

M15-09 custom format sandbox tests cover:

- core preset catalog resolves aliases
- sandbox exposes cloned base RuleSet feature summaries
- unknown format rejection preserves the source catalog
- disallowed pack rejection preserves the source catalog
- invalid catalog rejection preserves the underlying catalog reason
- empty allowed pack list accepts any pack for preview
- sandbox result JSON round-trip

Latest M15 custom pack/format closeout baseline after `M15-10`:

- v1 custom pack template validation passes with expected fallback-image warnings
- v2 custom pack template validation passes with ability data
- v2 custom pack import passes
- Python unit tests: `31/31`
- Unity compile with no compiler errors
- Unity EditMode: `762/762`

M15-10 custom format validation closeout tests cover:

- Standard, V-Premium, and Premium preset feature sets remain distinct
- core preset catalog JSON round-trip remains valid
- sandbox preview does not mutate format or RuleSet catalogs
- alias collisions reject before sandbox resolution
- pack allow/reject boundaries remain preview-only

Latest M16 UI/mobile baseline after `M16-01`:

- Unity compile with no compiler errors
- Unity EditMode: `767/767`

M16-01 pending AUTO panel polish tests cover:

- compact no-payload panel message
- valid queue panel summary
- selected hidden source redaction
- invalid payload panel message
- no payload or selection mutation during formatting
- PlayTable summary surface uses the polished panel formatter

Latest M16 UI/mobile baseline after `M16-02`:

- Unity compile with no compiler errors
- Unity EditMode: `772/772`

M16-02 trigger panel polish tests cover:

- compact local no-log panel
- online draft and selected-card summary
- latest trigger log summary without checked-card identity
- invalid payload panel message
- no payload mutation during formatting
- PlayTable trigger summary surface uses the polished panel formatter

Latest M16 UI/mobile baseline after `M16-03`:

- Unity compile with no compiler errors
- Unity EditMode: `778/778`

M16-03 manual resolution panel polish tests cover:

- compact no-decision panel message
- valid decision panel without source instance id
- hidden decision redaction
- invalid payload rejection summary
- apply preview summary without free-text leak
- no payload/apply-preview mutation during formatting
- PlayTable manual resolution summary uses the polished panel formatter

Latest M16 UI/mobile baseline after `M16-04`:

- Unity compile with no compiler errors
- Unity EditMode: `783/783`

M16-04 event/replay panel polish tests cover:

- compact empty panel message
- event count/latest/recent newest-first display
- resource flip instance id redaction
- max entry limiting
- no event mutation during formatting
- PlayTable event surface uses the polished panel formatter

Latest M16 UI/mobile baseline after `M16-05`:

- Unity compile with no compiler errors
- Unity EditMode: `783/783`

M16-05 online status/cursor panel polish tests cover:

- local mode text remains compact
- online status, transport, cursor, and trigger log count render as explicit
  key/value segments
- blank status/transport fall back to `unknown`
- reconnect segment appears only when a positive reconnect batch was applied
- no `GameState` mutation or network behavior changes

Latest M16 UI/mobile baseline after `M16-06`:

- Unity compile with no compiler errors
- Unity EditMode: `789/789`

M16-06 Deck Builder filter polish tests cover:

- compact no-filter card pool status
- active search, series, and clan filter text
- blank filter normalization
- deck count, issue total, and playable status text
- bounded validation issue summaries with severity labels
- no validation-result mutation during formatting

Latest M16 UI/mobile baseline after `M16-07`:

- Unity compile with no compiler errors
- Unity EditMode: `795/795`

M16-07 Card Browser Thai search polish tests cover:

- no-filter empty-result message
- Thai query preservation in visible empty-result text
- English query plus series/clan filter labels
- whitespace normalization for display
- blank filter omission
- no `CardQueryOptions` mutation during formatting

Latest M16 UI/mobile baseline after `M16-08`:

- Unity compile with no compiler errors
- Unity EditMode: `800/800`

M16-08 broken image fallback tests cover:

- cache fallback texture detection through `CardImageCache.IsFallbackTexture()`
- fallback texture loading without changing image cache behavior
- tile label fallback marker only when a fallback image is used
- detail status for missing path versus unreadable/missing file
- no card metadata, SQLite data, or image files changed

Latest M16 UI/mobile baseline after `M16-09`:

- Unity compile with no compiler errors
- Unity EditMode: `805/805`

M16-09 Android touch/layout QA tests cover:

- fixed Android phone portrait, phone landscape, and tablet reference viewports
- minimum `48` touch-target height for non-desktop Android profiles
- toolbar width budgets for Card Browser and Play Table
- card tile image/label sizing inside grid cells
- detail/deck panel, play side panel, and play table vertical budget checks
- negative issue-code tests for collapsed touch targets and toolbar overflow

Latest M16 UI/mobile baseline after `M16-10`:

- Unity compile with no compiler errors
- Client smoke runner: passed with no blockers
- Unity EditMode: `806/806`

M16-10 Windows + Android smoke flow covers:

- default pack manifest and SQLite database load
- runtime pack resolution from nested Windows player artifact paths
- Card Browser query and card detail load
- playable `50+4` deck validation and deck-code round-trip
- Play Table RulesCore draw, move, and phase command commit flow
- Android responsive layout QA reference viewports
- enabled Unity build scene
- Windows Standalone and Android build target support
- Standalone and Android `VANGUARD_PHOTON_REALTIME` scripting define checks

Latest M17 headless baseline after `M17-01`:

- Unity compile with no compiler errors
- Headless CLI runner: accepted default simulation result JSON
- Unity EditMode: `807/807`

M17-01 Headless CLI runner tests cover:

- default fixed-seed headless run accepts
- generated smoke deck and two-player game state are created without UI
- RulesCore executes Draw, MoveCard, SetPhase, and AddGiftMarker
- final phase, event count, zone counters, and Protect marker count are
  deterministic across repeated runs

Latest M17 headless baseline after `M17-02`:

- Unity compile with no compiler errors
- Headless CLI custom seed/ruleset input: accepted
- Headless CLI invalid seed input: rejected with result JSON
- Unity EditMode: `812/812`

M17-02 CLI input tests cover:

- default no-argument behavior remains accepted
- custom seed and ruleset are applied
- valid deck-code input runs and reports `deck_source = deck_code`
- invalid deck-code input returns a rejected result
- CLI parser accepts seed/ruleset/deck-code/result-path arguments
- invalid seed input returns parser errors before simulation

Latest M17 headless baseline after `M17-03`:

- Unity compile with no compiler errors
- Headless CLI result/replay output: accepted
- Unity EditMode: `813/813`

M17-03 replay/result output tests cover:

- `RunWithReplay()` creates accepted result and replay artifacts
- replay artifact schema version and hidden-state policy are explicit
- default script emits four redacted replay event records
- replay event ids are preserved while card instance ids are redacted
- CLI parser accepts `-headlessReplayPath`

Latest M17 headless baseline after `M17-10`:

- Unity compile with no compiler errors
- Headless batch CLI: 3 accepted runs
- Headless dataset export: schema v1 summary output without card ids or card instance ids
- Headless observation/action/reward API: player-view observation with sanitized action masks
- Headless performance profiler: bounded timing-only summaries
- Packed state decision gate: defer/allow-prototype report without migration
- Distributed worker prototype spec: local/spec-only/no-leak/no-RL/no-cluster validator
- Distributed worker local prototype: spec validation plus in-memory batch/dataset/profile artifacts
- Core regression suite: required category inventory and JSON report validation
- Ability regression suite: required category inventory, Python ability schema validation, and JSON report validation
- Multiplayer payload/no-leak suite: required category inventory and JSON report validation
- Windows build artifact: `build/windows/latest/VanguardThaiSim.exe`
- Android build artifact: `build/android/latest/VanguardThaiSim.apk`
- Release candidate checklist: `docs/RELEASE_CANDIDATE_CHECKLIST.md`
- Unity EditMode: `851/851`

M17-04 batch runner tests cover:

- default batch runs three accepted simulations with sequential seeds
- custom batch count/start seed/ruleset is applied
- out-of-range run count rejects before simulation
- CLI parser accepts batch count/start seed/ruleset/deck-code/result-path
- invalid batch count returns parser errors

M17-05 dataset export tests cover:

- batch result conversion into deterministic dataset schema v1
- run-level accepted/blocked counts and summary metrics
- no card id or card instance id fields in exported JSON
- null batch input returns an empty dataset

M17-06 observation/action/reward API tests cover:

- observation creation from player-view state without source mutation
- sanitized legal action mask without labels, card ids, or card instance ids
- deterministic observation output for the same source state
- smoke acceptance reward model for accepted and rejected results
- combined sample JSON no-leak behavior

M17-07 performance profiler tests cover:

- bounded profile request records accepted headless run summaries
- out-of-range run count rejects before simulation
- timing policy is explicit and timing is not a behavior signal
- deterministic simulation summary fields remain stable for the same seed

M17-08 packed state decision gate tests cover:

- missing input defers with readable-state policy
- failed safety gates defer even when profile is slow
- within-target performance defers migration
- stable slow profile allows prototype only, not live state migration

M17-09 distributed worker prototype spec tests cover:

- default worker spec validates required local worker boundaries
- hidden-state artifact policy removal rejects the spec
- excessive worker run count rejects the spec
- JSON keeps prototype status and non-goals visible

M17-10 distributed worker local prototype tests cover:

- worker runs bounded batch and dataset export
- worker rejects invalid spec before running
- worker rejects run count outside spec limit before running
- optional profile returns timing summary
