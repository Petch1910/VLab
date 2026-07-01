# VangPro Study Notes

วันที่ศึกษา: 2026-06-19

เป้าหมาย: ศึกษาแนวคิดระบบของ VangPro เพื่อนำมาปรับใช้กับระบบ Vanguard ใหม่ของเรา โดยไม่ลอก asset หรือ code ของแอปเดิม

## สถานะที่ตรวจพบ

- Package: `com.DOT.VangPro`
- Version: `13.8.8`
- Engine: Unity `2022.3.62f2`, IL2CPP, arm64-v8a
- Activity หลัก: `com.unity3d.player.UnityPlayerActivity`
- Auth/Backend ที่พบจาก strings: Firebase Auth, Firebase Realtime Database
- Multiplayer ที่พบจาก strings: Photon/PUN
- Asset system: Unity Addressables
- Card image remote base ที่พบจาก strings: Cloudflare R2 path `card-images/v1`
- มี custom card template ฝังในแอป รองรับ EN/JA/KO/TH/ZH

## Screenshot ที่เก็บไว้

- `work/vangpro_screen2.png` หน้า login/main
- `work/vangpro_after_load.png` lobby หลังเข้า guest
- `work/vangpro_deckedit.png` deck editor
- `work/vangpro_load_deck.png` load deck/share code dialog
- `work/vangpro_deck_type.png` deck accessory/type dialog

## Feature ที่น่าสนใจ

### 1. Lobby และโหมดเล่น

VangPro มีหน้า lobby ที่รวมสถานะสำคัญไว้ชัดเจน:

- Server version
- สถานะ connected
- user/guest name
- online count
- room count
- สร้างห้อง
- เล่นกับ CPU
- เล่นคนเดียว
- เลือก format เช่น `D STANDARD`
- deck status เช่น `ยังไม่ได้เลือกเด็ค`

แนวคิดที่ควรใช้กับระบบเรา:

- ทำหน้า dashboard เดียวที่บอกว่า deck พร้อมเล่นหรือไม่
- ห้ามเริ่ม CPU/online ถ้ายังไม่มี deck ที่ valid
- แยก `เล่นกับบอท`, `เล่นคนเดียว`, `สร้างห้อง`, `เข้าห้อง` ให้ชัด
- มี online/room count สำหรับระบบ multiplayer ในอนาคต

### 2. Deck editor

จุดเด่นที่เห็น:

- Layout landscape เต็มจอ
- Tabs ตาม nation/clan อยู่แถวบน
- Grid card pool ตรงกลาง
- Card preview/detail ด้านซ้าย
- Deck list ด้านขวา
- Counter เช่น `Ride: 0/4`, `Main: 0/50`
- ปุ่ม `จับภาพเด็ค`
- ปุ่ม `รีเซ็ตเด็ค`
- ปุ่ม `บันทึกเด็ค`
- ปุ่ม `โหลดเด็ค`
- ปุ่ม `ประเภทเด็ค`
- มี rule badge เช่น `Fighter's Rules JP`

แนวคิดที่ควรใช้กับระบบเรา:

- สร้าง deck builder ที่มี counter แบบ rule-aware
- Filter ด้วย series, clan, nation, grade, trigger, format
- Preview card และ text ภาษาไทยทันที
- รองรับ export image decklist
- รองรับ save/load deck เป็น JSON และ deck code
- เพิ่ม rule validator เช่น D/V/Premium/Thai custom rules

### 3. Deck sharing ด้วย code

Dialog load deck มี:

- selector deck ที่บันทึกไว้
- `โหลด`
- `ลบ`
- `Copy Code`
- `Apply Code`

แนวคิดที่ควรใช้กับระบบเรา:

- ทำ deck code ที่ copy/paste ได้
- ใช้ compact JSON + compression + base64url
- รองรับ import/export จาก clipboard
- ตรวจ card id หายหรือ version mismatch ตอน apply code
- แสดง diff ถ้า deck code มาจาก data version เก่า

### 4. Custom card import

VangPro มีระบบ custom card ที่ออกแบบดีมาก:

- ใช้ card data file: `.xlsx` หรือ `.csv`
- ใช้ card image ZIP: `.zip`
- column หลัก:
  - `CustomId`
  - `Name`
  - `Nation`
  - `Clan`
  - `Tribe`
  - `Grade`
  - `Power`
  - `Shield`
  - `Rare`
  - `Trigger`
  - `OrderType`
  - `Tags`
  - `OrderEvents`
  - `Effect`
  - `ImageFile`
  - `Notes`
- สร้าง `custom_card_manifest.json`
- ใช้ `CustomId`, definition hash, image hash เพื่อกันข้อมูล custom card ไม่ตรงกันใน multiplayer

แนวคิดที่ควรใช้กับระบบเรา:

- ทำระบบ import card pack แบบ `.csv/.xlsx + images.zip`
- ใช้ stable id เช่น `th-bt01-001` หรือ `custom-pack-id/card-id`
- สร้าง manifest ที่มี:
  - `cardId`
  - `source`
  - `definitionHash`
  - `imageHash`
  - `dataVersion`
  - `language`
- ก่อนเล่น multiplayer ให้ตรวจว่า card pack ของทั้งสองฝั่งตรงกัน
- ใช้ระบบเดียวกันกับ Thai card database ที่เราดึงจาก KK Card Fight ได้

### 5. Effect handling

Manual ของ VangPro ระบุว่า effect ยังเป็น display text และ automatic effect execution ต้องทำแยก

แนวคิดที่ควรใช้กับระบบเรา:

- ระยะเริ่มต้นไม่ควรพยายามทำ auto-resolve ทุกสกิลทันที
- ให้ระบบเล่นเป็น manual simulator ก่อน:
  - draw
  - ride
  - call
  - attack
  - guard
  - damage check
  - trigger check
  - power modifier
  - retire
  - soul/counter blast
- Bot layer ใช้ action ที่ระบบรองรับก่อน
- ค่อยเพิ่ม effect parser/keyword engine ทีละกลุ่ม เช่น trigger, persona ride, overtrigger, simple on-place

### 6. Accessories / deck type

Dialog deck type มี:

- เพลเมท
- สลีฟ
- Crest
- Persona Shield

แนวคิดที่ควรใช้กับระบบเรา:

- แยก asset slot ออกจาก card/deck หลัก:
  - sleeve
  - playmat
  - marker/crest
  - persona shield
  - token
- เก็บใน deck metadata
- ไม่ให้ asset พิเศษปนกับ main deck validation

### 7. Loading tips / tutorial

ระหว่างโหลด VangPro แสดงภาพ tutorial/manga panel

แนวคิดที่ควรใช้กับระบบเรา:

- ระหว่างโหลด data/card images แสดง tip ตาม rule
- ใช้ tutorial panels สำหรับผู้เล่นใหม่
- ใส่ short guide เช่น ride deck, persona ride, trigger timing

### 8. Replay / notes

หน้า lobby มีปุ่ม replay และ notes

แนวคิดที่ควรใช้กับระบบเรา:

- เก็บ action log ทุกเกมเป็น event list
- Replay คือเล่น event list ย้อนกลับ
- Notes ใช้จด deck/game plan หรือ match note
- Bot training ใช้ replay/action log เป็นข้อมูลฝึก decision heuristic ได้

## โครงสร้างระบบที่แนะนำสำหรับเรา

```text
card-database/
  vanguard_th_cards_with_images.json
  images/
  packs/
    kk-vgth-251/
      manifest.json

deck-system/
  deck.json
  deck-code.ts|py
  validator.ts|py

game-engine/
  state.py
  actions.py
  rules.py
  replay_log.py

bot/
  mulligan.py
  ride_plan.py
  attack_plan.py
  guard_plan.py

ui/
  lobby
  deck_builder
  card_browser
  play_table
  replay_viewer
```

## Roadmap ที่แนะนำ

### Phase 1: Card browser + deck builder

- ใช้ข้อมูลจาก `outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json`
- ทำ browser ที่ค้นชื่อ/รหัส/series/clan/grade/text ได้
- ทำ deck builder พร้อม counter main/ride/G
- save/load deck เป็น JSON
- export/import deck code

### Phase 2: Manual play table

- ทำสนามเล่นแบบ manual
- รองรับ zones: deck, hand, ride deck, vanguard, rear-guard, drop, bind, soul, damage, order, trigger zone
- action log ทุกครั้ง
- replay จาก log

### Phase 3: Bot playable

- Bot อ่าน deck + game state
- เริ่มด้วย heuristic:
  - mulligan ตาม grade curve
  - ride ตาม ride line
  - call attacker/booster
  - attack order
  - guard threshold
- ยังไม่ auto-resolve skill ทั้งหมด

### Phase 4: Effect engine

- ทำ keyword/action DSL สำหรับ effect ที่พบบ่อย
- map effect text บางส่วนเข้า action template
- เพิ่มทีละ clan/series ที่เล่นบ่อย

### Phase 5: Multiplayer/local room

- เริ่มจาก local room ก่อน
- sync state ด้วย event log
- ก่อนเริ่มเกม ตรวจ card pack hash และ deck hash
- เพิ่ม spectator mode ภายหลัง

## ข้อควรระวัง

- ไม่ควรลอก asset/UI/code ของ VangPro
- ใช้เฉพาะแนวคิดระบบและ UX pattern
- ข้อมูลการ์ด/รูปที่เราดึงจาก KK Card Fight ควรใช้เพื่อโปรเจกต์ส่วนตัวก่อน และหลีกเลี่ยงการ redistribute สาธารณะโดยไม่มีสิทธิ์
- ระบบ custom card/multiplayer ต้องมี hash verification เพื่อป้องกันเด็คหรือรูปไม่ตรงกัน
