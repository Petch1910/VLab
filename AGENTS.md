# AGENTS.md

ไฟล์นี้เป็นกฎหลักสำหรับ AI ทุกโมเดลที่เข้ามาช่วยพัฒนาโปรเจกต์นี้ ต้องอ่านก่อนเริ่มงานทุกครั้ง

## Project Goal

สร้างโปรแกรม Vanguard ภาษาไทยที่เล่นได้บนคอมและมือถือ มี card database, deck builder, manual play table, replay, bot CPU fight และรองรับ online/room fight ภายหลัง

## Current Source Of Truth

- Card data: `outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json`
- Card images: `outputs/kk_cardfight_export/data/images/`
- Verification: `outputs/kk_cardfight_export/data/verification_report.json`
- VangPro study: `outputs/vangpro_study/VANGPRO_STUDY_TH.md`
- Official digital games study: `outputs/vanguard_official_games_study/VANGUARD_DIGITAL_GAMES_STUDY_TH.md`

## Required Read Order

1. `docs/AI_QUICK_START.md`
2. เอกสาร spec ของระบบที่กำลังแตะ เช่น `DECK_SYSTEM_SPEC.md` หรือ `GAME_ENGINE_SPEC.md`
3. ถ้างานแตะ core rules, formats, timing windows, ability resolution, fixtures หรือ bot simulation boundaries ให้อ่าน `docs/CORE_DEVELOPMENT_GUARDRAILS.md` และ `docs/VANGUARD_CORE_RULE_ARCHITECTURE_REFERENCE.md`
4. ถ้าต้องการบริบทเต็ม ให้อ่าน `docs/AI_CONTEXT_BRIEF.md`, `docs/ROADMAP.md`, `docs/IMPLEMENTATION_PLAN.md`

## Engineering Direction

- ใช้ Unity + C# สำหรับโปรแกรมหลัก
- ใช้ SQLite เป็น local database
- ใช้ Python สำหรับ data import/build tools
- เก็บรูปเป็นไฟล์ ไม่เก็บ binary image ใน SQLite
- ทุกระบบใหญ่ต้องมี spec ก่อน implement
- ทำเป็น vertical slice เล็ก ๆ ที่ verify ได้

## AI Work Rules

- อย่าขยาย scope เอง
- อย่า refactor ใหญ่ถ้า task ไม่ขอ
- อย่าเปลี่ยน stack โดยไม่มี ADR
- อย่าลบหรือย้าย data ที่ดึงมาแล้ว
- ถ้าแก้ logic ต้องมี test หรือ verification
- ถ้าเพิ่ม feature ต้องอัปเดต docs ที่เกี่ยวข้อง
- ถ้าส่งงานต่อ ต้องใช้ `docs/AI_TASK_HANDOFF_TEMPLATE.md`

## Quality Bar

งานถือว่าเสร็จเมื่อ:

- ตรง spec
- build/test/verification ผ่าน
- ไม่มี missing data หรือ broken path
- docs อัปเดต
- สรุปสิ่งที่ทำและข้อจำกัดชัดเจน

ดูรายละเอียดใน `docs/DEFINITION_OF_DONE.md`
