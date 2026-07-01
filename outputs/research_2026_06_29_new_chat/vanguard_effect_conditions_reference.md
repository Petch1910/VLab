# Cardfight!! Vanguard - Effect Conditions / Keyword Reference

## Source References

Primary source registry: `vanguard_development/source_references.md`

This reference is based on:

- `SRC-RULES-455` - current keyword, timing, ability, marker, pseudo-card, and set-specific rule definitions
- `SRC-RULES-119` through `SRC-RULES-230` - historical keywords/patterns such as Limit Break, Break Ride, Legion, Stride, and Imaginary Gift
- `SRC-CARDLIST` / `SRC-CARDSEARCH` - individual card text examples and card wording patterns
- `SRC-CORPUS-MECHANICS` - mechanic presence across rule versions

ตรวจจาก Cardfight!! Vanguard Comprehensive Rules ver. 4.55, official Card List และตัวอย่างการ์ดเก่า/ใหม่ ณ 29 มิ.ย. 2026

## 1. โครงสร้างเอฟเฟกต์ที่ควรแยกเวลาอ่านการ์ด

รูปแบบทั่วไป:

```text
[AUTO](RC)[1/Turn]: When this unit attacks, if you have a grade 3 or greater vanguard,
COST [Counter-Blast 1], this unit gets Power +10000 until end of that battle.
```

ควรแยกเป็นช่อง:

| ช่อง | ความหมาย |
|---|---|
| Ability type | `AUTO`, `ACT`, `CONT` |
| Zone | `(VC)`, `(RC)`, `(GC)`, hand, soul, drop, bind, order zone, G zone |
| Restriction | `[1/Turn]`, once per game, Divine Skill, Regalis Piece |
| Trigger/timing | When placed, when attacks, at end of battle, during your turn |
| Condition | if Persona Ride, if GB1, if damage 4+, if soul 5+, etc. |
| Cost | COST `[Counter-Blast 1]`, discard, Soul-Blast, Energy-Blast |
| Effect | draw, call, retire, bind, power +, critical +, stand, ride |
| Duration | until end of battle, until end of turn, during this game |

## 2. ประเภทสกิลหลัก

| คำ | ความหมาย |
|---|---|
| `AUTO` | สกิลอัตโนมัติ เกิดเมื่อ trigger condition ตรง |
| `ACT` | สกิลที่ผู้เล่นเลือกกดใช้เองตอนมี play timing |
| `CONT` | สกิลต่อเนื่อง ทำงานตราบใดที่เงื่อนไขยังจริง |
| Trigger ability | เอฟเฟกต์จาก trigger ระหว่าง drive/damage check |
| Order ability | เอฟเฟกต์จาก Normal/Blitz/Set Order |

## 3. Keyword abilities ทางการใน Section 14

### Basic / Core

| Keyword | เงื่อนไขหรือผลหลัก |
|---|---|
| Drive Abilities | เพิ่ม drive เช่น Twin Drive, Triple Drive |
| Intercept | ยูนิตแถวหน้าขยับไป Guardian Circle ได้ตอน guard |
| Boost | ยูนิตแถวหลัง rest เพื่อบวก power ให้ตัวโจมตี |
| Restraint | ยูนิตโจมตีไม่ได้ |
| Lord | ถ้ามี unit คนละ clan กับตัวที่มี Lord ตัวนั้นโจมตีไม่ได้ |
| Sentinel | จำกัดใส่ deck ได้สูงสุด 4 ใบ และมักเป็น perfect guard |
| Resist | opponent's card effects cannot choose card นี้ |

### OG / Limit Break / Legion

| Keyword | เงื่อนไขหรือผลหลัก |
|---|---|
| Limit Break | ability เปิดเมื่อ damage zone มีจำนวนที่ระบุขึ้นไป เช่น LB4 |
| Forerunner | เมื่อ unit clan เดียวกัน ride ทับ สามารถ call ตัวนี้ลง RC |
| Seek Mate / D-Seek Mate | ability สำหรับทำ Legion กับคู่ที่ระบุ |

### G Era / Stride / Clan Keywords

| Keyword | เงื่อนไขหรือผลหลัก |
|---|---|
| Stride Skill | ระบุ timing/cost เพื่อ normal stride G unit |
| Ultimate Stride Skill | stride แบบพิเศษ เช่น Zeroth Dragon/Gyze |
| Generation Break | เปิดเมื่อมี face up G unit บน VC/G zone ตามจำนวน |
| G Guardian | call G unit จาก G zone มา GC ตอน guard |
| Brave | มือมี 3 ใบหรือน้อยกว่า |
| Oracle | มือมี 5 ใบขึ้นไป |
| Rescue | heal damage แล้ว deal damage ให้ vanguard ตัวเอง |
| Ritual | drop zone มี grade 1 ตามจำนวน |
| Unite | เทิร์นนั้น call ลง RC/GC รวม 2 ใบขึ้นไป |
| Revelation | ตอน placed ดูใบบน deck อาจใส่ soul ถ้าใส่ต้อง rest rear-guard |
| Blaze | ถ้า rear-guard เรามากกว่าคู่ต่อสู้ vanguard becomes blazing |
| Afterimage | เมื่อการ์ดเข้ามือ opponent จาก bind zone และมือเราน้อยพอ อาจคืน unit เข้ามือ |
| Dominate | คุม unit ฝ่ายตรงข้ามให้โจมตี/กระทำตาม effect |
| Engorge | ตอนโจมตี retire rear-guard เราเพื่อให้ unit becomes engorged |
| Shadowstitch | กลุ่ม ability ที่อ้างอิง attack against vanguard does not hit |
| Thunderstrike | opponent's bind zone มีการ์ดตามจำนวน |
| Rush | trigger เมื่อ unit stand ด้วย effect ของการ์ดเรา |
| Burst | อ้างอิง power ของ unit ว่าสูงถึงค่าที่กำหนดหรือไม่ |
| Charge | unit ที่ call ด้วย effect becomes charging และมักกลับก้น deck หลัง battle |
| Darkness | มีการ์ดเข้า soul ในเทิร์นนั้น นอกจากการ ride |
| Magia | call จาก soul แล้ว end turn ใส่กลับ soul |
| Time Leap | นำ unit ไป bind แล้ว call grade +1 จาก deck ชั่วคราว |
| Hollow | unit becomes hollowed; end turn retire hollowed units |
| Harmony | unit ในคอลัมน์เดียวกันเข้าสถานะ harmony |
| Wave | เปิดเฉพาะ battle ลำดับที่ระบุ เช่น 3rd battle |
| Dark Device | opponent ไม่มี standing unit ในคอลัมน์เดียวกัน |
| Success | unit becomes successful เมื่อ rear-guard power ถึงค่าที่กำหนด |
| Bloom | trigger เมื่อ unit ชื่อ/เงื่อนไขเดียวกันถูก placed |
| Melody | แชร์ ability ให้ units with Melody ฝั่งเดียวกัน |

### D / DZ Era Keywords

| Keyword | เงื่อนไขหรือผลหลัก |
|---|---|
| overDress | วาง unit ทับ rear-guard ตามเงื่อนไข ไม่ถือว่า call |
| White Wings / Black Wings | bind zone เป็น grade คี่ทั้งหมด/คู่ทั้งหมด และต้องมี Alestiel |
| DressBoost | อ้างอิง armed card เมื่อ boost |
| Powerful | soul มี 0 ใบ |
| Friend | มีอีก unit ในคอลัมน์เดียวกัน ทำให้เข้าสถานะ friends |
| Glitter | อ้างอิง vanguard ชื่อที่ระบุ เช่น Glitter - Rorowa |
| RevolDress | หลัง attack ride unit จากโซนอื่นแบบ stand |
| XoverDress | วาง unit ลง RC โดยใช้ specified cards เป็น originalDress |
| Cannonball | automatic ability ที่มีเงื่อนไขเกี่ยวกับ attack against vanguard |
| Regalis Piece | จำกัด deck construction และจำกัด resolution ต่อเกม |
| DressUp | card นี้ถือว่ามีชื่อเพิ่มอีกชื่อตลอดเกม |
| UnisonDress | ใส่ specific units เข้าสถานะ UnisonDress |
| Overcharge | อ้างอิง unit ที่ถูกเลือกด้วย ability ของ vanguard ในเทิร์นนั้น |
| Malwyrm | ใส่ specific unit เข้าสถานะ Malwyrm |
| Ace Unit | จำกัด deck construction ใส่ Ace Unit ได้ทั้ง deck สูงสุด 1 ใบ |
| Rewrite | activated ability ที่ ride จากมือโดยตรง |
| Mental Pollution | ใส่ specific unit เข้าสถานะ Mental Pollution |
| Happy Toys | continuous ability ไม่มีผลกติกาเฉพาะ แต่ถูกอ้างอิงโดยการ์ดอื่นได้ |
| SweetLink | ใส่ card เข้าสถานะ SweetLink เมื่อ soul มีชื่อเดียวกันตามเงื่อนไข |
| Arma Arms | keyword เขียนแบบ `(ability type icon) Arma Arms:(ability)` |
| Fantôme Skill | keyword เขียนแบบ `Fantôme Skill - (ability)` |

## 4. Specific actions / resource words ที่เอฟเฟกต์เรียกใช้

| Action | ความหมาย |
|---|---|
| Call | วาง unit ลง RC/GC |
| Ride | วาง unit ลง VC; มี normal ride/superior ride/persona ride |
| Legion / D-Legion | รวม vanguard กับ mate ตามเงื่อนไข |
| Stride | วาง G unit บน VC; vanguard เดิมกลายเป็น heart |
| overDress / XoverDress | วางทับ unit/originalDress โดยไม่ถือว่า call |
| Stand / Rest | ตั้ง/นอน unit |
| Turn face up/down | พลิกการ์ด เช่น damage/G zone |
| Draw | จั่วจาก deck |
| Search | ค้น deck แล้วมัก shuffle |
| Discard | ทิ้งจากมือไป drop |
| Reveal | เปิดเผยการ์ด |
| Retire | เอา rear-guard ไป drop |
| Remove | remove from game |
| Heal | เอาการ์ดจาก damage zone ไป drop |
| Drive Check | เปิดใบบน deck เพื่อ trigger แล้วเข้ามือ |
| Damage Check | เปิดใบบน deck เพื่อ trigger แล้วเข้าดาเมจ |
| Counter-Blast | พลิก damage หงายเป็นคว่ำ |
| Counter-Charge | พลิก damage คว่ำเป็นหงาย |
| Soul-Blast | เอาการ์ดจาก soul ไป drop |
| Soul-Charge | เอาใบบนสุด deck เข้า soul |
| Bind | ย้ายการ์ดไป bind zone |
| Lock / Unlock | สถานะ Link Joker ที่ปิดการใช้งาน circle/card แล้วปลดล็อกทีหลัง |
| Delete | ทำให้ vanguard ถูก delete ตามกติกาเฉพาะ |
| Time Leap | bind unit แล้ว call unit grade +1 ชั่วคราว |
| Gauge / Plant Gauge | ใส่/ย้ายการ์ดเป็น gauge ใต้ unit |
| Final Rush / Final Burst | สถานะของผู้เล่นที่ถูกการ์ดอื่นอ้างอิง |
| World | สถานะโลก เช่น Dark Night, Abyssal Dark Night, Stray Garden |
| Alchemagic | เล่น order พร้อม copy/รวม effect กับ order ใน drop ตามกติกา |
| Energy-Charge | เพิ่ม energy สูงสุดตาม limit |
| Energy-Blast | จ่าย energy |
| Choose / Cannot be chosen | เลือกเป้าหมาย หรือป้องกันการถูกเลือก |

## 5. Set-specific / Title-specific mechanics

| กลุ่ม | คำที่ควรแยก |
|---|---|
| Gyze | Gyze card, Zeroth Dragon, Ultimate Stride |
| Astral Poet | Astral Plane circles, Astral Deity |
| Music | Music Orders |
| Brandt Gate prison | Imprison |
| Lyrical | Songs, Singing |
| Arms | Arms Orders, Armed |
| SHAMAN KING | Oversoul, Völundr |
| Mushiking | Scout |
| Touken Ranbu | Kiwame-related title rules |
| Buddyfight | Buddy, Buddy Soul, Buddyfight arms |

## 6. Break Ride อยู่ตรงไหน

`Break Ride` มีจริงในยุค Limit Break แต่ใน Comprehensive Rules ver. 4.55 ไม่ได้ถูกแยกเป็นหัวข้อ keyword ability ชื่อ Break Ride แล้ว

รูปแบบที่ควรจัดคือ:

```text
Break Ride pattern =
[AUTO](VC) Limit Break 4:
When a unit/grade 3/specified clan rides this unit,
give your vanguard Power +10000 and an extra effect until end of turn.
```

ดังนั้นเวลาใส่ฐานข้อมูล ควรเก็บแบบนี้:

| ช่อง | ค่า |
|---|---|
| mechanic_group | Break Ride |
| formal_keyword | Limit Break |
| trigger | when a specified unit rides this unit |
| zone | VC |
| condition | damage 4 or more |
| effect | vanguard +10000 and additional text |
| era | OG / Link Joker / Legion Mate period |

คำที่คล้ายกันและควรเก็บเป็น pattern ไม่ใช่ keyword หลัก:

| Pattern | วิธีจำ |
|---|---|
| Break Ride | LB4 + เมื่อ ride ทับ unit นี้ |
| Cross Ride | vanguard ได้ power เพิ่มถ้า soul มีชื่อร่างก่อนหน้า |
| Ultimate Break | Limit Break 5 |
| Persona Blast | ทิ้ง/ใช้การ์ดชื่อเดียวกันเป็น cost |
| Mega Blast | cost ใหญ่แบบ CB5 + SB8 ของยุคแรก |
| Persona Ride | กติกายุค D/DZ: ride ชื่อเดียวกันที่มี icon |
| Divine Skill | restriction icon ใช้ได้ครั้งใหญ่ต่อเกมตามข้อจำกัด |

## 7. Trigger/timing conditions ที่เจอบ่อย

| Timing phrase | หมายถึง |
|---|---|
| When placed | เมื่อถูกวางลงโซนที่ระบุ |
| When rode upon | เมื่อถูก ride ทับ |
| When this unit attacks | ตอนประกาศโจมตี |
| When this unit boosts | ตอน boost |
| When boosted | เมื่อได้รับ boost |
| When attack hits | เมื่อการโจมตี hit |
| When attack does not hit | เมื่อการโจมตีไม่ hit |
| At the end of that battle | หลัง battle นั้นจบ |
| At the beginning/end of turn | ต้น/ท้ายเทิร์น |
| During your turn/opponent's turn | active เฉพาะเทิร์นที่ระบุ |
| If you persona rode this turn | เช็กว่าเทิร์นนั้น persona ride แล้ว |
| If you played an order this turn | เช็กการเล่น order |
| If a card was put into soul/drop/bind | เช็กเหตุการณ์การ์ดย้ายโซน |
| If your vanguard is grade 3 or greater | เช็ก grade vanguard |
| If your opponent's vanguard is grade 3 or greater | เช็ก grade ฝ่ายตรงข้าม |
| If you have X or more rear-guards | เช็กจำนวน unit |
| If your soul/drop/bind has X | เช็ก resource/zone |

## 8. แนะนำโครงสร้างจัดฐานข้อมูล

```text
effect_id
card_no
card_name
ability_type       AUTO / ACT / CONT
zone               VC / RC / GC / hand / drop / soul / bind / order zone
formal_keyword     Limit Break / GB / Brave / overDress / etc.
mechanic_group     Break Ride / Cross Ride / Persona Blast / etc.
timing             when attacks / when placed / at end of battle
condition_text
cost_text
effect_text
resource_used      CB / SB / EB / discard / retire / bind
duration
era                OG / G / V / D / DZ
notes
```

## Sources

- Official Comprehensive Rules ver. 4.55: https://en.cf-vanguard.com/wordpress/wp-content/uploads/2026/05/22173651/Cardfight-Vanguard-Comprehensive-Rules-4.55.pdf
- Official Card List: https://en.cf-vanguard.com/cardlist/
- Official Rules/Q&A: https://en.cf-vanguard.com/howto/
