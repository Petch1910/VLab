# Vanguard Development Package

## Source References

เอกสารชุดนี้อ้างอิงจากแหล่งข้อมูลกลางใน `source_references.md`

Primary source IDs:

- `SRC-RULES-455` - Comprehensive Rules 4.55, source หลักสำหรับกติกาปัจจุบัน
- `SRC-RULES-119` ถึง `SRC-RULES-455` - ใช้ดูวิวัฒนาการกฎตั้งแต่ยุคแรกถึง DZ
- `SRC-ANIME-OFFICIAL` - ใช้ยืนยันรายชื่อภาคอนิเมะ
- `SRC-CARDLIST` / `SRC-CARDSEARCH` - ใช้ตรวจ card text รายใบ

ชุดนี้เป็นเอกสารสำหรับนำกติกา Cardfight!! Vanguard ไปออกแบบระบบ/ฐานข้อมูล/rule engine ต่อ โดยอ้างอิงจาก Markdown corpus ใน `outputs/vanguard_rules_markdown/`

## ไฟล์ในชุดนี้

- `rule_engine_spec.md` - สเปกระบบละเอียด: object model, game loop, check timing, battle, cost/effect, format support
- `rule_taxonomy.json` - taxonomy แบบ machine-readable สำหรับ zones, phases, actions, keywords, triggers, rule actions
- `implementation_checklist.md` - checklist ว่าควร implement อะไรใน engine, อะไรเป็น card script, อะไรเป็น format rule

## หลักการใช้

1. ใช้ `rule_taxonomy.json` เป็น seed สำหรับ schema / enum / config
2. ใช้ `rule_engine_spec.md` เป็น blueprint การออกแบบ engine
3. ใช้ `implementation_checklist.md` เป็น backlog
4. เมื่อต้องตัดสินรายละเอียดของ rule ให้เปิด `outputs/vanguard_rules_markdown/versions/13_dz_rules_4_55.md` และ PDF ต้นฉบับประกอบ

## ระดับความครบ

ชุดนี้ครอบคลุมทุกหมวดหลักใน Comprehensive Rules 4.55:

- Card information
- Zones
- Information and status
- Unit placement
- Specific actions
- Setup
- Turn progression
- Battle
- Ability/effect resolution
- Processing terms
- Rule actions
- Keywords
- Markers/pseudo-cards
- Imaginary Gifts
- Set-specific rules
- Miscellaneous

ข้อจำกัด: icon glyph บางตัวใน PDF สกัดเป็นช่องว่างใน Markdown corpus ดังนั้นเวลาพัฒนา UI หรือ parser ที่ต้องรู้ icon จริง ควรเปิด PDF ต้นฉบับเทียบด้วย
