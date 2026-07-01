# KK Card Fight Vanguard TH Export

ตัวนี้ดึงข้อมูลการ์ดจาก public API ที่แอป KK Card Fight ใช้เอง แล้วจัดเป็นไฟล์สำหรับทำบอทหรือฐานข้อมูลต่อ

## ใช้งาน

```powershell
python .\outputs\kk_cardfight_export\kk_vanguard_export.py
```

ไฟล์หลักที่ได้:

- `data\vanguard_th_cards.json` ข้อมูลการ์ด 1 ใบต่อ object
- `data\vanguard_th_cards.csv` เปิดใน Excel ได้
- `data\vanguard_th_by_series_clan.json` จัดกลุ่มตามซีรี่ส์และแคลน
- `data\series_summary.csv` จำนวนการ์ดตามซีรี่ส์
- `data\clan_summary.csv` จำนวนการ์ดตามแคลน
- `data\series_clan_summary.csv` จำนวนการ์ดแบบซีรี่ส์ + แคลน
- `data\meta.json` แหล่งที่มาและ version ของข้อมูล

## ดาวน์โหลดรูปการ์ดทุกใบ

หลังจาก export ข้อมูลแล้ว ใช้คำสั่งนี้เพื่อโหลดรูปทั้งหมด:

```powershell
python .\outputs\kk_cardfight_export\kk_download_images.py
```

ค่าเริ่มต้นจะโหลดรูปไปที่:

- `data\images\<series_code>\<clan>\<card_id>.jpg`
- `data\image_manifest.csv`
- `data\vanguard_th_cards_with_images.json`

ถ้ารันซ้ำ script จะข้ามไฟล์ที่มีอยู่แล้ว จึงใช้สำหรับ resume ได้

สำหรับทำระบบใหม่ แนะนำใช้ `data\vanguard_th_cards_with_images.json` เป็นไฟล์หลัก เพราะมีทั้งข้อมูลการ์ด, URL ต้นทาง, และ `local_image_path` ของรูปที่โหลดมาแล้ว

## แหล่งข้อมูลที่พบจากแอป

- Base API: `https://user-api.kkcardfight.com/api`
- Vanguard TH game id: `62a192ac1991c4ed95b6a12e`
- Card pointer: `https://user-api.kkcardfight.com/api/player/card/list?gameId=62a192ac1991c4ed95b6a12e`

ข้อมูลเป็น UTF-8 ต้องอ่านด้วย parser ที่รองรับ UTF-8 เช่น Python/Node ไม่ควรเปิดด้วย PowerShell `Get-Content` แบบ default เพราะภาษาไทยจะเพี้ยน
