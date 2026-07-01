# M40-01 Second-Slice Review Packet

## Summary

- Target slice: `Classic Core` / `โอราเคิล ทิงค์ แทงค์`
- Fixture notes: `6`
- Manual-review cards: `7`
- Candidate edges: `259`
- Total review items: `272`
- Ready for M40-02: `True`

## Evidence

- M39-04 offline recipe pipeline allowed: `True`
- Runtime/saved deck/bot promotion closed: `True`
- Fixture expectations met: `True`
- Probe cards: `103`
- Probe edges: `2660`
- Expected candidate edges: `259`

## Fixture Notes

- `classic_core_second_slice_valid_minimal` expected=`pass` met=`True` reasons=`none`
- `classic_core_second_slice_short_main` expected=`fail` met=`True` reasons=`main_count:49!=50`
- `classic_core_second_slice_bad_trigger_count` expected=`fail` met=`True` reasons=`trigger_count:15!=16`
- `classic_core_second_slice_missing_grade_3_setup` expected=`fail` met=`True` reasons=`missing_setup_grade:3`
- `classic_core_second_slice_copy_limit_exceeded` expected=`fail` met=`True` reasons=`copy_limit_exceeded:BT03-038TH:5>4`
- `classic_core_second_slice_identity_mismatch` expected=`fail` met=`True` reasons=`identity_mismatch:BT01-003TH`

## Manual-Review Cards

- `BT05-019TH` เจ้าหญิงนัยน์ตาปีศาจ,ยูริเอล unmapped=`bounce_to_hand`
- `BT07-095TH` มูนซอลท์ สวอลโล่ว์ unmapped=`bounce_to_hand`
- `BT07-096TH` แบทเทิล ซิสเตอร์,เอแคลร์ unmapped=`bounce_to_hand`
- `BT09-003TH` เทพธิดาแห่งสุริยัน,อามาเทราสึ unmapped=`bounce_to_hand`
- `BT09-068TH` วีรสตรีแห่งสุริยัน,อุสุเมะ unmapped=`bounce_to_hand`
- `EB05-004TH` เทพธิดาดอกไม้แห่งการทำนาย,ซาคุยะ unmapped=`bounce_to_hand`
- `TD04-002TH` เทพธิดาแห่งดอกไม้พยากรณ์ ซากุยะ unmapped=`bounce_to_hand`

## Candidate Edge Sample

- `1` `BT01-006TH->BT02-033TH` score=`13` CEO อามาเทราสึ -> ลัค เบิร์ด
- `2` `BT01-006TH->TD04-011TH` score=`13` CEO อามาเทราสึ -> ลัค เบิร์ด
- `3` `EB05-001TH->BT02-033TH` score=`13` CEO อามาเทราสึ -> ลัค เบิร์ด
- `4` `EB05-001TH->TD04-011TH` score=`13` CEO อามาเทราสึ -> ลัค เบิร์ด
- `5` `BT03-007TH->BT02-033TH` score=`12` เทพธิดาจันทร์ครึ่งดวง ซึคุโยมิ -> ลัค เบิร์ด
- `6` `BT03-007TH->BT02-066TH` score=`12` เทพธิดาจันทร์ครึ่งดวง ซึคุโยมิ -> หนึ่งเดียวผู้จ้องมองความจริง
- `7` `BT03-007TH->BT09-063TH` score=`12` เทพธิดาจันทร์ครึ่งดวง ซึคุโยมิ -> เจนเทิล จิม
- `8` `BT03-007TH->BT09-066TH` score=`12` เทพธิดาจันทร์ครึ่งดวง ซึคุโยมิ -> แบทเทิลซิสเตอร์,ครีม
- `9` `BT03-007TH->BT09-067TH` score=`12` เทพธิดาจันทร์ครึ่งดวง ซึคุโยมิ -> แมชชีนกันทอร์ค ไรอัน
- `10` `BT03-007TH->EB05-026TH` score=`12` เทพธิดาจันทร์ครึ่งดวง ซึคุโยมิ -> หนึ่งเดียวผู้จ้องมองความจริง
- `11` `BT03-007TH->EB05-027TH` score=`12` เทพธิดาจันทร์ครึ่งดวง ซึคุโยมิ -> ลัค เบิร์ด
- `12` `BT03-007TH->TD04-011TH` score=`12` เทพธิดาจันทร์ครึ่งดวง ซึคุโยมิ -> ลัค เบิร์ด
- `13` `EB05-003TH->BT09-066TH` score=`12` สกาเล็ตวิช,โคโค่ -> แบทเทิลซิสเตอร์,ครีม
- `14` `BT03-068TH->BT02-033TH` score=`10` เซเครตทารี่ แองเจิล -> ลัค เบิร์ด
- `15` `BT03-068TH->TD04-011TH` score=`10` เซเครตทารี่ แองเจิล -> ลัค เบิร์ด
- `16` `BT03-070TH->BT02-033TH` score=`10` เฟทฟูล แองเจิล -> ลัค เบิร์ด
- `17` `BT03-070TH->TD04-011TH` score=`10` เฟทฟูล แองเจิล -> ลัค เบิร์ด
- `18` `BT09-065TH->BT02-033TH` score=`10` ร็อควิช,กาก้า -> ลัค เบิร์ด
- `19` `BT09-065TH->TD04-011TH` score=`10` ร็อควิช,กาก้า -> ลัค เบิร์ด
- `20` `EB05-003TH->BT02-033TH` score=`10` สกาเล็ตวิช,โคโค่ -> ลัค เบิร์ด
- `21` `EB05-003TH->TD04-011TH` score=`10` สกาเล็ตวิช,โคโค่ -> ลัค เบิร์ด
- `22` `BT03-037TH->BT02-066TH` score=`9` โอราเคิล กาเดี้ยน บลูอาย -> หนึ่งเดียวผู้จ้องมองความจริง
- `23` `BT03-037TH->BT09-063TH` score=`9` โอราเคิล กาเดี้ยน บลูอาย -> เจนเทิล จิม
- `24` `BT03-037TH->BT09-067TH` score=`9` โอราเคิล กาเดี้ยน บลูอาย -> แมชชีนกันทอร์ค ไรอัน
- `25` `BT03-037TH->EB05-026TH` score=`9` โอราเคิล กาเดี้ยน บลูอาย -> หนึ่งเดียวผู้จ้องมองความจริง
- ... plus `234` more candidate edges in JSON/CSV.

## Policy

- Offline review packet only.
- No deck recipe draft is created in M40-01.
- No saved-deck injection or UI deck-list publication.
- No runtime deck promotion.
- No bot/playbook promotion.
- No live card text parsing.
- No direct `GameState` mutation.

## Next

`M40-02`: Second-slice recipe draft model.
