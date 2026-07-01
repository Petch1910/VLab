# สรุประบบ Cardfight!! Area Full Version 4.16 สำหรับทำบอท

## โครงสร้างที่พบ

โฟลเดอร์เกมหลักคือ:

```text
D:\Cardfight!! Area Full Version 4.16
```

ไฟล์สำคัญ:

- `Vanguard.exe` เป็นตัวเกมหลักแบบ binary ไม่มี source code
- `Setting.ini` เก็บค่า nickname, server ip, deck default, sleeve, resolution
- `Text/*.txt` เป็นฐานข้อมูลไพ่ รูปแบบคล้าย GameMaker script
- `Decks/*.prfl` เป็นไฟล์เด็ค
- `Manual/index.html` มีคู่มือ control ในเกม
- `CardSprite/` และ `CardSpriteMini2/` เป็นรูปไพ่

ฐานข้อมูลไพ่ที่ parse ได้ตอนนี้ประมาณ 16,470 ใบจากไฟล์ text

## Format ฐานข้อมูลไพ่

ตัวอย่าง:

```text
CardStat = 8230
{
global.CardName[CardStat] = 'Chakrabarthi Divine Dragon, Nirvana'
global.CardText[CardStat] = 'Dragon Empire/Flame Dragon
...
global.UnitGrade[CardStat] = 3
}
global.PowerStat[CardStat] = 13000
global.DefensePowerStat[CardStat] = 0
```

ข้อมูลที่นำมาใช้กับบอทได้ทันที:

- card id
- card name
- card text
- grade
- power
- shield
- trigger จากข้อความ `Critical Trigger`, `Draw Trigger`, `Heal Trigger`, `Front Trigger`, `Over Trigger`

## Format deck `.prfl`

ไฟล์ `.prfl` เป็น ASCII hex ไม่ใช่ไฟล์ encrypted

โครงสร้างที่ถอดได้:

- 32 hex chars แรกเป็น header
- หลังจากนั้นแบ่งทีละ 16 hex chars ต่อ 1 slot
- ค่า card id เป็น double ที่สลับ word order

ตัวอย่าง:

```text
0013C04000000000 -> 8230 -> Chakrabarthi Divine Dragon, Nirvana
```

วิธี decode ใน Python:

```python
raw = bytes.fromhex(hex16)
card_id = int(round(struct.unpack("<d", raw[4:8] + raw[0:4])[0]))
```

เด็คตัวอย่าง `[D] DSD01 - Dragon Empire (Nirvana).prfl` decode ได้ 50 ใบครบ

## สิ่งที่ทำใน prototype แล้ว

อยู่ที่:

```text
outputs\cfa_bot\cfa_bot.py
```

คำสั่งที่มี:

- `scan` อ่านฐานข้อมูลไพ่
- `deck` ถอดและสรุปเด็ค
- `suggest` สุ่มมือแรกและแนะนำ mulligan/plan
- `play` จำลองบอทเล่นเองแบบ console

## ข้อจำกัดตอนนี้

- ยังไม่ได้ควบคุม `Vanguard.exe` โดยตรง
- ยังไม่ได้อ่าน state จากหน้าจอจริง
- ยังไม่ได้ reverse network protocol ของ server
- skill ของไพ่ยังไม่ได้ execute ตาม text แบบเต็ม เพราะ Cardfight Area เองก็พึ่งผู้เล่นกด/ตัดสินใจหลายอย่าง

## ทางต่อให้กลายเป็นบอทเล่นในเกมจริง

แนะนำทำเป็น 3 ชั้น:

1. Bot brain
   - ใช้ parser ที่ทำแล้ว
   - เพิ่ม state model: hand, deck, drop, damage, soul, VC, RC, opponent field
   - เพิ่ม strategy ต่อ deck เช่น Nirvana, Messiah, Chronojet

2. Screen/controller layer
   - จับตำแหน่งหน้าต่าง `Vanguard.exe`
   - map พิกัด field/deck/hand/buttons ตาม resolution
   - คลิก/drag ตาม action ที่ bot brain เลือก

3. Recognition layer
   - อ่านชื่อ/รูปไพ่จาก screen capture หรือจาก known deck state
   - detect phase และ prompt ที่เกมเปิดขึ้น
   - ถ้าอ่านไม่ได้ ให้ถามผู้เล่นผ่าน console

ทางนี้เร็วกว่า reverse protocol เพราะตัวเกมไม่มีเอกสาร protocol ในไฟล์ที่พบ

