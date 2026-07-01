# Vanguard Development Index

เริ่มอ่านที่นี่ถ้าจะนำข้อมูลไปพัฒนาระบบต่อ

## Core Files

| File | ใช้ทำอะไร |
|---|---|
| `README_TH.md` | ภาพรวม package |
| `rule_engine_spec.md` | blueprint ของ rule engine และ data model |
| `rule_taxonomy.json` | taxonomy machine-readable สำหรับทำ enum/config/schema |
| `implementation_checklist.md` | backlog/checklist สำหรับลงมือ implement |
| `field_layout_by_era.md` | รายละเอียดสนาม/โซน/เลย์เอาต์ตามยุค |
| `field_blueprints_by_anime_part.md` | field blueprint แยกตาม 23 ภาคอนิเมะ พร้อม ASCII layout และคำแนะนำ UI/engine |
| `field_blueprints.json` | machine-readable config สำหรับ template สนามและ mapping 23 ภาค |
| `source_references.md` | registry กลางของ source ID, official PDF/เว็บ, และ mapping ว่าเอกสารไหนใช้อ้างอิงอะไร |

## Upstream Corpus

| File | ใช้ทำอะไร |
|---|---|
| `../vanguard_rules_markdown/00_index.md` | index ของ Comprehensive Rules ทุกเวอร์ชัน |
| `../vanguard_rules_markdown/versions/13_dz_rules_4_55.md` | กติกาล่าสุดที่ใช้เป็นฐานหลัก |
| `../vanguard_rules_markdown/mechanic_presence_matrix.md` | ดูว่า mechanic เริ่มปรากฏในเวอร์ชันใด |
| `../vanguard_rules_markdown/all_rules_combined.md` | ค้นข้อความรวมทุกเวอร์ชัน |

## Recommended Development Flow

1. อ่าน `rule_engine_spec.md` เพื่อเข้าใจ architecture
2. โหลด `rule_taxonomy.json` ไปสร้าง enum/config เบื้องต้น
3. ใช้ `implementation_checklist.md` วาง milestone
4. ทำ prototype ด้วย OG core ก่อน
5. เพิ่ม modules ตามลำดับ: Legion -> Stride -> V -> D/DZ
6. ใช้ Markdown corpus และ PDF ต้นฉบับ verify edge cases

## Suggested Milestones

### Milestone 1: Playable Core

- Setup
- Ride
- Call
- Battle
- Guard
- Drive/damage checks
- Basic triggers
- Counter-Blast/Soul-Blast

### Milestone 2: Ability Engine

- ACT/AUTO/CONT
- Check timing
- Continuous effects
- Replacement effects
- Rule actions

### Milestone 3: Era Modules

- Limit Break/Lock
- Legion
- Stride/G Guardian
- Imaginary Gift
- Ride Deck/Persona Ride
- overDress/Orders
- Energy/Divine Skill

### Milestone 4: Full Card Pool Support

- Effect parser/DSL
- Card scripts
- Title-specific mechanics
- Format legality
- Ban/restriction updates
- Test suite per mechanic
