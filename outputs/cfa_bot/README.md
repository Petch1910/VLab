# Cardfight Area Bot Prototype

เครื่องมือนี้เป็นบอทต้นแบบแบบ external assistant สำหรับ `Cardfight!! Area Full Version 4.16`
โดยยังไม่แก้ `Vanguard.exe` และยังไม่คลิกแทนผู้เล่นในเกมจริง

สิ่งที่ทำได้ตอนนี้:

- อ่านฐานข้อมูลไพ่จาก `Text/*.txt`
- ถอดไฟล์เด็ค `.prfl`
- สรุปเด็คตาม grade / trigger / power
- สุ่ม opening hand และให้คำแนะนำ mulligan แบบ rule-based
- ให้คำแนะนำ play plan พื้นฐานตามไพ่ในมือ

## ใช้งาน

เปิด PowerShell แล้วรันจากโฟลเดอร์นี้:

```powershell
python .\cfa_bot.py scan --game "D:\Cardfight!! Area Full Version 4.16"
```

ดูเด็ค:

```powershell
python .\cfa_bot.py deck --game "D:\Cardfight!! Area Full Version 4.16" --deck "D:\Cardfight!! Area Full Version 4.16\Decks\[D] DSD01 - Dragon Empire (Nirvana).prfl"
```

ให้บอทสุ่มมือแรกและแนะนำ:

```powershell
python .\cfa_bot.py suggest --game "D:\Cardfight!! Area Full Version 4.16" --deck "D:\Cardfight!! Area Full Version 4.16\Decks\[D] DSD01 - Dragon Empire (Nirvana).prfl" --seed 1
```

จำลองบอทเล่นเอง 4 เทิร์น:

```powershell
python .\cfa_bot.py play --game "D:\Cardfight!! Area Full Version 4.16" --deck "D:\Cardfight!! Area Full Version 4.16\Decks\[D] DSD01 - Dragon Empire (Nirvana).prfl" --seed 1 --turns 4
```

## แนวทางต่อไป

ถ้าจะให้บอทเล่นในตัวเกมจริง มี 2 ทาง:

1. UI automation: ให้โปรแกรมอ่านหน้าจอ/ตำแหน่งไพ่ แล้วคลิกตาม action ที่ bot brain เลือก
2. Network/protocol bot: reverse protocol ของ server แล้วให้บอท join เป็น client เอง

จากไฟล์ที่ตรวจพบตอนนี้ ทางที่เร็วและเสี่ยงน้อยกว่าคือ UI automation เพราะตัวเกมเป็น `Vanguard.exe`
แบบไบนารี และ asset/data เปิดให้อ่านได้ แต่ยังไม่เห็น protocol เป็นเอกสารชัดเจน
