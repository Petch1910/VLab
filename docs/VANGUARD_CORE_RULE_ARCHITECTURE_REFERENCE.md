# Vanguard Core Development Guardrails

ไฟล์นี้ใช้เป็น checklist ระหว่างพัฒนา Vanguard Game Core เพื่อกันไม่ให้ลืมหลักสำคัญเรื่องกฎเกม, hidden information, RNG, probability, simulation และขอบเขตระหว่าง Core กับ AI Planning

## 1. หลักใหญ่ที่ห้ามลืม

- Core เป็นกรรมการ ไม่ใช่สมองบอท
- AI Planning คิดแผนได้ แต่ต้องสั่งผ่าน Legal Action API เท่านั้น
- Actual Game ต้องใช้ RNG จริงตามกฎ ห้ามใช้ค่าเฉลี่ย probability แทนผลสุ่ม
- Bot ต้องเห็นเฉพาะข้อมูลที่ผู้เล่นคนนั้นควรรู้
- ทุก action ต้องตรวจ legality ก่อน apply state
- ทุก random event ต้อง replay/debug ได้ด้วย seed หรือ event log
- ทุก effect ที่เกิดพร้อมกันต้องเข้าคิว pending resolution ไม่ resolve กระโดดไปมา

## 2. แบ่งชั้นระบบ

```text
RulesCore
├─ Legal Action API
├─ Strict Rule Enforcer
├─ Phase / Window Controller
├─ Trigger Resolver
├─ Continuous Effect / Power Tracker
├─ Hidden State Manager
├─ Actual RNG Resolver
├─ Pending Resolution Queue
├─ Snapshot / Rollback
└─ Event Log / Serialization

AI Planning Layer
├─ Exact Probability Engine
├─ Monte Carlo Simulator
├─ Battle Search
├─ Risk Evaluator
└─ Policy / Decision Model

Offline Dev Tools
├─ Generative Card Balancing
├─ Mass Simulation
├─ Balance Report
└─ Custom Format Testing
```

## 3. สิ่งที่ต้องอยู่ใน Core

### Legal Action API

- รับเฉพาะ command ที่เป็น structured data
- ห้ามให้ UI หรือ bot แก้ GameState โดยตรง
- ทุก command ต้อง validate ก่อน execute

ตัวอย่าง:

```text
Action(Type=CALL, CardId=102, From=HAND, To=RC_FRONT_LEFT)
Action(Type=ATTACK, Attacker=VG, Target=OPPONENT_VG)
Action(Type=GUARD, CardId=55, From=HAND, To=GC)
```

### Phase / Window Controller

Core ต้องคุม phase และ timing window เอง

```text
Stand Up
Stand & Draw
Ride Phase
  - Normal Ride
  - Persona Ride
  - G Assist
  - Stride Step / Stride Declaration
Main Phase
Battle Phase
End Phase
```

Stride ต้องเป็น timing/window ที่ Core รู้จักจริง ไม่ใช่แค่ flag เปิดปิด เพราะ Premium มีเงื่อนไขและ timing เฉพาะ เช่น grade requirement, stride cost, G zone, heart card, stride return ตอน end phase และ interaction กับ skill ที่ trigger ตอน stride

Battle Phase ต้องแตก step ชัดเจน:

```text
Attack Declaration
Guard Step
G-Guard Window
Drive Check
Damage Check
Battle Resolution
Close Step
```

G-Guard ต้องอยู่ใน guard timing ของ Battle Phase และต้อง validate แยกจาก normal guard เช่น เงื่อนไขการเรียกจาก G zone, discard/heal cost, generation zone state และ shield/effect ที่เกิดจาก G guardian

### Trigger Resolver

ต้อง resolve trigger จริงจาก RNG / deck order ของเกมจริงเท่านั้น

ต้องรองรับ:

- Draw trigger
- Critical trigger
- Stand trigger
- Front trigger
- Heal trigger
- Over trigger
- Power allocation
- Critical allocation
- Stand target selection
- Heal live condition

Trigger type ต้องขึ้นกับ RuleSet/Profile เช่น Premium อาจเปิด Stand trigger แต่ Standard อาจปิด Stand trigger และใช้ Front trigger/Over trigger ตามยุคแทน ห้าม hardcode ว่า trigger ทุกประเภทมีอยู่ในทุก format

### Continuous Effect / Power Tracker

Power ไม่ควร recompute ทั้งบอร์ดแบบสุ่มสี่สุ่มห้า แต่ควรแยกเป็น layer

```text
final power =
  base power
  + boost power
  + trigger buff
  + ability buff
  + marker buff
  + continuous modifier
```

ต้องมี cleanup เมื่อหมด timing:

- until end of battle
- until end of turn
- until unit leaves circle
- while condition is true

### Hidden State Manager

Core เป็น source of truth ของข้อมูลลับ

ต้องแยก view:

```text
FullGameState         ใช้ภายใน Core เท่านั้น
PlayerGameStateView   ใช้ส่งให้ผู้เล่นหรือ AI
PublicGameStateView   ใช้สำหรับ observer/replay ที่ไม่ควรเห็นข้อมูลลับ
```

AI ห้ามเห็น:

- มือคู่แข่งที่ยังไม่ reveal
- top deck ที่ยังไม่ reveal
- ลำดับ deck ที่ไม่รู้ตามกฎ
- private search choice ของคู่แข่ง

### Actual RNG Resolver

ต้องแยก RNG stream:

```text
LiveGameRng
ReplayRng
SimulationRng
TestFixtureRng
```

Live game ใช้ผลสุ่มจริง  
Simulation ใช้ seed แยก  
Replay ต้อง reproduce ผลเดิมได้

### Pending Resolution Queue

เมื่อมี auto ability หรือ trigger หลายตัวเกิดพร้อมกัน ต้องเข้าคิวก่อน

```text
Event occurs
-> collect matching abilities
-> add to pending pool
-> ask owner to choose order
-> resolve one by one
-> validate cost and target at resolution time
```

## 4. สิ่งที่ควรอยู่นอก Core

### Exact Probability Engine

ใช้คำนวณความน่าจะเป็นจากข้อมูลที่ผู้เล่น/AI รู้จริงเท่านั้น

เหมาะกับ:

- drive check 1-3 ใบ
- damage check
- trigger remaining
- heal chance
- over trigger chance
- lethal probability แบบง่าย
- expected shield pressure

ห้ามใช้ผล probability ไป apply กับเกมจริง

ผิด:

```text
trigger chance = 33%
เอา +3300 power ไปบวกทันที
```

ถูก:

```text
AI เห็นว่า trigger chance = 33%
AI ใช้ตัวเลขนี้ประเมินว่าจะบุกหรือถอย
Actual Game ยังต้อง reveal card จริงด้วย RNG
```

### Monte Carlo Simulator

ใช้เมื่อ branch ซับซ้อนจน exact formula ยุ่งเกินไป

เหมาะกับ:

- multi-attack หลายรอบ
- opponent guard policy เปลี่ยนตามผล trigger
- มี shuffle/search ระหว่าง battle
- มี skill ที่เปลี่ยน deck composition
- lethal line ที่มีหลาย decision point

Core ควรให้ API สำหรับ simulation แต่ไม่ควรเป็นคนรัน search เอง

API ที่ควรมี:

```text
CloneState()
CreateMaskedView(playerId)
GetLegalActions(state, playerId)
ApplyCommand(state, command)
ResolveVirtualCheck(state, rngSeed)
EvaluateTerminalState(state)
Rollback(snapshotId)
```

## 5. Rule Set / Format Profile

อย่าผูก Core กับ format เดียว

ควรมี flags เช่น:

```text
RuleSetProfile
├─ enableRideDeck
├─ enablePersonaRide
├─ enableOverTrigger
├─ enableStandTrigger
├─ enableFrontTrigger
├─ enableImaginaryGift
├─ enableStride
├─ enableGGuard
├─ enableEnergy
├─ enableQuickShield
├─ firstPlayerCanAttack
└─ maxCardsInDeck
```

เริ่มจาก profile หลัก:

- Standard
- V-Premium
- Premium
- Custom

## 6. Ability / Keyword Registry

Runtime ไม่ควร parse text สด

ให้ใช้ structured ability definition:

```text
SoulBlast(1)
CounterBlast(1)
CounterCharge(1)
Bind(target)
Lock(circle)
OverDress(condition)
DivineSkill(oncePerFight)
Draw(1)
ChooseUnit(count=1, zone=RC)
AddPower(target, amount=10000, until=EndOfTurn)
```

เป้าหมายคือให้ card text เป็นข้อมูลอ้างอิง แต่ runtime ใช้ compiled definition

## 7. Test Fixtures ที่ต้องมี

ทุก mechanic สำคัญต้องมี state เล็กๆ สำหรับ test

ตัวอย่าง test:

- ride ได้เฉพาะ Ride Phase
- stride ได้เฉพาะ Stride Step และเฉพาะ format ที่เปิด enableStride
- stride cost, G zone และ heart state ถูกต้องหลัง stride
- stride unit กลับ G zone ตอน end phase ถูกต้อง
- call ได้เฉพาะ circle ที่ถูกต้อง
- ผู้เล่นเริ่มก่อนโจมตีไม่ได้ ถ้า format ห้าม
- G-Guard เปิดได้เฉพาะ guard timing และเฉพาะ format ที่เปิด enableGGuard
- attack 23000 into 13000 แล้ว guard 10000 ต้อง pass
- drive check critical trigger แล้ว power/crit allocation ถูกต้อง
- heal trigger ทำงานเฉพาะเมื่อ damage condition ถูกต้อง
- over trigger ทำงานครั้งเดียวตาม format
- end turn cleanup ล้าง temporary buff ถูกต้อง
- bot view ไม่เห็นมือคู่แข่ง
- simulation clone ไม่กระทบ live state
- replay จาก event log ได้ผลเหมือนเดิม

## 8. Checklist ก่อน merge feature ใหม่

- Feature นี้อยู่ถูกชั้นหรือไม่: Core, AI Planning, หรือ Offline Tool
- มี legality validation ก่อนเปลี่ยน state หรือไม่
- มี event log สำหรับ action/effect/random result หรือไม่
- hidden information ถูก mask ใน AI/player view หรือไม่
- RNG ใช้ stream ถูกประเภทหรือไม่
- rollback/snapshot ไม่กระทบ live state หรือไม่
- effect ที่เกิดพร้อมกันเข้าคิว pending resolution หรือไม่
- power/critical cleanup ถูก timing หรือไม่
- format flags รองรับกฎต่างยุคหรือไม่
- มี test fixture ครอบคลุม edge case สำคัญหรือไม่

## 9. สิ่งที่ห้ามทำ

- ห้ามให้ bot อ่าน FullGameState
- ห้ามให้ UI แก้ GameState ตรงๆ
- ห้ามใช้ probability เป็นผลลัพธ์จริง
- ห้ามให้ Monte Carlo อยู่ใน Core runtime หลัก
- ห้าม parse card text สดระหว่าง match
- ห้าม hardcode กฎของ format เดียวลงในระบบกลาง
- ห้าม resolve auto ability โดยไม่ผ่าน pending queue
- ห้ามเขียน simulation ที่ mutate live state
- ห้ามทำ custom format ทุกอย่างตั้งแต่แรก

## 10. ลำดับพัฒนาที่แนะนำ

1. GameState model
2. Phase State Machine
3. Legal Action API
4. Event Log
5. Snapshot / Rollback
6. Seeded RNG
7. Hidden State View
8. Trigger Resolver
9. Power / Continuous Effect Tracker
10. Pending Resolution Queue
11. Ability Keyword Registry
12. RuleSet Profile
13. Virtual Simulation API
14. Exact Probability Engine
15. Monte Carlo Battle Search
16. Offline Balance Simulator

## 11. คำจำง่าย

```text
Core ตัดสินกฎ
RNG ตัดสินผลสุ่มจริง
AI ตัดสินใจจากความเสี่ยง
Probability เป็นแผนที่ ไม่ใช่อนาคต
Hidden state คือเส้นแบ่งระหว่างบอทฉลาดกับบอทโกง
```

## 12. Developer Core Principles

หลักคิดส่วนนี้ใช้ตอนเริ่มเปิดโค้ดจริง เพื่อให้ GameState, Snapshot, Hashing, Replay, UI และ AI Search ต่อกันง่ายตั้งแต่วันแรก

### Data-Driven Over Object-Oriented

Card, Unit, Zone และ Effect State ควรเป็น plain data หรือ struct ที่ copy, serialize และ hash ได้ง่าย

ไม่ควรให้การ์ดแต่ละใบเป็น object ที่มี logic ซ่อนอยู่ข้างใน เพราะจะทำให้ snapshot, rollback, replay, Zobrist hashing และ transposition table ยากขึ้นมาก

ควรคิดแบบนี้:

```text
CardInstance
├─ instanceId
├─ cardDefinitionId
├─ ownerPlayerId
├─ controllerPlayerId
├─ zone
├─ slot
├─ orientation
├─ faceState
└─ attachedStateFlags

Logic อยู่ใน:
ExpertRuleResolver
AbilityDefinitionRegistry
EffectResolutionSystem
```

ข้อดี:

- clone state ได้เร็ว
- rollback ง่าย
- serialize เป็น JSON/vector ได้ตรง
- Zobrist hash อัปเดตแบบ incremental ได้
- simulation ไม่ต้อง copy object graph ซับซ้อน

### Event-Driven Core Output

Core ไม่ควรสั่ง UI, print log หรือเขียน replay file โดยตรง

ทุกครั้งที่ action สำเร็จ ให้ Core emit domain event ออกมาแทน

ตัวอย่าง:

```text
[
  { event: "UNIT_CALLED", cardId: 10, from: "HAND", to: "RC_FL" },
  { event: "UNIT_RESTED", slot: "VG" },
  { event: "TRIGGER_REVEALED", triggerType: "CRITICAL", source: "DRIVE_CHECK" },
  { event: "POWER_CHANGED", slot: "RC_FL", delta: 10000, until: "END_OF_TURN" }
]
```

Event เหล่านี้ใช้ต่อได้หลายทาง:

- Event Log
- Replay Verifier
- Decision Trace
- UI animation
- Debug console
- Scenario regression
- Analytics / balance report

กฎสำคัญ:

- event ต้องเกิดจาก state transition ที่สำเร็จแล้วเท่านั้น
- event ต้องมีข้อมูลพอสำหรับ replay/debug
- event ห้าม leak hidden information ไปยัง view ที่ไม่ควรเห็น
- random result ต้องมี event ที่ตรวจสอบย้อนหลังได้

### Bitboard Representation For Fixed Slots

สนาม Vanguard มี slot หลักที่ค่อนข้างตายตัว จึงควรแทน occupancy และ relation ด้วย bit mask

ตัวอย่าง slot mask:

```text
VG      = 0000001
RC_FL   = 0000010
RC_FC   = 0000100
RC_FR   = 0001000
RC_BL   = 0010000
RC_BC   = 0100000
RC_BR   = 1000000
```

ใช้ bitboard เพื่อตอบคำถามเร็วๆ เช่น:

```text
occupiedFrontRow = occupiedMask & FRONT_ROW_MASK
emptyRearSlots = REAR_ROW_MASK & ~occupiedMask
canBoost = rearSlotOccupied & matchingFrontSlotOccupied
hasInterceptor = opponentFrontRowMask & interceptorMask
```

ข้อดี:

- hard pruning เร็วขึ้น
- invariant checker ง่ายขึ้น
- legal target query เร็วขึ้น
- power line calculation ง่ายขึ้น
- เข้ากับ Zobrist hashing และ state cache ได้ดี

ข้อควรระวัง:

- bitboard ใช้กับ slot fixed ได้ดี แต่ไม่ควรฝืนใช้แทนทุก zone
- deck, hand, drop, bind, soul ยังควรเป็น array/list ที่ preserve order หรือ visibility ตามกฎ
- Guardian Circle อาจต้องเป็น collection พร้อม aggregate shield ไม่ใช่ bit เดียวเสมอไป

## 13. Updated Five-Layer Blueprint

```text
The Guardian
Core + FSM + DSL + Rule Invariant Checker + Data-Driven GameState

The Accelerator
Zobrist Hashing + Bitboard Slot Mask + Transposition Table + Action Pruning + Search Budget

The Brain
Dynamic Evaluator + Behavior Trees + ISMCTS + Opponent Modeling

The Lab
Scenario DSL + Deterministic Bots + Regression Suite + Balance Simulation

The Auditor
Domain Events + Event Log + Decision Trace + Replay Verifier + Hidden Info Leak Detector
```

## 14. Rule Coverage Gap Checklist

หัวข้อนี้คือรายการที่ต้องเช็คเพิ่มก่อนเริ่ม implement จริง เพราะไม่ได้เป็น AI architecture โดยตรง แต่เป็นกฎพื้นฐานที่ถ้าขาดแล้ว Core จะไม่ครบ

### Zone Model / Card Identity

ต้อง model zone และ identity ให้ชัดตั้งแต่แรก

```text
CardDefinitionId = การ์ดชนิดนั้นคืออะไร
CardInstanceId   = ใบจริงในเกมใบนั้นคือใบไหน
OwnerPlayerId    = เจ้าของการ์ด
MasterPlayerId   = ผู้เล่นที่การ์ดสังกัดตามกฎ
ControllerId      = ผู้ควบคุมปัจจุบัน ถ้าระบบต้องรองรับ
CurrentZone       = zone ปัจจุบัน
ZoneOrderIndex    = ลำดับใน zone ถ้า zone นั้นมี order
VisibilityState   = public / hidden / revealed-to-player / face-down
```

Zone ที่ควรรองรับเป็น first-class:

```text
Deck
Hand
RideDeck
GZone
VanguardCircle
RearGuardCircle
GuardianCircle
OrderArea
TriggerZone
DamageZone
Soul
Drop
Bind
Gauge
Additional set-specific zones
```

กฎสำคัญ:

- การ์ด 1 instance อยู่ได้แค่ zone เดียว
- zone บางชนิดต้อง preserve order เช่น deck
- zone บางชนิดต้อง preserve face state เช่น damage, bind, G zone
- hidden/revealed state ต้องอยู่กับ zone entry ไม่ใช่อยู่แค่ card definition
- effect ที่ย้ายการ์ดต้อง emit event และ update hash

### Check Timing / Rule Action Engine

ต้องมีระบบ check timing แยกจาก action/effect resolution

Rule action ที่ควรเช็ค:

```text
Lose by 6 damage
Lose by deck out
Lose by no vanguard and no soul
Illegal guardian
Illegal trigger zone card
Illegal order area card
Illegal attached card
Excess energy
Damage application process
Multiple units on an exclusive circle
```

กฎสำคัญ:

- rule action ไม่ใช่ ability
- rule action อาจเกิดหลัง effect resolve แล้วเข้าสู่ check timing
- ถ้าหลาย rule action ต้องทำพร้อมกัน ให้ resolver รองรับ simultaneous processing
- event log ต้องแยก RuleActionEvent ออกจาก AbilityResolvedEvent

### Cost / Choice / Target Engine

Ability resolver ต้องแยก 4 ช่วงนี้ชัดๆ:

```text
1. Check play/activation legality
2. Choose targets / modes / numbers
3. Pay cost
4. Resolve effect as much as possible
```

ต้องรองรับ:

- choose up to X
- choose exactly X
- choose from hidden zone
- reveal เมื่อกฎบังคับให้พิสูจน์ว่าทำไม่ได้
- cost ที่จ่ายแล้วไม่ refund แม้ effect resolve ไม่ครบ
- effect ที่ทำได้บางส่วนต้องทำเท่าที่ทำได้
- turn player เลือกก่อนเมื่อหลายผู้เล่นต้องเลือกพร้อมกัน

### Order / Set Order / Blitz Order / Product-Specific Order

Order card ต้องเป็นระบบของตัวเอง ไม่ใช่ unit แบบพิเศษ

ต้องมี:

```text
PlayOrderAction
OrderArea
OrderPlayLimitPerTurn
NormalOrderTiming
BlitzOrderTiming
SetOrderPlacement
MusicOrderOnCircle
ArmsOrder / Product-specific order hooks
```

กฎสำคัญ:

- order ที่กำลังเล่น/resolve อยู่กับ order ที่ค้างใน order area ต้องแยกกัน
- blitz order ต้องผูกกับ guard/defense timing
- set order อาจอยู่ใน order zone/area ต่อเนื่องและมี continuous effect
- music/arms/product-specific order ต้องเข้าระบบ set-specific rules ไม่ hardcode ใน action หลัก

### Marker / Pseudo-Card / Token System

ต้องมีระบบ object ที่ไม่ใช่ card instance ปกติ

ควรรองรับ:

```text
Imaginary Gift marker
Force / Accel / Protect
Plant token
Crest
Ride deck crest
Ticket / gauge-like pseudo object
Set-specific pseudo-card
```

กฎสำคัญ:

- pseudo-card อาจมี owner/master แต่ไม่ควรปนกับ main deck card instance
- marker/pseudo-card ต้องเข้าระบบ visibility, event, serialization และ replay
- format profile ต้องเปิด/ปิด marker system ได้

### Deck Construction / Game Setup / Mulligan

Core ไม่ควรรอให้ UI validate deck แทน

ต้องมี:

```text
ValidateDeck(profile, deckList)
ValidateRideDeck(profile, rideDeck)
ValidateGZone(profile, gZone)
InitialSetup()
ChooseFirstVanguard()
ShuffleDeck(seed)
DrawOpeningHand()
Mulligan()
DetermineFirstPlayer()
```

กฎสำคัญ:

- setup เป็นส่วนหนึ่งของ replay/debug
- mulligan ต้อง preserve hidden information
- deck legality ต้องขึ้นกับ RuleSet/Profile
- tournament/floor rules อาจเป็นอีก layer แยกจาก comprehensive game rules

### Damage / Drive / Trigger Zone Pipeline

Drive check และ damage check ควรผ่าน pipeline เดียวกันบางส่วน แต่ต้องมี context แยก

```text
RevealToTriggerZone
DetectTriggerIcon
ResolveTriggerEffect
MoveFromTriggerZoneToDestination
RunCheckTiming
```

ต้องแยก context:

```text
DriveCheckContext
DamageCheckContext
EffectCheckContext
```

กฎสำคัญ:

- trigger zone ต้อง clear ตาม timing
- damage check destination คือ damage zone
- drive check destination ปกติคือ hand หลัง resolve
- effect บางใบอาจทำให้ check process เปลี่ยน destination หรือจำนวน check

### Search / Draw / Shuffle / Reveal Semantics

ต้องแยก action เหล่านี้ชัด:

```text
Draw
Look
Search
Reveal
Put
Shuffle
ChooseFromHidden
ChooseAtRandom
```

กฎสำคัญ:

- look ไม่เท่ากับ reveal
- search จาก hidden zone อาจมี proof/reveal rule เมื่อหาไม่ได้
- shuffle ต้องสร้าง event ที่ replay ได้ แต่ไม่ leak deck order
- AI view ต้องรู้แค่ว่ามี shuffle/reveal เกิดขึ้น ไม่ใช่รู้ลำดับใหม่

### Format / Set-Specific Rule Plugin

RuleSet flag อย่างเดียวอาจไม่พอในระยะยาว ควรมี plugin/hook สำหรับ set-specific mechanics

```text
RuleSetProfile
├─ enabledCoreRules
├─ enabledKeywords
├─ enabledCardTypes
├─ enabledTriggerTypes
├─ enabledPseudoCards
└─ setSpecificRuleModules
```

ตัวอย่าง module:

```text
StrideModule
ImaginaryGiftModule
RideDeckModule
EnergyModule
OrderModule
DressModule
ImprisonModule
CrestModule
EncounterModule
CollaborationSetModule
```

### Performance / Memory Budget

ต้องกำหนด budget ตั้งแต่แรก ไม่ใช่ optimize ทีหลัง

```text
Max legal action generation time
Max state clone time
Max hash update time
Max search memory
Max event log size per game
Max simulation playouts per decision
```

ต้องมี benchmark fixture สำหรับ:

- legal action generation
- snapshot/rollback
- Zobrist update
- transposition lookup
- 200ms search
- replay verification

### Documentation / Rules Versioning

ทุก rule implementation ต้องโยงกับ source rule version

```text
RuleVersion = "CFV Comprehensive Rules 4.52"
RuleSection = "13.10 Illegal Trigger"
ImplementedBy = "RuleActionResolver.IllegalTrigger"
TestFixture = "illegal-trigger-zone-card"
```

กฎสำคัญ:

- เมื่อ comprehensive rules update ต้องรู้ว่า test ไหนเสี่ยงพัง
- card DSL ควรมี version/schema migration
- replay เก่าต้องระบุ rule version ที่ใช้ตอนเล่น

## 15. Format Coverage Matrix

Core ต้องรองรับ format เป็น profile/module ไม่ใช่ hardcode เป็น Standard อย่างเดียว

แหล่งอ้างอิงที่ต้อง track:

```text
EN official:
  Fighter's Rules / Deck Construction Rules
  Comprehensive Rules
  Booster Draft and Sealed Rules

JP official:
  ファイターズルール
  デッキ構築ルール
  総合ルール

TH reference:
  ThaiVanguard rules / deck / tournament pages
```

### Constructed Format Profiles

```text
StandardNationFight
├─ regulationIcon = D
├─ deckIdentity = Nation
├─ mainDeckSize = 50
├─ rideDeck = required
├─ rideDeckSize = normally 5
├─ rideDeckCrest = required in current Standard
├─ triggerTotal = exactly 16 across main + ride deck
├─ overTriggerMax = 1
├─ healTriggerMax = 4
├─ criticalTriggerMax = 8
├─ drawTriggerMax = 8
├─ frontTriggerMax = 8
├─ standTrigger = disabled
├─ energy = enabled only with Energy Generator
├─ gDeck = conditionally enabled by specific Standard stride decksets/cards
└─ cardPool = D-icon cards plus explicit same-name/special allowances

VPremiumClanFight
├─ regulationIcon = V
├─ deckIdentity = Clan
├─ rideDeck = disabled
├─ gDeck = disabled
├─ imaginaryGift = enabled
├─ overTrigger = disabled unless a rule/event explicitly allows it
├─ frontTrigger = enabled for V-era card pool
├─ standTrigger = usually disabled in V-era card pool
└─ cardPool = V-icon cards plus explicit allowances

PremiumClanFight
├─ regulationIcon = All / Premium
├─ deckIdentity = Clan
├─ mainDeckSize = 50
├─ rideDeck = disabled by default
├─ gDeck = enabled
├─ gZoneSize = 0..16
├─ stride = enabled
├─ gGuard = enabled
├─ imaginaryGift = enabled when card/effect grants it
├─ overTrigger = enabled if legal card pool allows it
├─ standTrigger = enabled
├─ clanNationMapping = required for D-era nation cards
└─ cardPool = all sold/distributed cards subject to restrictions

PremiumExtremeFight
├─ regulationIcon = All / Premium
├─ deckIdentity = Mixed clan/nation
├─ rideDeck = disabled
├─ gDeck = enabled if profile allows
├─ triggerClanCondition = bypassed
├─ cardRestrictions = usually not applied unless event profile says otherwise
└─ cardPool = all sold/distributed cards with mixed clan/nation construction
```

### Limited / Event Format Profiles

```text
BoosterDraft
├─ productScope = overDress products D-BT01 onward unless event says otherwise
├─ players = 2+ recommended, 4+ better
├─ packCount = usually 5 per player
├─ draftDirection = left, then from fourth pack onward right
├─ generatedCardPool = drafted cards + prepared Grade 0 + trigger units + ride deck crest
├─ rideDeck = crest + G0 + G1 + G2 + G3
├─ mainDeckSize = 26
├─ triggerMainDeckMax = 9
├─ healTriggerMax = 2
├─ overTriggerMax = 1
├─ fourCopyRule = disabled
├─ loseAtDamage = 4
├─ allCardsAllNations = true
└─ triggersAlwaysActivate = true

Sealed
├─ productScope = overDress products D-BT01 onward unless event says otherwise
├─ packOpen = all packs opened by player
├─ generatedCardPool = opened cards + prepared Grade 0 + trigger units + ride deck crest
├─ rideDeck = crest + G0 + G1 + G2 + G3
├─ mainDeckSize = 26
├─ triggerMainDeckMax = 9
├─ healTriggerMax = 2
├─ overTriggerMax = 1
├─ fourCopyRule = disabled
├─ loseAtDamage = 4
├─ allCardsAllNations = true
└─ triggersAlwaysActivate = true
```

### Historical / Optional Profiles

ระบบไม่จำเป็นต้อง implement ทุกตัวตั้งแต่วันแรก แต่ data model ต้องไม่ปิดทาง

```text
GStandardHistorical
├─ purpose = legacy / replay / custom event
├─ cardPool = pre-V icon era by historical rule
├─ gDeck = enabled
├─ stride = enabled
├─ gGuard = enabled
├─ rideDeck = disabled
└─ note = historical regulation, not a current main EN/JP constructed regulation

LegacyPreDStandard
├─ purpose = old Standard/V-era replay support
├─ deckIdentity = Clan
├─ imaginaryGift = enabled if V-era
└─ note = useful for simulator archives, not first implementation priority

Title / Collaboration Event Profile
├─ purpose = Touken Ranbu, Buddyfight, BanG Dream, event-specific title fights
├─ deckIdentity = Title / World / special identity
├─ rideDeckRules = may override Standard defaults
├─ nationClanReference = may be replaced by title/world logic
└─ implementedAs = SetSpecificRuleModule + DeckConstructionProfile

LocalTournamentProfile
├─ purpose = Thai/local events such as D Fight, Normal Fight, title/nation events
├─ source = event announcement
├─ overrides = deck construction, allowed card pool, time rules, prizes
└─ implementedAs = EventProfile layered on top of base format
```

### Format Profile Schema

```text
FormatProfile
├─ formatId
├─ displayName
├─ sourceRegion
├─ sourceUrl
├─ effectiveDate
├─ ruleVersion
├─ deckConstruction
├─ allowedCardPool
├─ restrictionList
├─ identityRule
├─ triggerPolicy
├─ rideDeckPolicy
├─ gDeckPolicy
├─ energyPolicy
├─ markerPolicy
├─ orderPolicy
├─ keywordModules
├─ setSpecificModules
├─ limitedFormatSetup
├─ tournamentOverrides
└─ replayCompatibilityMode
```

### Format Rules ที่ห้ามลืม

- Format หลักปัจจุบันจาก official tournament rules คือ Standard, V Premium, Premium
- Premium มีทั้ง Clan Fight และ Extreme Fight
- Standard เป็น Nation Fight และมี ride deck เป็น default
- V Premium ไม่ใช้ ride deck
- Premium ใช้ G deck/G zone และต้องรองรับ stride/G-Guard
- Standard บาง deck ใช้ G deck ได้ผ่านเงื่อนไขเฉพาะ card/deckset
- Energy ไม่ใช่ global resource ของทุก format ต้องผูกกับ Energy Generator/crest
- Booster Draft/Sealed มี deck size, damage lose condition, trigger cap และ nation rule ต่างจาก constructed
- Card restrictions ต้องเป็น data ราย format และมี effective date
- Event/local format ต้องเป็น overlay profile ไม่ใช่แก้ Core rule กลาง

## 16. Official Source Index

ใช้ลิงก์เหล่านี้เป็น source อ้างอิงเวลาเอาเอกสารนี้ไปคุยกับ AI ตัวอื่น หรือใช้ตอน implement rule/test fixture

### English Official Sources

```text
Main site
https://en.cf-vanguard.com/

Rules / Q&A
https://en.cf-vanguard.com/howto/

Fighter's Rules
https://en.cf-vanguard.com/howto/fighters_rules/

Deck Construction Rules
https://en.cf-vanguard.com/howto/fighters_rules/deckrules/

Comprehensive Rules PDF
https://en.cf-vanguard.com/wordpress/wp-content/uploads/Cardfight-Vanguard-Comprehensive-Rules-4.52.pdf

Booster Draft and Sealed Rules PDF
https://en.cf-vanguard.com/wordpress/wp-content/uploads/VGD-Sneak-Preview-Recommended-Rules-20250228.pdf

Card List / Card Database
https://en.cf-vanguard.com/cardlist/

Products
https://en.cf-vanguard.com/products/
```

### Japanese Official Sources

```text
Main site
https://cf-vanguard.com/

大会ルール / Rules
https://cf-vanguard.com/howto/

ファイターズルール
https://cf-vanguard.com/howto/fighters_rule/

デッキ構築ルール
https://cf-vanguard.com/howto/fighters_rule/card/

カードリスト
https://cf-vanguard.com/cardlist/
```

### Thai Official / Local Distributor Reference

```text
Thai Vanguard main site
https://www.thaivanguard.net/

กฎการเล่น / Q&A
https://www.thaivanguard.net/rules/

การจัดเด็ค
https://www.thaivanguard.net/deck_cate/

การแข่งขัน / Tournament announcements
https://www.thaivanguard.net/category/event/
```

### Source Priority

```text
1. Comprehensive Rules
   ใช้เป็น source หลักของ game mechanics, phases, zones, actions, rule actions, keywords

2. Fighter's Rules / Deck Construction Rules
   ใช้เป็น source หลักของ format, card pool, restriction list, tournament regulation

3. Booster Draft / Sealed PDF
   ใช้สำหรับ limited/event format ที่มี deck size และ lose condition ต่างจาก constructed

4. Card database
   ใช้ตรวจ card text ล่าสุด, errata, card identity, icon, nation/clan/title

5. ThaiVanguard
   ใช้เป็น reference สำหรับสินค้าไทย, งานแข่งไทย, local event format และคำอธิบายภาษาไทย
```

### Implementation Note

เวลา implement rule หรือ test fixture ให้แนบ metadata แบบนี้เสมอ

```text
sourceRegion: "EN"
sourceType: "ComprehensiveRules"
sourceVersion: "4.52"
sourceUrl: "https://en.cf-vanguard.com/wordpress/wp-content/uploads/Cardfight-Vanguard-Comprehensive-Rules-4.52.pdf"
sourceSection: "13.10 Illegal Trigger"
effectiveDate: "2026-03-20"
```

ถ้าใช้ข้อมูล JP/TH ให้เก็บ `sourceRegion` แยก เพราะ rule version และวันที่อัปเดตอาจไม่ตรงกับ EN

## 17. Scope Status: Architecture vs Full Rule Script

ไฟล์นี้ตอนนี้เป็น architecture guardrails และ rule coverage checklist ไม่ใช่ full executable rule script ของทุก format

สถานะปัจจุบัน:

```text
ครอบคลุมแล้ว:
  - โครงสร้าง Core / AI / Lab / Auditor
  - รายการ module ที่ต้องมี
  - format coverage matrix
  - official source index
  - rule gap checklist
  - แนวทาง versioning และ test fixture

ยังต้องเขียนเพิ่มก่อน implement จริง:
  - step-by-step play procedure ของแต่ละ format
  - timing window matrix ราย format
  - deck construction validator ราย format
  - card pool / restriction list loader
  - keyword/module behavior ราย mechanic
  - scenario test fixture ตาม official rule section
```

ดังนั้นเอกสารนี้ “เข้าใจว่าต้องรองรับอะไร” แต่ยังไม่ได้ encode “ทุกขั้นการเล่นทุก format” แบบละเอียดครบ 100%

### Phase / Timing Matrix ที่ต้องทำต่อ

ควรสร้างตารางแยกสำหรับแต่ละ format:

```text
StandardNationFight
├─ Game Setup
│  ├─ validate 50-card main deck
│  ├─ validate ride deck / ride deck crest
│  ├─ determine first player
│  ├─ choose first vanguard
│  ├─ shuffle deck
│  ├─ draw opening hand
│  └─ mulligan
├─ Stand Phase
├─ Draw Phase
├─ Ride Phase
│  ├─ normal ride
│  ├─ ride deck discard cost
│  ├─ persona ride check
│  └─ G Assist if profile/event allows
├─ Main Phase
│  ├─ call
│  ├─ play normal order
│  ├─ play set order
│  ├─ activate ACT abilities
│  └─ resolve AUTO abilities/check timing
├─ Battle Phase
│  ├─ attack declaration
│  ├─ guard step
│  ├─ blitz order timing
│  ├─ drive check
│  ├─ damage check
│  ├─ battle resolution
│  └─ close step
└─ End Phase
   ├─ end of turn abilities
   ├─ cleanup temporary effects
   ├─ clear trigger/power modifiers
   └─ check timing
```

```text
VPremiumClanFight
├─ no ride deck
├─ no stride/G deck
├─ imaginary gift timing
├─ clan-based deck identity
├─ V-era trigger policy
└─ same base turn structure with V-specific modules
```

```text
PremiumClanFight
├─ no ride deck by default
├─ G zone / G deck enabled
├─ Stride Step in Ride Phase
├─ G-Guard Window in Guard Step
├─ legacy + V + D card interactions
├─ clan/nation mapping rules
├─ stand trigger enabled
└─ larger keyword/module coverage requirement
```

```text
PremiumExtremeFight
├─ mixed clan/nation construction
├─ trigger clan condition bypass/override
├─ restriction policy may differ by event
└─ otherwise built on Premium rule modules
```

```text
BoosterDraft / Sealed
├─ special setup from packs
├─ 26-card main deck
├─ prepared ride deck package
├─ trigger cap differs from constructed
├─ lose at 4 damage
├─ all cards treated as all nations
├─ triggers always activate
└─ four-copy rule disabled
```

### Definition of Done ก่อนบอกว่ารองรับทุก format

จะถือว่า “เข้าใจและรองรับทุกขั้นตอนทุก format” ได้ก็ต่อเมื่อมีครบ:

```text
1. FormatProfile data ครบทุก format
2. PhaseTimingMatrix ครบทุก format
3. DeckConstructionValidator ครบทุก format
4. RuleAction tests ครบตาม comprehensive rules
5. Keyword/module tests ครบตาม card pool ที่เลือก
6. Hidden-information tests ครบทุก zone
7. Replay tests ครบตั้งแต่ setup ถึง end game
8. Official source metadata ผูกกับทุก rule/test
```
