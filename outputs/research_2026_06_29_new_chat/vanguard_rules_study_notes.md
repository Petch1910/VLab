# Cardfight!! Vanguard Rules Study Notes

## Source References

Primary source registry: `vanguard_development/source_references.md`

These notes are based on:

- `SRC-RULES-119` through `SRC-RULES-455` - official Comprehensive Rules PDFs converted into local Markdown
- `SRC-CORPUS-INDEX` - reading order and PDF source table
- `SRC-CORPUS-MECHANICS` - mechanic presence by rule version
- `SRC-CORPUS-HEADINGS` - section lookup across rule versions

อ่านจากชุด Markdown ที่แปลงจาก official Comprehensive Rules PDF ใน `outputs/vanguard_rules_markdown/`

สถานะ: first structural pass ครอบคลุม `1.19 -> 4.55` และเจาะอ่านส่วน core engine, battle, ability processing, keyword evolution, D/DZ rule architecture

## 1. ภาพรวม rule engine

Vanguard ไม่ได้เป็นแค่เกม "โจมตีให้ครบ 6 damage" แต่เป็น rule engine ที่ประกอบด้วยชั้นหลัก:

1. Card information: ชื่อ, clan/nation, grade, power, shield, critical, trigger icon, type, skill icon, text
2. Zones: deck, hand, drop, field/circles, soul, damage, bind, trigger, G zone, gauge, order area/zone, ride deck, crest zone
3. Unit placement: ride, call, legion, stride, overDress/XoverDress, attach, alchemix
4. Turn progression: stand, draw, G assist, ride, stride, main, battle, end
5. Battle resolution: attack step, guard step, drive step, damage step, close step
6. Ability/effect engine: ACT/AUTO/CONT, cost, trigger, check timing, play timing, replacement/continuous/one-shot effects
7. Rule actions: losing, overloaded units, illegal guardians/orders/gauge/energy, no vanguard, damage resolution
8. Keyword layer: Limit Break, Legion, Stride, Imaginary Gift, overDress, Energy, Divine Skill, title-specific rules

## 2. แกนกติกาที่มีตั้งแต่ยุคเก่า

### Winning / Losing

- ผู้เล่นแพ้ทันทีเมื่อ rule action ตรวจพบ losing condition
- เงื่อนไขหลัก: damage zone มี 6 ใบขึ้นไป, deck ไม่มีการ์ด, กติกาปัจจุบันเพิ่มกรณีไม่มี vanguard และไม่มี soul
- ผู้เล่น concede ได้ทันที และเอฟเฟกต์ไม่สามารถขัดขวางหรือบังคับ concession ได้
- ถ้าทั้งสองฝ่ายแพ้พร้อมกัน เกมเสมอ

### Golden rules

- ถ้ากติกากับ text การ์ดขัดกัน ให้ text การ์ดชนะ
- ถ้าทำได้บางส่วน ให้ทำเท่าที่ทำได้
- ถ้ามีเอฟเฟกต์ "ทำ" กับ "ห้ามทำ" ชนกัน เอฟเฟกต์ห้ามทำชนะ
- ถ้าผู้เล่นหลายคนต้องเลือกพร้อมกัน turn player เลือกก่อน แล้ว non-turn player เลือกทีหลังโดยรู้คำตอบของ turn player

## 3. Card / Unit model

### ข้อมูลบนการ์ด

- `name`: ใช้เช็กชื่อเดียวกัน, Persona Ride, DressUp, specific support
- `clan / nation`: ยุคเก่าใช้ clan เป็นแกน, ยุค D/DZ ใช้ nation เป็นแกน แต่กติกาปัจจุบันยัง map clan/nation เพื่อรองรับ Premium
- `race`: ใช้กับเอฟเฟกต์เฉพาะ เช่น Astral Deity
- `grade`: จำกัด ride/call และใช้เป็นเงื่อนไขจำนวนมาก
- `power`: ใช้เทียบ hit
- `shield`: ใช้บวกให้ unit ที่ถูกโจมตีจาก guardian
- `critical`: จำนวน damage ที่ทำกับ vanguard ถ้า hit
- `trigger icon`: กำหนด trigger ability
- `drive`: จำนวน drive check ของ vanguard/บาง unit

### Unit states / associated states

- stand/rest
- face up/face down
- locked/deleted
- legion state / D-legion state
- heart state
- originalDress state
- imprisoned state
- friend/harmony/charging/hollowed/blazing/engorged/successful ฯลฯ ตาม keyword

## 4. Zones

โซนพื้นฐาน:

- `deck zone`: hidden, ลำดับห้ามดู/เปลี่ยนเว้นแต่ rule/effect ให้ทำ
- `hand`: hidden ต่อคู่ต่อสู้
- `drop zone`: public
- `field`: รวม circles
- `vanguard circle`: unit หลัก
- `rear-guard circles`: unit สนับสนุน/โจมตี
- `guardian circle`: guard ระหว่าง battle
- `soul`: ใต้ vanguard, ใช้จ่าย Soul-Blast หรือเงื่อนไข
- `damage zone`: ทรัพยากร Counter-Blast และตัวนับแพ้
- `bind zone`: ใช้กับ Narukami/Gear Chronicle/Alestiel และกลไกอื่น
- `trigger zone`: โซนชั่วคราวสำหรับ drive/damage check

โซนเพิ่มตามยุค:

- `G zone`: G units/G guardians
- `gauge zone`: gauge/plant gauge
- `order area / order zone`: order, set order, prison, song, world, arms
- `ride deck zone`: ride deck ยุค D/DZ
- `crest zone`: crest / Energy Generator / ride deck crest

หลักสำคัญ: การ์ดที่ย้ายข้ามโซนโดยทั่วไปถือเป็น object ใหม่ และเอฟเฟกต์เดิมหลุด เว้นแต่เป็นการย้าย circle-to-circle หรือกติกาเฉพาะระบุไว้

## 5. Deck / game setup

### ยุคเก่า 1.19

- deck 50 ใบ
- trigger 16 ใบพอดี
- heal trigger สูงสุด 4
- Sentinel สูงสุด 4
- first vanguard เลือก grade 0 จาก deck วางคว่ำบน VC
- มือเริ่ม 5 ใบ แล้ว mulligan โดยสับกลับ deck และจั่วคืน

### ปัจจุบัน 4.55

- เตรียม main deck, ได้สูงสุด 1 ride deck, ได้สูงสุด 1 G deck
- main deck 50 ใบ ไม่รวม subtype G
- ride deck มีอย่างน้อย 4 ใบตาม ride deck construction
- G deck สูงสุด 16 ใบ subtype G
- main deck + ride deck รวมกันต้องมี trigger 16 ใบ
- heal รวมสูงสุด 4, over trigger รวมสูงสุด 1
- Sentinel รวมสูงสุด 4
- Regalis Piece รวมสูงสุด 1
- first vanguard มาจาก ride deck ถ้ามี ถ้าไม่มีจึงเลือกจาก main deck

## 6. Turn structure

โครงปัจจุบัน:

1. Stand Phase
2. Draw Phase
3. G Assist Step
4. Ride Phase
5. Ride Step
6. Stride Step
7. Main Phase
8. Battle Phase
9. End Phase

สิ่งที่สำคัญ:

- เริ่มแต่ละ phase มักมี automatic abilities เข้า standby แล้ว resolve check timing
- Main Phase คือช่วง normal call, สลับตำแหน่ง rear-guard ใน column, เล่น non-blitz order, ใช้ ACT, ทำ overDress
- Battle Phase เข้า attack sub-phase ซ้ำได้จน turn player เลือกไม่โจมตี
- End Phase คืน G unit เข้า G zone หงาย, unlock/undelete, จัดการ music order/attached unit, เคลียร์ duration จนจบเทิร์น

## 7. Battle model

Battle แบ่งเป็น:

1. Start Step: เลือกว่าจะโจมตีหรือไม่
2. Attack Step: เลือก attacking unit, rest, เลือกเป้าหมาย, boost
3. Guard Step: ฝ่ายรับ call guardian / G Guardian / intercept / blitz order
4. Drive Step: ถ้า attacking unit เป็น vanguard หรือได้ drive จาก effect
5. Damage Step: เทียบ power, hit, damage check, retire guardian/rear-guard ที่ถูก hit
6. Close Step: resolve "at end of battle", เคลียร์สถานะ attacking/being attacked

หลัก hit:

- attack hit ถ้า power ฝ่ายโจมตี >= power ฝ่ายรับหลัง guardian/shield
- ถ้าโจมตี vanguard แล้ว hit จะทำ damage ตาม critical
- ถ้าโจมตี rear-guard แล้ว hit จะ retire rear-guard แต่ไม่มี damage check
- ถ้า unit สำคัญย้ายออกจาก circle ระหว่าง battle กติกาจะยังพยายามดำเนิน battle ต่อ แต่บางส่วนเช่น power comparison/damage อาจไม่เกิดตามเงื่อนไข

## 8. Trigger model

Trigger เป็น ability จาก trigger icon เมื่อการ์ดเข้า trigger zone จาก drive/damage check

ยุคเก่า:

- Critical: +1 critical และ +5000
- Draw: draw 1 และ +5000
- Stand: stand rear-guard และ +5000
- Heal: ถ้า damage เราไม่น้อยกว่าคู่ต่อสู้ heal 1 และ +5000

ยุค V/D/DZ:

- power ตามเลขบน trigger icon ปกติ +10000
- Front Trigger เพิ่ม power ให้ front row ทั้งหมด
- Over Trigger: remove ตัวเอง, draw 1, ให้ +100000000, และถ้าออกจาก drive check ทำ text เพิ่ม

จุดเปลี่ยน:

- Stand Trigger เป็นของ OG/G เป็นหลัก
- Front Trigger เข้ามาใน V
- Over Trigger เข้ามาใน D

## 9. Ability / effect engine

### Ability types

- `ACT`: ผู้เล่นเลือก play ตอนมี play timing และจ่าย cost
- `AUTO`: trigger จาก event/situation แล้วเข้า standby, resolve ใน check timing
- `CONT`: ทำงานต่อเนื่องตราบใดที่ active

### Effect types

- one-shot effect: ทำทันทีแล้วจบ เช่น draw, retire
- continuous effect: มี duration เช่น until end of turn/battle
- replacement effect: แทน event หนึ่งด้วย event อื่น

### Check timing

ลำดับโดยรวม:

1. resolve rule actions
2. resolve standby AUTO ของ turn player ทีละอัน
3. resolve standby AUTO ของ non-turn player ทีละอัน
4. ถ้าเกิด rule action/AUTO ใหม่ กลับไปตรวจอีก

### Cost

- cost ต้องจ่ายครบจึง play/resolve ส่วนนั้นได้
- cost หลายส่วนมักจ่ายพร้อมกัน
- ถ้าจ่ายไม่ได้ ห้ามเล่น ability/card นั้น
- AUTO ที่มี cost จ่ายตอน resolve

### Choice

- ถ้าเขียน `choose N` ต้องเลือกให้ครบเท่าที่ทำได้
- ถ้าเขียน `up to N` เลือกได้ตั้งแต่ 0 ถึง N
- hidden zone search อาจเลือกไม่เจอได้ แม้จริง ๆ มีการ์ดที่ตรงเงื่อนไข
- public zone search ถ้ามีของตรงเงื่อนไขต้องเลือก

## 10. Specific actions / resource model

Core actions:

- draw, discard, retire, place, ride, call, heal, reveal, search, shuffle
- stand/rest, turn face up/down
- drive check, damage check
- give/lose/get abilities
- bind, lock, unlock, delete

Resource actions:

- `Counter-Blast`: พลิก damage หงายเป็นคว่ำ
- `Counter-Charge`: พลิก damage คว่ำเป็นหงาย
- `Soul-Blast`: ย้ายจาก soul ไป drop
- `Soul-Charge`: ย้ายใบบนสุด deck เข้า soul
- `Energy-Charge`: เพิ่ม energy ถึง upper limit
- `Energy-Blast`: ลด energy ตามจำนวน ถ้ามีไม่พอทำไม่ได้

## 11. Rule actions

Rule action คือสิ่งที่เกมบังคับเองเมื่อถึง check timing หรือเมื่อกติกาเฉพาะให้ตรวจทันที

ตัวอย่าง:

- losing the game
- overloaded vanguard/rear-guard: มีหลาย unit ใน circle เดียวเกินกติกา
- illegal guardians: guardian อยู่นอก battle
- having no vanguard: กติกาปัจจุบันมีการแก้กรณีไม่มี vanguard และ soul
- damage application/resolution
- erasure of pseudo-cards
- illegal gauge/order/attached card
- excess energy

## 12. Mechanics by era

### OG / Link Joker layer

- `Limit Break`: ability active เมื่อ damage zone มีจำนวนถึงที่กำหนด เช่น LB4
- `Forerunner`: grade 0 starter call ตัวเองลง RC เมื่อ ride ด้วย unit clan เดียวกัน
- `Lord`: ถ้ามี unit คนละ clan กับ Lord ตัวนั้นโจมตีไม่ได้
- `Sentinel`: จำกัด deck และมักเป็น perfect guard
- `Lock/Unlock`: เปลี่ยน card/circle เป็น locked state, หงายกลับช่วง end phase
- `Delete`: ทำให้ vanguard ถูก delete ตามกติกาเฉพาะ

Effect patterns ที่ไม่ใช่หัวข้อ keyword แยกใน rules ปัจจุบัน:

- `Break Ride`: LB4 + เมื่อถูก ride ทับ ให้ vanguard ใหม่ power +10000 และ effect
- `Cross Ride`: ได้ power เพิ่มเมื่อ soul มีร่างก่อนหน้าชื่อที่กำหนด
- `Ultimate Break`: Limit Break 5
- `Mega Blast`: cost ใหญ่ยุคแรก เช่น CB5/SB8
- `Persona Blast`: ใช้การ์ดชื่อเดียวกันเป็น cost

### Legion Mate layer

- `Legion`: รวม vanguard 2 ใบเป็น Legion Leader + Legion Mate
- ทั้งคู่ถือเป็น vanguard แต่โจมตี/ถูกโจมตีมี rule เฉพาะ
- ตอน Legion attack จะ rest พร้อมกัน และ power ของ mate ถูกบวกให้ leader ระหว่างโจมตี
- `Seek Mate`: ACT ที่ search mate จาก deck แล้ว legion โดยมี cost คืนการ์ดจาก drop เข้า deck
- `D-Seek Mate / D-Legion`: เวอร์ชัน D/DZ ใหม่ มี cost/ข้อจำกัดต่างจาก Seek Mate เก่า

### G layer

- `Stride`: เอา G unit จาก G zone วางบน VC ชั่วคราว ไม่ถือว่า ride
- vanguard เดิมกลายเป็น `Heart`
- G unit ได้ชื่อและ power ของ heart ที่เลือก
- จบเทิร์น G unit กลับ G zone แบบ face up
- `Generation Break`: active ตามจำนวน face-up G units บน VC/G zone
- `G Guardian`: call G unit จาก G zone ไป guardian circle ระหว่าง guard step
- `Ultimate Stride`: stride พิเศษ เช่น Zeroth/Gyze, cost/เงื่อนไขไม่เหมือน normal stride

G-era keywords:

- Brave, Oracle, Rescue, Ritual, Unite, Revelation, Blaze, Afterimage
- Dominate, Engorge, Shadowstitch, Thunderstrike, Rush, Burst, Charge
- Darkness, Magia, Time Leap, Hollow, Harmony, Wave, Dark Device, Success, Bloom

### V layer

- `Imaginary Gift`: Force / Accel / Protect
- `Front Trigger`: front row ได้ power
- trigger power โดยทั่วไปขยับเป็น +10000
- V rules ยังเก็บ core engine เดิม แต่เพิ่ม marker/pseudo-card handling

Imaginary Gift โดยย่อ:

- Force I: circle ได้ +10000 during your turn
- Force II: original critical ของ unit บน circle กลายเป็น 2
- Accel I: เพิ่ม front row RC และ +10000
- Accel II: เพิ่ม front row RC, draw 1, marker ให้ +5000
- Protect I: สร้าง protect pseudo-card ใน hand
- Protect II: marker บน RC ให้ intercept ได้ shield +10000

### D / DZ layer

- `Ride Deck`: ride จาก ride deck zone และ main/ride deck share copy/trigger/sentinel limits
- `Persona Ride`: ride ชื่อเดียวกันที่มี persona ride icon
- `Over Trigger`: trigger พิเศษ +100000000 และ remove
- `Order / Set Order / Blitz Order`: order limit ปกติ 1 ต่อเทิร์น, blitz ใช้ตอน guard
- `overDress`: วาง unit ทับ rear-guard เป็น originalDress โดยไม่ถือว่า call
- `XoverDress`: ใช้ specified cards เป็น originalDress
- `Final Rush / Final Burst`: player state ที่ effect อื่นอ้างอิง
- `World`: Dark Night / Abyssal Dark Night / Stray Garden
- `Alchemagic`: order handling เฉพาะของ Stoicheia/Alchemagic
- `Energy`: Energy-charge / Energy-blast, excess energy เป็น rule action
- `Divine Skill`: restriction icon ใช้แล้วผู้เล่นใช้ ability icon นี้อีกไม่ได้จนจบเกม
- `Regalis Piece`: deck construction + resolution limit

D/DZ keyword additions:

- DressBoost, Powerful, Friend, Glitter, RevolDress, Cannonball, DressUp
- UnisonDress, Overcharge, Malwyrm, Ace Unit, Rewrite, Mental Pollution
- Happy Toys, SweetLink, Arma Arms, Fantôme Skill

## 13. Markers / pseudo-cards

กติกาปัจจุบันแยก marker ออกเป็น pseudo-cards และ records

ตัวอย่าง:

- Protect pseudo-card
- token unit
- token set order
- ticket pseudo-card
- crest pseudo-card
- record marker
- gift marker

หลักสำคัญ:

- pseudo-card มี effective zone และ erase zone
- ถ้า pseudo-card ย้ายไปโซนที่ไม่ควรอยู่ มักถูก erased
- token unit อยู่บน field เท่านั้น ถ้าไป soul/drop/อื่น ๆ จะถูกจัดการตาม erase rule

## 14. Set-specific rules

กติกาปัจจุบันมีหมวดเฉพาะสำหรับสินค้า/ธีมบางชุด:

- Gyze card
- Astral Plane circles
- Music Orders
- Imprison / Prison
- Songs and Singing
- Arms Orders / Armed
- SHAMAN KING / Völundr
- Scout
- Products and Operating
- Mushiking
- Touken Ranbu
- The Quintessential Quintuplets
- CoroCoro Collab Pack
- Buddyfight

หมวดนี้ไม่ใช่ core engine ของทุกเกม แต่สำคัญเมื่อต้องรองรับ card pool ทั้งหมดใน Premium/Standard ที่มี title mechanics

## 15. แนวคิดสำหรับทำฐานข้อมูล/engine

ควรแยกข้อมูลเป็นชั้น:

```text
card
card_version
deck_construction_rule
zone
game_object
state
ability
effect
cost
condition
timing_event
specific_action
keyword
mechanic_group
rule_action
format_rule
```

สำหรับ ability ควรแยก field:

```text
ability_type: ACT / AUTO / CONT
effective_zone
restriction_icon
keyword_condition
trigger_event
condition_text
cost_text
effect_text
duration
source_object
era
formal_keyword
mechanic_group
```

ตัวอย่าง:

```text
formal_keyword: Limit Break
mechanic_group: Break Ride
ability_type: AUTO
zone: VC
trigger_event: when ridden upon
condition: damage >= 4
effect: new vanguard gets +10000 and extra ability until end of turn
```

## 16. สิ่งที่ต้องระวังจากการอ่าน Markdown

- PDF icon glyph บางตัวสกัดเป็นช่องว่าง จึงต้องเปิด PDF ต้นฉบับเทียบเมื่อเจอ icon สำคัญ
- คำบางคำใน matrix อาจติด false positive เช่น `Force` อาจหมายถึงคำทั่วไป ไม่ใช่ Imaginary Gift เสมอ
- Comprehensive Rules ปัจจุบันรวมทุกยุคไว้ด้วยกัน แต่การเล่นจริงขึ้นกับ format/card pool
- บาง mechanic เป็น keyword ทางการ บาง mechanic เป็นชื่อที่ผู้เล่นเรียกจาก pattern เช่น Break Ride

## 17. สรุปความเข้าใจรอบแรก

Vanguard มีแกนหลักเดียวตั้งแต่ยุคแรก: วาง Vanguard, ride เพิ่ม grade, call rear-guard, โจมตีผ่าน battle substeps, เปิด trigger จาก drive/damage check, ใช้ damage/soul/hand เป็น resource

ระบบแต่ละยุคไม่ได้แทนที่แกนหลัก แต่เพิ่ม layer:

- OG เพิ่ม Limit Break/Lock
- Legion เพิ่ม Vanguard สองใบ
- G เพิ่ม G zone/Stride/Heart/Generation Break
- V เพิ่ม Imaginary Gift/Front Trigger
- D เพิ่ม Ride Deck/Persona Ride/Order/Over Trigger/overDress
- DZ เพิ่ม Energy/Divine Skill และ keyword ใหม่

ดังนั้นวิธีเรียนที่ถูกคือจำ core engine ก่อน แล้วค่อยเปิด mechanic layer ตามยุค ไม่ควรเริ่มจาก keyword list ทั้งหมดใน 4.55 ทันที
