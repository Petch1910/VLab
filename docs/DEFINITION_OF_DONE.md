# Definition Of Done

งานถือว่าเสร็จเมื่อครบเงื่อนไขนี้

## General

- ตรง acceptance criteria
- ไม่มีงานครึ่ง ๆ กลาง ๆ ที่จำเป็นต่อ feature
- ไม่มี error ที่รู้แล้วปล่อยไว้โดยไม่บันทึก
- สรุปผลและข้อจำกัดชัดเจน

## Code

- build ผ่าน
- tests/verification ผ่าน
- ไม่มี dead code ที่ไม่จำเป็น
- ไม่มี hardcoded local path ยกเว้น script เฉพาะ workspace

## Data

- จำนวน records ตรง expected
- paths ที่อ้างถึงมีจริง
- ไม่มีไฟล์ชั่วคราวค้าง เช่น `.part`
- version/hash ถูกอัปเดตเมื่อ data เปลี่ยน

## Docs

- spec ที่เกี่ยวข้องอัปเดต
- ADR เพิ่มเมื่อมี decision ใหญ่
- handoff note มีถ้างานยังต้องต่อ

## Unity Future

- scene เปิดได้
- Windows build หรือ Android build ผ่านตาม target phase
- UI ไม่พังใน desktop/mobile aspect ratio หลัก

