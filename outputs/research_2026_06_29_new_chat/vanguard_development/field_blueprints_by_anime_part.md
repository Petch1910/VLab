# Vanguard Field Blueprints by Anime Part

## Source References

Primary source registry: `source_references.md`

This blueprint uses these source IDs:

- `SRC-ANIME-OFFICIAL` - official animation/season naming
- `SRC-RULES-455` - current field, zone, circle, marker, pseudo-card, and set-specific rule model
- `SRC-RULES-119` through `SRC-RULES-230` - historical mechanics used to map older anime parts to field templates
- `SRC-CORPUS-MECHANICS` - mechanic presence by Comprehensive Rules version

เอกสารนี้ออกแบบ "สนามแข่ง" ของ Cardfight!! Vanguard แยกตาม 23 ภาคอนิเมะที่ใช้เป็นแกนจัดระบบ โดยมองจากมุมพัฒนาระบบจริง:

- UI ต้องวางอะไรให้ผู้เล่นเห็น
- engine ต้องเก็บ zone/state อะไร
- ภาคไหนใช้สนามเหมือนกัน และภาคไหนเป็น overlay เพิ่ม
- ถ้าทำ rule engine ไม่ควร hardcode เป็น 3x2 ตายตัว

> หมายเหตุ: สนามของ Vanguard เปลี่ยนแบบ "เพิ่มชั้นกติกา" มากกว่าเปลี่ยนกระดานใหม่ทุกภาค หลายภาคจึงใช้ field template เดียวกัน แต่เปิด mechanic overlay ต่างกัน

## 1. สัญลักษณ์ที่ใช้ในแผนผัง

```text
VC    = Vanguard Circle
RC    = Rear-guard Circle
FL    = Front Left
FC    = Front Center
FR    = Front Right
BL    = Back Left
BC    = Back Center
BR    = Back Right
GC    = Guardian Circle
DK    = Deck
HD    = Hand
DR    = Drop Zone
DMG   = Damage Zone
SOUL  = Soul under/associated with vanguard
BIND  = Bind Zone
TRG   = Trigger Zone
GZ    = G Zone
RDZ   = Ride Deck Zone
OZ    = Order Zone
OA    = Order Area
CZ    = Crest Zone
GAGE  = Gauge Zone
EN    = Energy value / Energy Generator
```

## 2. สนามแกนกลางที่ใช้เป็นฐาน

จากมุมมองผู้เล่น 1 คน:

```text
                     [GC]

Front Row:       [RC-FL] [VC]    [RC-FR]
Back Row:        [RC-BL] [RC-BC] [RC-BR]

Core Zones:      [DK] [HD] [DR] [DMG] [SOUL] [BIND] [TRG]
```

จากมุมมอง arena 2 ผู้เล่น:

```text
Opponent special zones / hand hidden

                     [OPP GC]
              [OPP RC-BR] [OPP RC-BC] [OPP RC-BL]
              [OPP RC-FR] [OPP VC]    [OPP RC-FL]

------------------------------------------------------------

              [RC-FL]     [VC]        [RC-FR]
              [RC-BL]     [RC-BC]     [RC-BR]
                     [GC]

Your hand / special zones
```

สำหรับ UI จริง แนะนำ layout หลัก:

```text
+----------------------------------------------------------+
| Opponent compact zones: hand count, deck, drop, damage    |
| Opponent special: GZ/RDZ/OZ/CZ only if format needs        |
|                                                          |
|                  Opponent field, mirrored                 |
|                                                          |
|---------------------- battle space -----------------------|
|                                                          |
|                     Your field                            |
|                                                          |
| Your core zones + special zones                           |
| Your hand                                                 |
+----------------------------------------------------------+
```

## 3. Template สนามหลัก

### FIELD-01: Classic Core

ใช้กับภาคแรก, Asia Circuit, Link Joker ก่อน overlay, และเป็นฐานของทุกยุค

```text
                     [GC]

                [RC-FL] [VC] [RC-FR]
                [RC-BL] [RC-BC] [RC-BR]

     [DK] [DR] [DMG] [SOUL] [BIND] [TRG]          [HD]
```

Engine requirement:

- `field.circles` มี 7 circles ต่อผู้เล่น
- `VC` เก็บ unit หลักและ soul association
- `GC` รองรับหลายการ์ดพร้อมกัน
- `TRG` เป็น temporary public zone ระหว่าง drive/damage check
- `DMG` ต้องเก็บ face-up/face-down state เพื่อ Counter-Blast

### FIELD-02: Classic + Lock Overlay

ใช้หลักใน Link Joker และยังต้องรองรับในกติกาปัจจุบัน

```text
                     [GC]

                [RC-FL] [VC] [LOCK]
                [LOCK]  [RC] [RC]

Locked circle:
  - circle type becomes lock
  - card is face down / locked state
  - cannot be used as normal unit circle
```

Engine requirement:

- `circle.type = lock`
- `circle.original_type` ต้องจำไว้เพื่อ unlock
- locked card ต้องถือว่าไม่มีข้อมูลการ์ดตามเงื่อนไข lock
- unlock timing โดยทั่วไปอยู่ช่วง end phase/rule processing

### FIELD-03: Legion Vanguard Circle

ใช้กับ Legion Mate และการ์ดที่มี Legion/D-Legion ในกติกาหลังจากนั้น

```text
                     [GC]

                [RC-FL] [VC: Leader + Mate] [RC-FR]
                [RC-BL] [RC-BC]             [RC-BR]

VC association:
  leader = attacking/reference vanguard
  mate   = associated vanguard on same VC
```

Engine requirement:

- `vanguard_circle.units = [leader, mate]`
- ทั้งสองใบเป็น vanguard แต่ target การโจมตีโดยปกติอ้าง leader
- ต้องเก็บ `legion_state`, `leader_id`, `mate_id`
- ถ้าใบใดใบหนึ่งออกจาก VC ความเป็น Legion ต้องจบตาม rule action

### FIELD-04: G / Stride Field

ใช้กับ Vanguard G ถึง G Z

```text
                     [GC]

                [RC-FL] [VC: G Unit] [RC-FR]
                [RC-BL] [RC-BC]      [RC-BR]

VC underlay:
  heart card(s) under / associated with G Unit

Special Zones:
  [GZ face down] [GZ face up]
```

Guard step with G Guardian:

```text
[GZ face down G Guardian] -> [GC] -> [GZ face up]
```

Engine requirement:

- `g_zone.cards` แยก face-down / face-up
- `stride_state.active = true`
- `vanguard_circle.top_unit = g_unit`
- `vanguard_circle.heart_cards[]`
- end phase ต้องคืน G Unit เข้า G Zone face up
- Generation Break นับ face-up G units ใน VC/GZ ตามกติกา

### FIELD-05: V Reboot + Imaginary Gift

ใช้กับ Vanguard 2018, High School Arc, Shinemon Arc, Extra Story -if-

```text
                     [GC]

       [Accel RC?] [RC-FL] [VC] [RC-FR] [Accel RC?]
                  [RC-BL] [RC-BC] [RC-BR]

Markers:
  Force marker  -> on VC/RC
  Accel marker  -> creates extra front-row RC
  Protect I     -> pseudo-card in hand
  Protect II    -> marker on RC
```

Engine requirement:

- `circle.markers[]`
- `field.dynamic_circles[]` สำหรับ Accel
- `pseudo_cards[]` สำหรับ Protect I
- UI ต้องยอมให้ front row ยาวกว่า 3 ช่อง
- marker อยู่กับ circle ไม่ใช่ unit ยกเว้น card text ระบุ behavior เฉพาะ

### FIELD-06: D / overDress Standard

ใช้กับ overDress และ will+Dress

```text
                     [GC]

                [RC-FL] [VC] [RC-FR]
                [RC-BL] [RC] [RC-BR]

Special Zones:
  [RDZ] [OZ] [OA] [CZ optional]

RC stack:
  top unit
  originalDress / xoverDress materials
```

Engine requirement:

- `ride_deck_zone.cards`
- `order_zone.cards`
- `order_area.cards`
- `crest_zone.cards` optional แต่ควรเตรียมไว้
- `unit.associations.original_dress[]`
- overDress/XoverDress ไม่ควรถือว่าเป็น normal call

### FIELD-07: DZ / DivineZ Field

ใช้กับ Divinez, DELUXE, Parallactic Clash

```text
                     [GC]

                [RC-FL] [VC] [RC-FR]
                [RC-BL] [RC] [RC-BR]

Special Zones:
  [RDZ] [OZ] [CZ: Energy Generator / Crest]
  EN = numeric resource

Player state:
  used_divine_skill = false/true
```

Engine requirement:

- `player.energy`
- `player.energy_limit`
- `energy_generator` อยู่ใน Crest Zone/crest related object
- `used_divine_skill` เป็น player-level flag
- Divine Skill เป็น once-per-game restriction ไม่ใช่ zone ใหม่
- Crest และ ride deck crest ควรเป็น pseudo/card-like object แยกจาก normal card

### FIELD-08: Set-specific Overlay Field

ไม่ได้ผูกกับภาคอนิเมะเดียว แต่เป็นสิ่งที่ engine ต้องรองรับเพราะการ์ดบางชุดสร้าง field behavior เฉพาะ

```text
Astral Plane:
  Circle type += astral_plane

Music:
  Music order can be placed on circle temporarily

Prison:
  Order Zone has prison card + imprisoned cards

Arms:
  Arms order associated with Vanguard Circle

Gauge:
  Gauge Zone has cards associated with unit/player

Stage:
  Some gained circles can become stage type
```

Engine requirement:

- `circle.types[]` ต้องรองรับมากกว่า 1 type
- `associated_objects[]` ต้องใช้กับ unit/circle/player
- `order_zone.sub_areas` เช่น prison/imprisoned/scouted
- `gauge_zone.associations`

## 4. Mapping สนามตาม 23 ภาค

| # | ภาคอนิเมะ | Field template | สนามที่ควรแสดงใน UI | Overlay สำคัญ |
|---:|---|---|---|---|
| 1 | Cardfight!! Vanguard | FIELD-01 | classic 7 circles + core zones | basic soul/damage/trigger |
| 2 | Asia Circuit | FIELD-01 | classic 7 circles + core zones | Limit Break เป็น condition ไม่ใช่ zone |
| 3 | Link Joker | FIELD-02 | classic field พร้อม lock state | Lock / Unlock |
| 4 | Legion Mate | FIELD-03 | classic field แต่ VC รองรับ 2 vanguards | Legion leader/mate |
| 5 | Cardfight!! Vanguard G | FIELD-04 | classic field + G Zone | Stride / Heart / GB |
| 6 | G: GIRS Crisis | FIELD-04 | classic field + G Zone | Stride / G assist style support |
| 7 | G: Stride Gate | FIELD-04 | classic field + G Zone | Stride / G Guardian |
| 8 | G NEXT | FIELD-04 | classic field + G Zone | GB, clan keywords, G Guardian |
| 9 | G Z | FIELD-04 | classic field + G Zone | Ultimate Stride / Zeroth Dragon / Gyze related cards |
| 10 | Cardfight!! Vanguard 2018 | FIELD-05 | classic field + Gift marker area | Force/Accel/Protect |
| 11 | High School Arc Cont. | FIELD-05 | same as V field | Imaginary Gift |
| 12 | Shinemon Arc | FIELD-05 + FIELD-08 | V field, optional Astral Plane | Imaginary Gift, Astral Plane support |
| 13 | Extra Story -if- | FIELD-05 | V field | Imaginary Gift, token/pseudo-card support |
| 14 | overDress Season 1 | FIELD-06 | D field + Ride Deck + Order Zone | Ride Deck, Persona Ride, overDress |
| 15 | overDress Season 2 | FIELD-06 | D field + Ride Deck + Order Zone | overDress/XoverDress style support |
| 16 | will+Dress Season 1 | FIELD-06 | D field + Ride Deck + Order Zone | Orders, Persona Ride, Dress mechanics |
| 17 | will+Dress Season 2 | FIELD-06 | D field + Ride Deck + Order Zone | more ride line specific mechanics |
| 18 | will+Dress Season 3 | FIELD-06 | D field + Ride Deck + Order Zone | Crest/order support should be ready |
| 19 | Divinez Season 1 | FIELD-07 | D/DZ field + Crest Zone + Energy | Energy, Divine Skill, Fated One |
| 20 | Divinez Season 2 | FIELD-07 | D/DZ field + Crest Zone + Energy | Energy, Divine Skill, Destined One |
| 21 | Divinez DELUXE Arc | FIELD-07 | DZ field | Energy/Crest + deck-specific mechanics |
| 22 | Divinez DELUXE Finals | FIELD-07 | DZ field | Energy/Crest + deck-specific mechanics |
| 23 | Divinez Parallactic Clash | FIELD-07 + FIELD-08 | DZ field, extensible overlays | Energy/Crest, Fantome/Parallactic-era card modules |

## 5. แบบสนามรายภาค

### 1. Cardfight!! Vanguard

```text
                     [GC]
                [RC-FL] [VC] [RC-FR]
                [RC-BL] [RC] [RC-BR]

Visible zones: [DK] [DR] [DMG] [SOUL] [BIND] [TRG] [HD]
Hidden/optional: none
```

ออกแบบ UI:

- ใช้สนาม 3x2 + GC
- soul แสดงเป็น stack ใต้ VC หรือ panel ข้าง VC
- damage zone ต้องคลิกพลิก face-up/face-down ได้
- trigger zone แสดงเฉพาะตอน check แล้วค่อยย้ายไป hand/damage/drop ตามผล

### 2. Asia Circuit

```text
Same as FIELD-01

Extra UI badges:
  LB4 / LB5 condition indicator from damage count
```

ออกแบบ UI:

- สนามเหมือนภาคแรก
- เพิ่ม condition badge บนการ์ด/ผู้เล่น เช่น `Limit Break active: damage >= 4`
- ไม่ต้องเพิ่ม zone

### 3. Link Joker

```text
                     [GC]
                [RC-FL] [VC] [LOCK]
                [LOCK]  [RC] [RC-BR]

Visible zones: classic zones
Extra state: locked circle/cards
```

ออกแบบ UI:

- circle ที่ถูก lock ต้องเปลี่ยนภาพเป็น face-down lock panel
- ห้ามลาก/เลือกเป็น unit ปกติ
- ต้องมี unlock animation/state transition ตอน end phase
- lock ไม่ใช่ zone ใหม่ แต่เปลี่ยน type ของ circle ชั่วคราว

### 4. Legion Mate

```text
                     [GC]
                [RC-FL] [VC: Leader + Mate] [RC-FR]
                [RC-BL] [RC]                [RC-BR]
```

ออกแบบ UI:

- VC ต้องแสดงการ์ด 2 ใบแบบคู่ ไม่ใช่ stack ทับจนหาย
- leader ควรอยู่ด้านหน้า/ซ้าย และ mate ด้านหลัง/ขวา
- power display ต้องแยก base/current ของ leader และ power contribution จาก mate
- target marker ควรชี้ว่าโจมตี vanguard หลัก ไม่ใช่ mate โดยตรง

### 5. Cardfight!! Vanguard G

```text
                     [GC]
                [RC-FL] [VC: G Unit] [RC-FR]
                [RC-BL] [RC]         [RC-BR]

Special: [GZ face down] [GZ face up]
VC: G Unit + Heart card(s)
```

ออกแบบ UI:

- G Zone เป็น panel แยก เห็นจำนวน face-down/face-up
- เมื่อ stride ให้ G Unit อยู่บน VC และ heart อยู่ใต้/หลัง
- UI ต้องแสดงชื่อ/power ที่ได้จาก heart
- end phase ต้องย้าย G Unit กลับ G Zone face up

### 6. G: GIRS Crisis

```text
Same as FIELD-04
```

ออกแบบ UI:

- ใช้ G field เต็มรูปแบบ
- ต้องมี Stride Step เป็นส่วนหนึ่งของ turn UI
- G Zone ต้องรองรับการเปิด face-up ตาม cost/effect

### 7. G: Stride Gate

```text
                     [GC: normal guardians or G Guardian]
                [RC-FL] [VC: G Unit] [RC-FR]
                [RC-BL] [RC]         [RC-BR]

G Guardian path:
  GZ -> GC -> GZ face up
```

ออกแบบ UI:

- Guard step ต้องเปิด action "call G Guardian"
- GC ต้องรองรับ G Unit ชั่วคราว
- หลัง guard จบ G Guardian ไม่ไป drop แต่กลับ G Zone face up

### 8. G NEXT

```text
Same as FIELD-04
```

ออกแบบ UI:

- เหมือน G field
- เพิ่ม badge เงื่อนไข clan keyword เช่น Brave, Ritual, Bloom, Charge, Darkness ฯลฯ ใน card script layer
- keyword ส่วนใหญ่ไม่เปลี่ยนสนาม แต่เปลี่ยน condition/effect

### 9. G Z

```text
Same as FIELD-04

Additional display:
  Ultimate Stride / Zeroth Dragon / Gyze related condition panels
```

ออกแบบ UI:

- G Zone ยังเป็นแกนหลัก
- ต้องรองรับ G Unit พิเศษและ Ultimate Stride cost
- Gyze/Zeroth Dragon เป็น card/module เฉพาะ ไม่ควรเปลี่ยน core field

### 10. Cardfight!! Vanguard 2018

```text
       [Accel RC?] [RC-FL] [VC] [RC-FR] [Accel RC?]
                  [RC-BL] [RC] [RC-BR]
                     [GC]

Markers: Force / Accel / Protect
```

ออกแบบ UI:

- เพิ่ม marker layer บน circle
- Accel ต้องสร้าง circle ใหม่ใน front row
- Protect I ต้องสร้าง pseudo-card ใน hand
- Protect II เป็น marker บน RC

### 11. High School Arc Cont.

```text
Same as FIELD-05
```

ออกแบบ UI:

- ใช้ V field เหมือนภาค 2018
- ทำ gift marker panel แยกจาก card zones
- UI ควรมี history ว่าได้ gift จากการ ride กี่ครั้ง

### 12. Shinemon Arc

```text
V Field + optional Astral Plane

                [RC-FL] [VC] [RC-FR]
                [RC-BL] [ASTRAL/RC] [RC-BR]
```

ออกแบบ UI:

- ใช้ V field เป็นฐาน
- ต้องเตรียม circle type เพิ่ม เช่น `astral_plane`
- Astral Plane ไม่ใช่ RC ธรรมดา เพราะ unit บนนั้นโจมตี/drive check ได้ตามกติกาเฉพาะ

### 13. Extra Story -if-

```text
Same as FIELD-05
```

ออกแบบ UI:

- V field + pseudo-card/token support
- ไม่ควรผูกกติกา -if- กับสนามใหม่ ให้ทำเป็น card/module

### 14. overDress Season 1

```text
                     [GC]
                [RC-FL] [VC] [RC-FR]
                [RC-BL] [RC] [RC-BR]

Special: [RDZ] [OZ] [OA]
RC stack: top unit + originalDress
```

ออกแบบ UI:

- Ride Deck Zone ต้องอยู่ชัด เพราะเป็น flow หลักทุกเกม
- Order Zone ต้องวางใกล้ field แต่แยกจาก unit circles
- overDress unit บน RC ต้องเปิดดู originalDress ใต้การ์ดได้

### 15. overDress Season 2

```text
Same as FIELD-06

RC:
  top = overDress/XoverDress unit
  under = specified originalDress cards
```

ออกแบบ UI:

- เพิ่ม stack viewer บน RC
- action placement ต้องแยก `call` กับ `overdress`
- originalDress ไม่ใช่ soul และไม่ใช่ gauge

### 16. will+Dress Season 1

```text
Same as FIELD-06

Special emphasis:
  [RDZ] [OZ] [Persona Ride display]
```

ออกแบบ UI:

- แสดง Persona Ride status ใน player status bar
- Order Zone ต้องรองรับ set order หลายแบบ
- Deck-specific mechanics ให้เป็น module ต่อจาก field เดิม

### 17. will+Dress Season 2

```text
Same as FIELD-06
```

ออกแบบ UI:

- สนามไม่เปลี่ยนจาก will+Dress Season 1
- เพิ่ม card-script module ตาม ride line/deck archetype
- UI ต้องรองรับ order, token, stack, associated cards พร้อมกัน

### 18. will+Dress Season 3

```text
Same as FIELD-06, with Crest Zone prepared

Special: [RDZ] [OZ] [CZ optional]
```

ออกแบบ UI:

- เตรียม Crest Zone แม้บาง match จะไม่ใช้
- ให้ซ่อน/ย่อ CZ ถ้าไม่มี crest เพื่อไม่รก
- เหมาะกับการใช้ layout เดียวกับ D/DZ ตั้งแต่ตรงนี้ไป

### 19. Divinez Season 1

```text
                     [GC]
                [RC-FL] [VC] [RC-FR]
                [RC-BL] [RC] [RC-BR]

Special: [RDZ] [OZ] [CZ: Energy Generator]
Resource: EN = 0..limit
Flag: Divine Skill unused/used
```

ออกแบบ UI:

- Energy เป็น numeric resource ไม่ใช่วงกลมบนสนาม
- Energy Generator/Crest ควรแสดงใน Crest Zone
- Divine Skill ต้องเป็นปุ่ม/flag once per game
- Fated One/Destined One เป็น card module ไม่ใช่สนามใหม่

### 20. Divinez Season 2

```text
Same as FIELD-07
```

ออกแบบ UI:

- ใช้ DZ field เหมือน Season 1
- เพิ่ม state สำหรับ Destined One/Divine Skill interactions
- battle UI ต้องรองรับ ability ที่อ้าง energy และ divine skill

### 21. Divinez DELUXE Arc

```text
Same as FIELD-07
```

ออกแบบ UI:

- สนามหลักยังเป็น DZ field
- tournament/deck variety สูง ควรเปิด plugin/module ต่อ deck
- แสดง Crest/Energy/Order Zone เป็น default

### 22. Divinez DELUXE Finals

```text
Same as FIELD-07
```

ออกแบบ UI:

- ไม่ต้องสร้างสนามใหม่จาก DELUXE Finals
- ใช้ DZ field + card modules
- เพิ่ม match/tournament metadata ได้ แต่ไม่ใช่กติกาสนาม

### 23. Divinez Parallactic Clash

```text
DZ Field + extensible overlay layer

                     [GC]
                [RC-FL] [VC] [RC-FR]
                [RC-BL] [RC] [RC-BR]

Special: [RDZ] [OZ] [CZ] [EN]
Optional overlay: Fantome / Parallactic-era card mechanics
```

ออกแบบ UI:

- ใช้ FIELD-07 เป็นแกน
- ห้าม hardcode ว่า DZ มีแค่ Energy/Divine Skill เพราะ card text รุ่นใหม่อาจเพิ่ม associated object
- ให้มี `mechanic_slots[]` หรือ `overlay_panels[]` สำหรับ module เฉพาะชุด
- ถ้า mechanic ไม่มี zone ใหม่ ให้เก็บเป็น player/card state แทน

## 6. Component Design สำหรับ UI

### CoreBoard component

```text
CoreBoard
  - VanguardCircle
  - RearGuardGrid
  - GuardianCircle
  - CoreZonesPanel
  - HandPanel
```

ใช้ทุกภาค

### SpecialZonesPanel component

```text
SpecialZonesPanel
  - GZonePanel
  - RideDeckPanel
  - OrderZonePanel
  - CrestZonePanel
  - GaugePanel
  - EnergyCounter
```

เปิด/ปิดตาม field template

### CircleLayer component

```text
CircleLayer
  - UnitCard
  - MarkerLayer
  - LockOverlay
  - AssociationStack
  - CircleTypeBadge
```

จำเป็นสำหรับ Lock, Legion, Stride, Imaginary Gift, Astral Plane, overDress

### PlayerStateBar component

```text
PlayerStateBar
  - damage count
  - counter-blast availability
  - soul count
  - generation break count
  - persona ride active
  - energy count
  - divine skill used/unused
```

## 7. Data Model ที่แนะนำ

```text
BoardState
  players[2]
  active_player_id
  turn_phase
  battle_state
  format_profile
  enabled_modules[]

PlayerBoard
  field
  zones
  resources
  flags
  visible_ui_panels[]

Field
  base_circles[]
  dynamic_circles[]
  guardian_circle

Circle
  id
  owner
  row
  column
  base_type
  current_types[]
  cards[]
  markers[]
  associations[]
  state

Zones
  deck
  hand
  drop
  damage
  soul
  bind
  trigger
  g_zone
  ride_deck
  order_zone
  order_area
  crest_zone
  gauge_zone

Resources
  energy
  energy_limit

Flags
  persona_ride_this_turn
  used_divine_skill
```

## 8. กฎออกแบบสำคัญสำหรับระบบ

1. อย่าเก็บสนามเป็น array `3x2` อย่างเดียว เพราะ Accel, Astral Plane, stage, dynamic circle จะพังทันที
2. VC ต้องรองรับมากกว่า 1 object เพราะ Legion และ Stride/Heart ใช้ association
3. RC ต้องรองรับ stack/associated cards เพราะ overDress, gauge, Buddy Soul, Arms-like mechanics
4. Marker ต้องแยกจาก card เพราะ Force/Accel/Protect/Crest pseudo-card มี rule ของตัวเอง
5. Zone ที่ไม่ใช้ใน format นั้นควรซ่อนใน UI แต่ engine ควรรองรับเป็น optional module
6. Card-specific field change ต้องทำเป็น overlay module ไม่แก้ core board ทุกครั้ง

## 9. Source Notes

- Official animation page checked for current/past listed seasons from G Stride Gate through Divinez Parallactic Clash: https://en.cf-vanguard.com/animation/
- Field/zone model cross-checked against Comprehensive Rules 4.55 in the local corpus: `../vanguard_rules_markdown/versions/13_dz_rules_4_55.md`
- This file is a development blueprint, not a replacement for the official Comprehensive Rules.
