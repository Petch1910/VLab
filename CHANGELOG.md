# Changelog

## Unreleased

- Added Unity runtime card browser bootstrap UI with paged grid, lazy image loading, search, series filter, clan filter, and detail panel.
- Added reusable Unity dropdown template and detail texture cleanup for the card browser.
- Verified Unity final compile in `work/unity_compile_card_browser_final.log`.
- Verified Unity EditMode tests passed 3/3 in `work/unity_editmode_results_final.xml`.
- Added Unity deck model in `VanguardThaiSim.Decks` with main/ride/G zones, add/remove/clear, and JSON round-trip support.
- Added `DeckValidator` with count, unknown-card, and copy-limit validation backed by `ICardRepository`.
- Added deck controls to the runtime card browser: add selected card to main/ride, remove from main, clear deck, live deck status, validation issues, and deck list.
- Verified M3 Unity compile in `work/unity_compile_m3_deck.log`.
- Verified Unity EditMode tests passed 6/6 in `work/unity_editmode_results_m3_deck.xml`.
- Added `DeckCodeCodec` for `VGTH1.<base64url(gzip-json)>` deck export/import.
- Added `DeckStorage` for local JSON deck save/load/list/delete.
- Added card browser Save/Load/Code buttons for local deck persistence and clipboard deck-code export.
- Verified M3-04 Unity compile in `work/unity_compile_m3_04_deck_code.log`.
- Verified Unity EditMode tests passed 8/8 in `work/unity_editmode_results_m3_04_deck_code.xml`.
- Added `VanguardThaiSim.Game` state model with player zones, phases, card instances, deterministic game factory, opening hand draw, and JSON round-trip.
- Verified M4-01 Unity compile in `work/unity_compile_m4_01_game_state.log`.
- Verified Unity EditMode tests passed 11/11 in `work/unity_editmode_results_m4_01_game_state.xml`.
- Added `GameActionService` with draw, move-card, set-phase, event logging, invalid source protection, and undo-latest action.
- Verified M4-02 Unity compile in `work/unity_compile_m4_02_manual_actions.log`.
- Verified Unity EditMode tests passed 15/15 in `work/unity_editmode_results_m4_02_manual_actions.xml`.
- Added `PlayTableBootstrap` manual table overlay with zones, click-selected card moves, draw/phase/undo actions, opponent summary, and event log panel.
- Added `GameEventReducer`, `GameReplay`, and `GameReplayPlayer` for replay save/load, step playback, jump-to-start, and jump-to-end.
- Added legal action generation/execution for draw, move-card, and phase actions.
- Added `EasyBotController` baseline bot and profile bot support for Aggro/Balanced/Defensive deterministic seeded behavior.
- Added Gift marker state/action/UI for Force, Accel, and Protect markers, including event log, replay, and undo support.
- Verified M4-03 Unity compile/tests in `work/unity_compile_m4_03_play_table.log` and `work/unity_editmode_results_m4_03_play_table.xml`.
- Verified M4-04 Unity compile/tests in `work/unity_compile_m4_04_replay.log` and `work/unity_editmode_results_m4_04_replay.xml`.
- Verified M5-01 Unity compile/tests in `work/unity_compile_m5_01_legal_actions.log` and `work/unity_editmode_results_m5_01_legal_actions.xml`.
- Verified M5-02 Unity compile/tests in `work/unity_compile_m5_02_easy_bot.log` and `work/unity_editmode_results_m5_02_easy_bot.xml`.
- Verified M5-03 Unity compile/tests in `work/unity_compile_m5_03_bot_profiles.log` and `work/unity_editmode_results_m5_03_bot_profiles.xml`.
- Verified Gift marker Unity compile/tests in `work/unity_compile_gift_markers.log` and `work/unity_editmode_results_gift_markers.xml`.
- Latest Unity EditMode tests passed 25/25.

- สร้างเอกสารกลางสำหรับควบคุมการพัฒนาด้วย AI หลายโมเดล
- กำหนดทิศทาง Unity + SQLite + Python data tools
- บันทึก source of truth ของ card data และ image assets
- เพิ่ม `docs/IMPLEMENTATION_PLAN.md` สำหรับแตก roadmap เป็น milestone/task พร้อม acceptance criteria
- อัปเดต `AGENTS.md`, `README.md`, และ `docs/INDEX.md` ให้ชี้ไปที่ implementation plan
- สร้างโครงสร้างโฟลเดอร์เริ่มต้นตาม M0-01
- เพิ่ม `tools/data/build_vanguard_th_pack.py` สำหรับสร้าง runtime manifest และ SQLite
- เพิ่ม `tools/verification/verify_vanguard_th_pack.py` สำหรับตรวจ runtime pack
- สร้าง `data/packs/vanguard_th/cards.sqlite`, `manifest.json`, และ `verification_report.json`
- ติดตั้ง Unity Hub ผ่าน official signed installer หลัง `winget` เจอ hash mismatch และ Chocolatey ติด non-admin lock
- ติดตั้ง Unity CLI `0.1.0-beta.7`
- ติดตั้ง Unity Editor `6000.5.0f1` พร้อม Android Build Support, Android SDK/NDK Tools, OpenJDK
- สร้าง Unity project `client/unity/VanguardThaiSim/` ด้วย template `com.unity.template.2d`
- รัน Unity batchmode import/compile ผ่าน
- เพิ่ม `tools/data/query_vanguard_th_pack.py` สำหรับ inspect/search runtime SQLite
- เพิ่ม `tests/test_vanguard_th_pack.py` สำหรับ unittest runtime pack
- เพิ่ม `docs/UNITY_DATA_ACCESS_CONTRACT.md` สำหรับเตรียม M2-01
- เพิ่ม staging C# DTO/interface/SQL constants สำหรับ Unity card data access layer
- เพิ่ม `docs/UNITY_SETUP.md` สำหรับติดตั้ง Unity Hub/Editor และปลดล็อก M0-02/M0-03
- เพิ่ม SQLite managed/native runtime สำหรับ Unity Editor/Windows
- เพิ่ม `SqliteCardRepository`, `CardPackFileSystem`, runtime asmdef และ EditMode tests
- Unity EditMode tests ผ่าน 3/3 สำหรับ manifest, SQLite counts, card lookup, search/filter
- เพิ่ม `docs/THIRD_PARTY.md` บันทึกที่มาและ hash ของ SQLite native DLL

## Data Preparation

- ดึง Vanguard TH จาก KK Card Fight สำเร็จ `10,836` ใบ
- ดาวน์โหลดรูปการ์ดครบ `10,836` ไฟล์
- สร้าง `vanguard_th_cards_with_images.json`
- สร้าง `verification_report.json`
