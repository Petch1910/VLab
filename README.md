# Vanguard Thai Simulator

โปรเจกต์นี้มีเป้าหมายสร้างโปรแกรมเล่น Cardfight!! Vanguard ที่ใช้ได้ทั้งคอมและมือถือ โดยเริ่มจากฐานข้อมูลการ์ดไทยที่ดึงจาก KK Card Fight แล้วต่อยอดเป็น deck builder, manual simulator, replay, bot และ multiplayer ในอนาคต

## สถานะข้อมูลปัจจุบัน

- ดึงข้อมูล Vanguard TH แล้ว `10,836` ใบ
- ดึงรูปครบ `10,836` ไฟล์
- รายงานตรวจครบอยู่ที่ `outputs/kk_cardfight_export/data/verification_report.json`
- ไฟล์หลักสำหรับระบบใหม่: `outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json`

## Runtime Pack ปัจจุบัน

- Pack manifest: `data/packs/vanguard_th/manifest.json`
- SQLite runtime DB: `data/packs/vanguard_th/cards.sqlite`
- Pack verification: `data/packs/vanguard_th/verification_report.json`
- Build command: `python tools\data\build_vanguard_th_pack.py`
- Verify command: `python tools\verification\verify_vanguard_th_pack.py`
- Query command: `python tools\data\query_vanguard_th_pack.py summary`
- Test command: `python -m unittest discover -s tests -p "test_*.py"`

## Unity สถานะปัจจุบัน

- Unity Hub ติดตั้งแล้วที่ `C:\Program Files\Unity Hub\Unity Hub.exe`
- Unity Editor ติดตั้งแล้ว: `6000.5.0f1`
- Android Build Support, Android SDK/NDK Tools, OpenJDK ติดตั้งแล้ว
- Unity project สร้างแล้วที่ `client/unity/VanguardThaiSim/`
- SQLite runtime สำหรับ Editor/Windows วางแล้วที่ `client/unity/VanguardThaiSim/Assets/Plugins/`
- Card data repository อยู่ที่ `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Cards/`
- batchmode import/compile และ EditMode tests ผ่านแล้ว

## เอกสารสำคัญ

เริ่มอ่านจาก:

1. `AGENTS.md`
2. `docs/AI_CONTEXT_BRIEF.md`
3. `docs/PRODUCT_SPEC.md`
4. `docs/ROADMAP.md`
5. `docs/IMPLEMENTATION_PLAN.md`
6. `docs/ARCHITECTURE.md`

รายละเอียดเอกสารทั้งหมดอยู่ที่ `docs/INDEX.md`

## ทิศทางเทคโนโลยี

- Client/game: Unity + C#
- Local DB: SQLite
- Data import tools: Python
- Future backend: Firebase หรือ custom server ตาม phase
- Multiplayer future option: Photon/Fusion หรือ room server ที่ sync event log

## หลักการทำงาน

- ทำ manual simulator ก่อน auto effect engine
- ทุก action ในเกมต้องบันทึกเป็น replay/event log
- deck validator ต้องมาก่อน bot
- online/multiplayer ต้องมาหลัง local engine เสถียร
- ห้ามลอก asset/code จาก VangPro หรือเกม commercial อื่น

## Current Implementation Notes

- Unity runtime card browser is implemented in `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/CardBrowserBootstrap.cs`.
- The browser auto-creates its UI on play, reads `data/packs/vanguard_th/cards.sqlite`, shows a paged grid, lazy-loads card images, supports search plus series/clan filters, and opens a card detail panel.
- Initial deck builder state is implemented in `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Decks/` and wired into the card browser with add/remove/clear controls, live validation, local save/load, and `VGTH1` deck-code export.
- Game state foundation is implemented in `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/Game/` with two-player setup, seeded shuffle, opening hand draw, zones, phase, JSON round-trip, manual draw/move/set-phase actions, event log, and undo latest action.
- Manual play table, Gift marker support, replay playback, legal action generation, Easy bot, and profile bot are implemented as initial vertical slices.
- Latest verification passed: `python tools\verification\verify_vanguard_th_pack.py`, `python -m unittest discover -s tests -p "test_*.py"`, Unity compile, and Unity EditMode tests 25/25.
