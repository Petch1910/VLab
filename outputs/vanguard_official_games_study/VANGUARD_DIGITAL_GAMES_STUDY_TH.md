# Vanguard Digital Games Study

วันที่ศึกษา: 2026-06-19

เกมที่ศึกษา:

- Cardfight!! Vanguard EX
- Cardfight!! Vanguard Dear Days
- Cardfight!! Vanguard Dear Days 2

เป้าหมาย: เก็บแนวคิดระบบที่เหมาะเอามาเสริมระบบใหม่ของเรา ซึ่งต่อยอดจากข้อมูล Vanguard TH + รูปการ์ดที่ดึงจาก KK Card Fight และแนวคิดจาก VangPro

## แหล่งข้อมูลหลัก

- Vanguard EX official site: `https://www.cs.furyu.jp/vanguard-ex/`
- Dear Days Steam page: `https://store.steampowered.com/app/1881420/Cardfight_Vanguard_Dear_Days/`
- Dear Days 2 Steam page: `https://store.steampowered.com/app/2457540/Cardfight_Vanguard_Dear_Days_2/`
- Dear Days 2 official site: `https://vgdd2.cf-vanguard.com/en/`
- Dear Days 2 Nintendo page: `https://www.nintendo.com/us/store/products/cardfight-vanguard-dear-days-2-switch/`

## 1. Cardfight!! Vanguard EX

### สิ่งที่ยืนยันจาก official/FuRyu

- Platform: Nintendo Switch / PlayStation 4
- Genre: Vanguard simulator
- มีตัวเอก original ชื่อ `士導イズル / Izuru Shidou`
- มี original story เฉพาะเกม
- ใช้กฎเหมือนการ์ดจริงในยุคนั้น
- มีการ์ดล่าสุดในเวลานั้น
- มี network battle

### ไอเดียที่ควรเอามาใช้

#### Story as tutorial/progression

EX ไม่ได้เป็นแค่ simulator เปล่า ๆ แต่ใส่ original protagonist และ story progression เพื่อค่อย ๆ พาผู้เล่นเข้าเกม

สำหรับระบบเรา:

- ทำ `Bot Campaign` เป็นเส้นทางฝึกเล่น
- เริ่มจาก deck ง่าย เช่น Trial Deck
- เพิ่ม mission:
  - ride ให้ครบ grade
  - attack order ให้ถูก
  - guard ให้รอด
  - ใช้ trigger timing
  - ใช้ persona ride
- ให้ bot แต่ละตัวมี deck archetype และนิสัยการเล่นต่างกัน

#### NPC fight library

แทนที่จะมีบอทตัวเดียว ควรทำ NPC profile:

```json
{
  "npc_id": "kai_style_kagero",
  "name": "Kagero Pressure Bot",
  "deck_id": "bt01_kagero_sample",
  "difficulty": 2,
  "style": ["retire", "pressure", "vanguard_attack_first"]
}
```

ข้อดี:

- ใช้ทดสอบบอทได้หลาย matchup
- ใช้เป็น campaign ได้
- ใช้ฝึกผู้เล่นใหม่ได้

## 2. Cardfight!! Vanguard Dear Days

### สิ่งที่ยืนยันจาก official/Steam

- Release: 2022-11-16
- Platform: Steam / Nintendo Switch
- มี original story ในโลก overDress
- มี Standard format มากกว่า 1,000 cards
- มี game-original animations
- Modes:
  - Ranked Fight
  - CPU Fight
  - Friend Fight
  - Story mode
- มี tutorial สำหรับผู้เล่นใหม่
- Collection/customization:
  - characters
  - sleeves
  - playmats
- มี DLC card sets และ cosmetic sets

### ไอเดียที่ควรเอามาใช้

#### One engine, many modes

Dear Days ใช้ fight engine เดียว แต่เปิดหลายโหมด: story, CPU, ranked, friend

สำหรับระบบเรา:

```text
game-engine/
  rules
  actions
  validators
  state
  replay

modes/
  solo_test
  cpu_fight
  bot_campaign
  local_room
  replay_viewer
```

สิ่งสำคัญคืออย่าแยก logic ของแต่ละ mode จนซ้ำกัน ให้ทุก mode ใช้ action/state format เดียว

#### Tutorial แบบ point-by-point

Dear Days เน้นให้ beginner เข้าเล่นได้

สำหรับระบบเรา:

- ทำ tutorial overlay บนสนามจริง
- Highlight zone ที่ต้องกด
- Bot/ระบบเสนอ action ที่ถูกต้อง
- มี lesson ย่อย:
  - Stand & Draw
  - Ride
  - Call
  - Battle phase
  - Drive check
  - Damage check
  - Guard
  - Trigger effects
- Tutorial ควรบันทึกเป็น script/event ได้ เพื่อ replay และ test

#### Collection ไม่ควรปนกับ deck logic

Dear Days มี collection/cosmetics เช่น sleeve/playmat

สำหรับระบบเรา:

- แยก `deck cards` ออกจาก `deck cosmetics`
- deck metadata:

```json
{
  "deck_id": "my_deck",
  "format": "D",
  "main": [],
  "ride": [],
  "g": [],
  "cosmetics": {
    "sleeve": "default_blue",
    "playmat": "dark_grid",
    "marker": "crest_default"
  }
}
```

## 3. Cardfight!! Vanguard Dear Days 2

### สิ่งที่ยืนยันจาก official/Steam/Nintendo

- Release: 2025-01-30
- Platform: Steam / Nintendo Switch
- มี over 3,000 cards สำหรับ Standard format
- มี original story และ story ที่โยงกับ will+Dress / DivineZ
- Modes:
  - Ranked Fight
  - Free Fight
  - Room Fight
  - CPU Fight
  - Story mode
- มี point-by-point tutorial
- มี collection/customization เช่น characters, sleeves, playmats
- Steam page ระบุ storage 16 GB

### ไอเดียที่ควรเอามาใช้

#### Format/version-aware card database

Dear Days 2 สำคัญเพราะ card pool ใหญ่ขึ้นมาก และเข้าสู่ DZ/DivineZ

ระบบเราควร version ข้อมูลแบบนี้:

```json
{
  "source": "kk-cardfight-vgth",
  "source_card_version": 251,
  "ruleset": "thai-vanguard",
  "formats": ["D", "V", "Premium"],
  "cards": []
}
```

ต้องรองรับ:

- card pool version
- ban/restriction version
- rule version
- image version
- deck created version

เวลา import deck code ต้องรู้ว่า deck ถูกสร้างจาก data version ไหน

#### Room Fight มากกว่า Friend Fight

Dear Days 2 เปลี่ยน/เพิ่มแนวคิดเป็น Room Fight ชัดเจนกว่าแค่ friend fight

สำหรับระบบเรา:

- เริ่มจาก local room ก่อน
- room state:

```json
{
  "room_id": "ABCD",
  "format": "D",
  "players": [],
  "spectators": [],
  "deck_hash_required": false,
  "card_pack_hash": "..."
}
```

- รองรับ spectator ภายหลัง
- ก่อนเริ่มเกมตรวจ:
  - card database hash
  - custom card pack hash
  - deck validity

#### Quick/intuitive fight

Dear Days 2 เน้น quick/intuitive play

ระบบเราไม่ควรทำ manual simulator ที่กดเยอะเกินไป ควรมี smart actions:

- คลิก deck = draw ถ้าอยู่ draw phase
- คลิก hand card = show legal actions
- คลิก attacker = show attack targets
- คลิก trigger = เลือก power target + draw/crit/front/heal behavior
- ปุ่ม shortcut:
  - end phase
  - auto shuffle
  - reveal top
  - send to drop
  - add power
  - undo last action

#### Better beginner/returning player support

Dear Days 2 บอกชัดว่าผู้เล่นใหม่หรือผู้เล่นเก่ากลับมาเรียนจาก tutorial ได้

สำหรับระบบเรา:

- เพิ่ม `coach mode`
- Bot อธิบายเหตุผลของ action:
  - ทำไมควร ride ใบนี้
  - ทำไมควรโจมตี vanguard ก่อน
  - ทำไมควร guard หรือไม่ guard
- เพิ่ม `mistake warning`:
  - deck main ยังไม่ครบ 50
  - ride deck ไม่ครบ
  - trigger ratio ผิด
  - ใช้ card format ผิด

## Feature Matrix

| ระบบ | EX | Dear Days | Dear Days 2 | VangPro | ควรทำในระบบเรา |
|---|---:|---:|---:|---:|---|
| Original/story mode | มี | มี | มี | ไม่ใช่จุดหลัก | Bot campaign |
| CPU fight | น่าจะมีจาก simulator/story | มี | มี | มี | ต้องมี |
| Ranked online | มี network fight | มี | มี | มี room/network | ภายหลัง |
| Room/Friend fight | network fight | Friend Fight | Room Fight | สร้างห้อง | Phase 5 |
| Tutorial | story/learning | มี | point-by-point | loading tips/manual | Phase 2 |
| Deck builder | มี | มี | มี | ดีมาก | Phase 1 |
| Deck code/share | ไม่ยืนยัน | ไม่ชัด | ไม่ชัด | มี | Phase 1 |
| Cosmetics | ไม่ใช่จุดหลัก | characters/sleeves/playmats | characters/sleeves/playmats | sleeves/playmat/crest | Phase 3 |
| Custom card import | ไม่ยืนยัน | ไม่มีใน official | ไม่มีใน official | มีชัด | Phase 2-3 |
| Replay | ไม่ยืนยัน | ไม่ชัด | ไม่ชัด | มีปุ่ม replay | Phase 2 |
| Large versioned DB | ยุค V | 1,000+ | 3,000+ | remote images/data | ต้องมีตั้งแต่แรก |

## Architecture ที่ควรใช้

### Core principle

ทำ `single source of truth` สำหรับ game state แล้วให้ทุก mode ใช้เหมือนกัน

```text
card database
  -> deck builder
  -> deck validator
  -> game state
  -> action engine
  -> replay log
  -> bot decision
  -> UI
```

### Action log

ทุกการเล่นควรเป็น event:

```json
{
  "turn": 3,
  "phase": "battle",
  "actor": "player",
  "action": "attack",
  "source": "front_left",
  "target": "opponent_vanguard",
  "metadata": {
    "power": 18000
  }
}
```

ข้อดี:

- ทำ replay ได้
- undo ได้
- debug bot ได้
- sync multiplayer ได้
- ใช้ train heuristic ได้

### Deck code

ควรใช้ format ประมาณ:

```text
VGTH1.<base64url(zstd/json)>
```

ข้อมูลด้านใน:

```json
{
  "format": "D",
  "source": "kk-vgth",
  "version": 251,
  "main": {"BT01-001TH": 4},
  "ride": {},
  "g": {},
  "cosmetics": {}
}
```

### Card pack manifest

เพราะเรามีรูปทุกใบแล้ว ควรทำ manifest เพิ่ม:

```json
{
  "pack_id": "kk-vgth-251",
  "card_count": 10836,
  "image_count": 10836,
  "definition_hash": "...",
  "image_manifest_hash": "...",
  "created_at": "..."
}
```

## Roadmap รวมหลังศึกษา VangPro + EX + Dear Days + Dear Days 2

### Phase 1: Data + Browser + Deck Builder

- ใช้ `vanguard_th_cards_with_images.json`
- Card browser ค้นจากชื่อ/รหัส/text/series/clan/nation/grade
- Deck builder:
  - main/ride/g counters
  - format selector
  - rule validator
  - save/load JSON
  - copy/apply deck code
  - export deck image

### Phase 2: Manual Simulator + Tutorial

- สนามเล่น manual
- action log
- undo/replay
- tutorial script
- coach hints

### Phase 3: Bot CPU Fight

- Bot heuristic:
  - mulligan
  - ride choice
  - call attacker/booster
  - attack target/order
  - guard threshold
- Difficulty profiles
- NPC/campaign fights

### Phase 4: Custom Pack System

- Import `.csv/.xlsx + images.zip`
- manifest + hash
- local custom card database
- custom cards show in browser/deck builder

### Phase 5: Room Fight / Multiplayer Foundation

- local room first
- event sync
- deck/card pack hash check
- spectator/replay viewer

### Phase 6: Effect Engine

- เริ่มจาก common action templates:
  - draw
  - retire
  - search
  - power +X
  - counter blast
  - soul blast
  - persona ride
  - trigger effects
- ไม่ต้อง parse Thai text ทั้งหมดทันที

## สรุปเชิงออกแบบ

สิ่งที่ควรเอาจากแต่ละเกม:

- จาก EX: story progression + NPC campaign
- จาก Dear Days: official-style tutorial + CPU/Ranked/Friend mode split
- จาก Dear Days 2: large versioned database + room fight + beginner support
- จาก VangPro: deck code, custom card import, replay, accessory slots, deck builder layout

ทิศทางระบบเราควรเป็น:

```text
Vanguard Area-style manual freedom
+ Dear Days-style tutorial/CPU/story
+ VangPro-style deck builder/import/share
+ Our bot layer
+ Thai card database/images from KK Card Fight
```

นี่จะทำให้ระบบเราไม่ใช่แค่ clone โปรแกรมเดิม แต่เป็น simulator + deck lab + bot training environment สำหรับ Vanguard ภาษาไทย
