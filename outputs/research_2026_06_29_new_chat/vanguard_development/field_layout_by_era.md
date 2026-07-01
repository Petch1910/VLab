# Vanguard Field / Zone Layout by Era

## Source References

Primary source registry: `source_references.md`

This field/zone layout uses these source IDs:

- `SRC-RULES-455` - current zone definitions, circle types, pseudo-cards, markers, set-specific rules, Energy, and Crest Zone
- `SRC-RULES-119` - baseline classic field, Limit Break-era core, Lock/Unlock
- `SRC-RULES-131` - Legion/Seek Mate
- `SRC-RULES-138` through `SRC-RULES-148` - Stride, Heart, G Zone, G Guardian, Ultimate Stride
- `SRC-RULES-201` through `SRC-RULES-230` - V-era Imaginary Gift, Force, Accel, Protect
- `SRC-CORPUS-MECHANICS` - mechanic appearance by rule version

เอกสารนี้แยก "สนามในเชิงกติกา" ตามยุค เพื่อใช้พัฒนา UI, game state, board model และ rule engine

## 1. คำตอบสั้น

ใช่ สนาม/พื้นที่เล่นของ Vanguard ในแต่ละยุค **ไม่เหมือนกันทั้งหมด** แต่ไม่ได้เปลี่ยนแบบรื้อกระดานหลักทุกภาค

แกนหลักที่คงอยู่เกือบตลอดคือ:

```text
Front Row:  [RC] [VC] [RC]
Back Row:   [RC] [RC] [RC]
Guardian Circle: แยกต่างหาก ใช้ตอน guard
```

สิ่งที่เปลี่ยนตามยุคคือ:

- มีโซนเสริมเพิ่ม เช่น G Zone, Ride Deck Zone, Order Zone, Crest Zone
- มี marker/circle เพิ่ม เช่น Accel circle
- Vanguard Circle รองรับสภาพซ้อนพิเศษ เช่น Legion, Heart/Stride, Arms
- Rear-guard Circle รองรับสภาพซ้อนพิเศษ เช่น originalDress, gauge, imprisoned interactions
- บาง deck/format ใช้บางโซน บาง format ไม่ใช้

## 2. Baseline Field Model

สำหรับผู้เล่น 1 คน ควร model สนามเป็น:

```text
                    [Guardian Circle]

Front Row:          [Front Left RC] [Vanguard Circle] [Front Right RC]
Back Row:           [Back Left RC]  [Back Center RC]  [Back Right RC]

Off-board zones:    Deck / Hand / Drop / Damage / Soul / Bind / Trigger
```

ในกติกาเก่า guardian circle ถูกนับเป็น circle เพิ่มหนึ่งช่อง ทำให้ field มี 7 circles:

- 3 front-row circles
- 3 back-row circles
- 1 guardian circle

ในกติกาปัจจุบันควร model เป็น circle ที่มี `circle_type`:

```text
vanguard
rear_guard
guardian
lock
astral_plane
stage
```

## 3. OG / Aichi / Asia Circuit / Link Joker

ใช้กับภาค:

- Cardfight!! Vanguard
- Asia Circuit
- Link Joker

### โซนที่ต้องมี

```text
Deck
Hand
Drop Zone
Field / Circles
Soul
Damage Zone
Bind Zone
Trigger Zone
```

### สนามหลัก

```text
GC

RC  VC  RC
RC  RC  RC
```

### สิ่งที่เปลี่ยนใน Link Joker

เพิ่มสภาพ `Lock`:

- card ถูกพลิกคว่ำเป็น locked card
- circle นั้นกลายเป็น lock type circle
- locked card ไม่มีข้อมูลการ์ดเดิมตามกติกา lock
- unlock ตอน end phase แล้วถือว่าไม่ใช่ card เดิมเชิงกติกาบางส่วน

สำหรับระบบ:

```text
Circle.state = normal | locked
Card.state includes locked
Circle.original_type must be remembered for unlock
```

## 4. Legion Mate

สนามพื้นฐานเหมือน OG แต่ Vanguard Circle ต้องรองรับ Vanguard 2 ใบ

```text
VC stack/association:
  Legion Leader
  Legion Mate
```

หลักสำคัญ:

- ทั้งสองใบถือเป็น vanguard
- Legion Mate ไม่ถูกโจมตีโดยตรง
- ตอนโจมตี ใช้ความสัมพันธ์ leader/mate และ power ของ mate ถูกบวกให้ leader ตาม timing กติกา
- ถ้าใบใดใบหนึ่งย้ายโซน ความเป็น legion จบ

สำหรับระบบ:

```text
VanguardCircle.units = [leader, mate]
association.type = legion | d_legion
association.leader_id
association.mate_id
```

## 5. G / Stride Era

ใช้กับ:

- Vanguard G
- GIRS Crisis
- Stride Gate
- G NEXT
- G Z

### เพิ่มโซน

```text
G Zone
```

### Vanguard Circle ต้องรองรับ Stride

เมื่อ Stride:

```text
VC:
  top: G Unit
  underneath/associated: Heart card(s)
```

หลักสำคัญ:

- Stride ไม่ใช่ ride
- Vanguard เดิมกลายเป็น heart
- G Unit ได้ชื่อและ power ของ heart ที่เลือก
- จบเทิร์น G Unit กลับ G Zone face up
- face-up G units เปิด Generation Break

### Guardian Circle กับ G Guardian

Guard Step ต้องรองรับ call G Guardian จาก G Zone:

```text
G Zone face down -> Guardian Circle -> G Zone face up
```

สำหรับระบบ:

```text
zones.g_zone.cards
Card.subtype includes G
Card.state face_down / face_up in G zone
VanguardCircle.top_unit
VanguardCircle.heart_cards
```

## 6. V Reboot

ใช้กับ:

- Vanguard 2018
- High School Arc
- Shinemon Arc
- Extra Story -if-

สนามพื้นฐานยังเหมือนเดิม แต่เพิ่มระบบ marker

### Imaginary Gift

ต้องมี marker system:

```text
Force marker
Accel marker
Protect pseudo-card / Protect marker
```

### Force

วาง marker บน VC หรือ RC เดิม

```text
Circle.markers += Force I / Force II
```

### Accel

สร้าง rear-guard circle เพิ่มใน front row

```text
Front Row: [Accel RC] [RC] [VC] [RC] [Accel RC] ...
Back Row:          [RC] [RC] [RC]
```

กติกาปัจจุบันอธิบายเป็นการได้ circle เพิ่มและอาจมี stage type circle ในบางกรณี

สำหรับระบบ:

```text
Field.dynamic_circles[]
Circle.source = base | accel | stage | effect
Circle.row = front
Circle.type = rear_guard
```

### Protect

Protect I สร้าง pseudo-card ใน hand  
Protect II วาง marker บน rear-guard circle

สำหรับระบบ:

```text
PseudoCard.type = protect
Circle.markers += Protect II
```

## 7. D / overDress / will+Dress

ใช้กับ:

- overDress
- will+Dress

### เพิ่มโซนสำคัญ

```text
Ride Deck Zone
Order Zone
Order Area
Crest Zone
```

ใน Standard ปัจจุบัน มักต้องมี ride deck และอาจมี crest/energy generator ตามยุค

### Ride Deck Zone

ไม่ใช่สนามต่อสู้ แต่เป็นโซนสำคัญสำหรับ ride

```text
RideDeckZone.cards
Card.face_state: face_down / face_up
```

### Order Zone / Order Area

ใช้กับ:

- Set Order
- Prison
- World
- Song
- Product/Operating
- Token Set Order

สำหรับระบบ:

```text
OrderZone.cards
OrderArea.temporary_cards
Order.limit_per_turn = usually 1
```

### overDress / originalDress

Rear-guard circle ต้องรองรับการซ้อน originalDress:

```text
RC:
  top: overDress unit
  associated: originalDress card(s)
```

overDress ไม่ถือว่า call

สำหรับระบบ:

```text
Unit.associations.originalDress[]
PlacementAction.type = overdress
is_call = false
```

### XoverDress

คล้าย overDress แต่ใช้ specified cards เป็น originalDress และมีข้อกำหนดเฉพาะ

## 8. DZ / DivineZ

ใช้กับ:

- Divinez
- Divinez Season 2
- DELUXE
- Parallactic Clash

สนามพื้นฐานเหมือน D แต่เพิ่มความสำคัญของ:

```text
Crest Zone
Energy value
Energy Generator
Divine Skill restriction state
```

### Crest Zone

ใช้วาง crest / ride deck crest / Energy Generator

```text
CrestZone.cards
Player.energy_value
Player.energy_upper_limit
```

### Energy

Energy ไม่ใช่ card circle แต่เป็น resource value ของ player

```text
Energy-Charge -> increase player.energy
Energy-Blast -> decrease player.energy
Excess Energy -> rule action
```

### Divine Skill

ไม่ใช่ zone แต่เป็น player-level once-per-game restriction

```text
Player.used_divine_skill = true
```

## 9. Set-specific Field Changes

กติกาปัจจุบันมี field/zone behavior เฉพาะบางชุด:

### Astral Plane

Circle ได้ type `Astral Plane`

- เรียก Astral Deity ลงได้โดย ignore grade requirement
- unit บน circle นี้โจมตีได้แม้อยู่ back row
- unit บน circle นี้ perform drive checks
- end phase เอา unit กลับก้น deck

### Music Orders

Music order วางบน back row center circle ชั่วคราว

- ไม่ถือเป็น unit/rear-guard
- end phase ไป drop

### Prison / Imprison

การ์ดถูกนำไปไว้ใน order zone ใน imprisoned state

```text
OrderZone.prison_card
OrderZone.imprisoned_cards[]
```

### Arms Orders

Arms order อาจถูกวาง/associated กับ vanguard circle

```text
Vanguard.associated_arms[]
```

### Gauge

Gauge ไม่ใช่ circle แต่เป็น associated cards ใน gauge zone หรือใต้/กับ unit ตามกติกาเก่า/ปัจจุบัน

```text
GaugeZone.cards associated with unit/player
```

## 10. ตารางสรุปตามยุค

| ยุค | Base 6 unit circles + GC | โซน/สนามเพิ่ม | สิ่งที่ UI/engine ต้องรองรับ |
|---|---:|---|---|
| OG | ใช่ | ไม่มีโซนพิเศษใหญ่ | VC, RC, GC, soul, damage, trigger |
| Asia Circuit | ใช่ | เหมือน OG | Limit Break เป็น condition ไม่ใช่สนาม |
| Link Joker | ใช่ | Lock circle state | locked card/circle |
| Legion Mate | ใช่ | VC มี 2 vanguard | leader/mate association |
| G | ใช่ | G Zone | stride, heart, face-up G count |
| G Guardian | ใช่ | G Zone -> GC | G Guardian call/return |
| V | ใช่ | Imaginary Gift markers, Accel extra circles | dynamic front-row RC, gift markers |
| D | ใช่ | Ride Deck, Order Zone, Crest Zone | ride deck, set orders, overDress stack |
| DZ | ใช่ | Energy/Crest emphasis | energy value, divine skill, new crest behavior |
| Title-specific | แล้วแต่ card | Astral Plane, Music, Prison, Arms, Gauge | circle type/association extensions |

## 11. Recommended Data Model

```text
Field
- base_circles: Circle[]
- dynamic_circles: Circle[]
- guardian_circle: Circle

Circle
- id
- owner_player_id
- base_position
- row: front | back | guardian | none
- column: left | center | right | dynamic
- type: vanguard | rear_guard | guardian | lock | astral_plane | stage
- original_type
- markers[]
- cards[]
- associated_objects[]

PlayerBoard
- field
- deck
- hand
- drop
- damage
- soul
- bind
- trigger
- g_zone
- ride_deck_zone
- order_zone
- order_area
- crest_zone
- gauge_zone
- energy
```

## 12. Implementation Advice

อย่า hardcode สนามเป็น array 3x2 แบบตายตัว เพราะจะรองรับ Accel, Astral Plane, Music Order, Prison, Arms, overDress และ future mechanics ยาก

ควรทำเป็น:

```text
base circles + dynamic circles + zone extensions + card associations
```

และให้แต่ละ mechanic module เพิ่ม state/marker/association ให้สนาม แทนการแก้ core board ทุกครั้ง
