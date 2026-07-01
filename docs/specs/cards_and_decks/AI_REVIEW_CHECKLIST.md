# AI Review Checklist

## Scope

- งานตรงกับ request หรือไม่
- มี feature นอก scope หรือไม่
- มี refactor ที่ไม่จำเป็นหรือไม่

## Correctness

- data structure ถูกตาม spec หรือไม่
- edge case สำคัญถูกจัดการหรือไม่
- error handling ชัดเจนหรือไม่

## Game Rules

- deck validation ตรง format หรือไม่
- game zones ถูกต้องหรือไม่
- action log replay ได้หรือไม่
- bot ไม่ใช้ข้อมูลที่ผู้เล่นไม่ควรรู้หรือไม่

## Data

- card id ไม่ซ้ำ
- local image path มีจริง
- version/hash ถูกเก็บ
- ไม่ hardcode absolute path ที่ไม่ควรมี

## Unity

- logic ไม่ผูกกับ UI เกินจำเป็น
- scene/component ไม่เก็บ state ที่ควรอยู่ใน model
- mobile input และ desktop input ถูกคิดไว้

## Tests

- มี test หรือ verification ที่เหมาะกับ risk หรือไม่
- test อ่านง่ายหรือไม่
- มี regression test สำหรับ bug fix หรือไม่

## Docs

- spec อัปเดตหรือไม่
- ADR จำเป็นหรือไม่
- changelog จำเป็นหรือไม่

