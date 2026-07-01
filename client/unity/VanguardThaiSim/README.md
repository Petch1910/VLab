# VanguardThaiSim Unity Project

Unity project นี้สร้างด้วย Unity `6000.5.0f1` และ template `com.unity.template.2d`

สถานะปัจจุบัน:

- batchmode import/compile ผ่านแล้ว
- Android Build Support, Android SDK/NDK Tools, OpenJDK ติดตั้งแล้ว
- SQLite managed/native runtime สำหรับ Editor/Windows อยู่ใน `Assets/Plugins/`
- `Assets/Scripts/Vanguard/Cards/` มี DTO, repository, SQL constants และ file-system helpers สำหรับ `M2-01`
- `Assets/Scripts/Vanguard/UI/CardBrowserBootstrap.cs` สร้าง card browser UI อัตโนมัติเมื่อกด Play
- Card browser แสดง grid แบบ paging, lazy-load รูปเฉพาะหน้าปัจจุบัน, search, filter series/clan และ detail panel
- `Assets/Scripts/Vanguard/Decks/` มี deck model, validation result/rules และ `DeckValidator`
- Card browser ต่อ deck controls แล้ว: add main, add ride, remove main, clear deck, live validation, deck list
- Deck persistence ต่อแล้ว: save/load local JSON deck และ copy deck code format `VGTH1.<base64url(gzip-json)>`
- `Assets/Scripts/Vanguard/Game/` มี game state model, player zones, phase, deterministic game factory, opening hand draw และ JSON round-trip
- `GameActionService` รองรับ draw, move card, set phase, event log และ undo action ล่าสุด
- `Assets/Scripts/Vanguard/UI/PlayTableBootstrap.cs` เปิด manual table จาก deck builder พร้อม zone/action/log panel
- Manual table รองรับ Gift marker: Force, Accel, Protect พร้อม event log/replay/undo
- `GameReplay`/`GameReplayPlayer` รองรับ replay playback ทีละ step และ jump start/end
- `Assets/Scripts/Vanguard/Bots/` มี Easy bot และ profile bot แบบ Aggro/Balanced/Defensive พร้อม deterministic seed

งานถัดไป:

1. ทำ `M6-01` image cache strategy สำหรับมือถือ
2. ทำ `M6-02` responsive UI pass
3. เริ่ม `M7-01` custom pack schema
4. เริ่ม `M8-01` event sync protocol หลัง local/mobile foundation เสถียร

คำสั่งตรวจสอบ:

```powershell
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
& $unityExe -batchmode -nographics -quit -projectPath $projectPath -logFile work\unity_compile_card_browser_final.log
& $unityExe -batchmode -nographics -runTests -testPlatform EditMode -projectPath $projectPath -testResults work\unity_editmode_results_final.xml -logFile work\unity_editmode_tests_final.log
& $unityExe -batchmode -nographics -quit -projectPath $projectPath -logFile work\unity_compile_m3_deck.log
& $unityExe -batchmode -nographics -runTests -testPlatform EditMode -projectPath $projectPath -testResults work\unity_editmode_results_m3_deck.xml -logFile work\unity_editmode_tests_m3_deck.log
& $unityExe -batchmode -nographics -quit -projectPath $projectPath -logFile work\unity_compile_m3_04_deck_code.log
& $unityExe -batchmode -nographics -runTests -testPlatform EditMode -projectPath $projectPath -testResults work\unity_editmode_results_m3_04_deck_code.xml -logFile work\unity_editmode_tests_m3_04_deck_code.log
& $unityExe -batchmode -nographics -quit -projectPath $projectPath -logFile work\unity_compile_m4_01_game_state.log
& $unityExe -batchmode -nographics -runTests -testPlatform EditMode -projectPath $projectPath -testResults work\unity_editmode_results_m4_01_game_state.xml -logFile work\unity_editmode_tests_m4_01_game_state.log
& $unityExe -batchmode -nographics -quit -projectPath $projectPath -logFile work\unity_compile_m4_02_manual_actions.log
& $unityExe -batchmode -nographics -runTests -testPlatform EditMode -projectPath $projectPath -testResults work\unity_editmode_results_m4_02_manual_actions.xml -logFile work\unity_editmode_tests_m4_02_manual_actions.log
& $unityExe -batchmode -nographics -quit -projectPath $projectPath -logFile work\unity_compile_m5_03_bot_profiles.log
& $unityExe -batchmode -nographics -runTests -testPlatform EditMode -projectPath $projectPath -testResults work\unity_editmode_results_m5_03_bot_profiles.xml -logFile work\unity_editmode_tests_m5_03_bot_profiles.log
& $unityExe -batchmode -nographics -quit -projectPath $projectPath -logFile work\unity_compile_gift_markers.log
& $unityExe -batchmode -nographics -runTests -testPlatform EditMode -projectPath $projectPath -testResults work\unity_editmode_results_gift_markers.xml -logFile work\unity_editmode_tests_gift_markers.log
```
