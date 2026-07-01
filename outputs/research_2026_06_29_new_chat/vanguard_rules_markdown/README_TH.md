# วิธีใช้ชุด Markdown กติกา Cardfight!! Vanguard

## แหล่งอ้างอิง

- Source registry กลาง: `../vanguard_development/source_references.md`
- รายชื่อ PDF ทางการและไฟล์ Markdown แต่ละเวอร์ชัน: `00_index.md`
- Metadata สำหรับตรวจสอบ source URL, SHA-256, version, page count: `manifest.json`

ทุกไฟล์ใน `versions/` มี source PDF URL และ SHA-256 อยู่ใน metadata ตอนต้นไฟล์ เพื่อให้ตรวจกลับกับ PDF ทางการได้

ชุดนี้แปลงจาก official Comprehensive Rules PDF แบบ page-by-page โดยไม่ย่อเนื้อหาในส่วน full text เพื่อให้ค้นหาและศึกษาได้ง่ายกว่า PDF เดิม

## ไฟล์หลัก

- `00_index.md` - จุดเริ่มต้น มีลำดับอ่านตามยุคและลิงก์ไปทุกไฟล์
- `versions/` - ไฟล์ Markdown แยกตามเวอร์ชันกติกา
- `mechanic_presence_matrix.md` - ตารางดูว่า mechanic ไหนเริ่มปรากฏในเวอร์ชันใด
- `section_heading_index.md` - ดัชนีหัวข้อที่สกัดจากทุกเวอร์ชัน
- `all_rules_combined.md` - รวมทุกเวอร์ชันไว้ไฟล์เดียวสำหรับค้นคำแบบรวม
- `manifest.json` - metadata สำหรับเครื่องอ่าน เช่น version, date, pages, character count, sha256

## ลำดับอ่านที่แนะนำ

1. อ่าน `01_og_link_joker_rules_1_19.md` เพื่อจับ core rules
2. อ่าน `02_legion_mate_rules_1_31.md` สำหรับ Legion / Seek Mate
3. อ่าน `03_g_stride_rules_1_38.md` สำหรับ Stride / Heart / Generation Break
4. อ่าน `04_g_guardian_keywords_rules_1_44_1.md` และ `05_g_next_rules_1_46_4.md` สำหรับ keyword ยุค G
5. อ่าน `06_gz_ultimate_stride_rules_1_48.md` สำหรับ Ultimate Stride / Gyze
6. อ่าน `07_v_reboot_rules_2_01.md` ถึง `10_v_reboot_rules_2_3.md` สำหรับ V Reboot
7. อ่าน `11_dz_rules_4_45.md` ถึง `13_dz_rules_4_55.md` สำหรับ D/DZ ปัจจุบัน

## หมายเหตุเรื่องความครบถ้วน

ไฟล์ Markdown เก็บข้อความที่สกัดได้จาก PDF ทุกหน้า ไม่ใช่สรุปย่อ แต่ PDF บางหน้าใช้ icon glyph หรือภาพประกอบที่ตัวแปลงข้อความอาจดึงออกมาเป็นช่องว่างได้ ดังนั้นในแต่ละไฟล์จึงเก็บ source PDF URL และ SHA-256 ไว้ให้ตรวจย้อนกับต้นฉบับได้

ถ้าต้องวิเคราะห์ rule เชิงละเอียด ให้ใช้ Markdown สำหรับค้นหาและเทียบเวอร์ชัน แล้วเปิด PDF ต้นฉบับประกอบเมื่อเจอจุดที่มี icon หรือ layout สำคัญ
