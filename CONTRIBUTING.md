# Contributing

แนวทางนี้ใช้กับทั้งคนและ AI ที่เข้ามาช่วยพัฒนา

## Before Starting

1. อ่าน `AGENTS.md`
2. อ่าน `docs/AI_CONTEXT_BRIEF.md`
3. อ่าน spec ที่เกี่ยวกับงาน
4. เช็ก `docs/ROADMAP.md` ว่างานอยู่ phase ไหน

## Development Flow

1. ระบุ scope ให้เล็ก
2. วาง acceptance criteria
3. implement ทีละ slice
4. verify ด้วย test/build/data check
5. อัปเดต docs
6. สรุปผลและข้อจำกัด

## Commit Guidance

- Commit เล็กและมีความหมาย
- ไม่ commit ไฟล์ build ขนาดใหญ่
- ไม่ commit card images ทั้งหมดเข้า repo หลัก
- ใช้ Git LFS หรือ external storage สำหรับไฟล์ใหญ่ถ้าจำเป็น

## Review Checklist

ก่อน merge หรือส่งต่อ:

- ตรง spec หรือไม่
- มี regression risk หรือไม่
- มี test/verification หรือไม่
- มี hardcoded path ที่ไม่ควรมีหรือไม่
- docs อัปเดตหรือไม่

