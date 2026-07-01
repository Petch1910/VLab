# Vanguard Implementation Checklist

## Source References

Primary source registry: `source_references.md`

Checklist items are based on:

- `SRC-RULES-455` - current Comprehensive Rules coverage
- `SRC-CORPUS-MECHANICS` - list of mechanics/keywords detected across rule versions
- `SRC-RULES-119` through `SRC-RULES-230` - historical mechanics required for older era support

เช็กลิสต์นี้แยกงานเป็น engine / module / card script เพื่อใช้วางแผนพัฒนาระบบ

## A. Core Engine

- [ ] CardDefinition / CardInstance model
- [ ] Zone model พร้อม visibility และ ordering
- [ ] Circle model พร้อม row/column/type
- [ ] Card movement API ที่รักษา previous/current info สำหรับ zone-change triggers
- [ ] Owner/master separation
- [ ] Event bus
- [ ] Check timing loop
- [ ] Play timing loop
- [ ] Rule action loop
- [ ] Continuous effect layer system
- [ ] Replacement effect system
- [ ] Cost payment system
- [ ] Target/choice system
- [ ] Hidden-zone search behavior
- [ ] Public-zone search behavior
- [ ] Timestamp/dependency handling for continuous effects

## B. Setup / Deck Construction

- [ ] Main deck validation
- [ ] Ride deck validation
- [ ] G deck validation
- [ ] Copy-name limit across main + ride deck
- [ ] Trigger count exactly 16
- [ ] Heal limit
- [ ] Over trigger limit
- [ ] Sentinel limit
- [ ] Regalis Piece limit
- [ ] Format legality
- [ ] Banned/restricted list input
- [ ] First vanguard selection
- [ ] Shuffle/mulligan
- [ ] First-player randomization and first player choice

## C. Turn / Phase Engine

- [ ] Stand Phase
- [ ] Draw Phase
- [ ] G Assist Step
- [ ] Ride Phase
- [ ] Ride Step
- [ ] Stride Step
- [ ] Main Phase
- [ ] Battle Phase
- [ ] End Phase
- [ ] Begin/end phase events
- [ ] Begin/end step events
- [ ] Until-end-of-turn cleanup

## D. Battle Engine

- [ ] Attack declaration
- [ ] Attack target selection
- [ ] Multiple attacked units
- [ ] Boost
- [ ] Guard from hand
- [ ] Intercept
- [ ] G Guardian
- [ ] Blitz Order
- [ ] Shield application
- [ ] Drive check
- [ ] Damage step power comparison
- [ ] Damage dealing by critical
- [ ] Damage check
- [ ] Hit / does-not-hit event timing
- [ ] Rear-guard retire on hit
- [ ] Guardian retire
- [ ] End-of-battle effects

## E. Trigger Engine

- [ ] Critical Trigger
- [ ] Draw Trigger
- [ ] Stand Trigger
- [ ] Heal Trigger
- [ ] Front Trigger
- [ ] Over Trigger
- [ ] Trigger power from icon
- [ ] Trigger effect nullification
- [ ] Drive-check-only Over Trigger text
- [ ] Damage check destination handling

## F. Specific Actions API

- [ ] stand/rest
- [ ] turn face up/down
- [ ] put
- [ ] draw
- [ ] look at deck
- [ ] switch
- [ ] discard
- [ ] reveal
- [ ] search
- [ ] shuffle
- [ ] declare
- [ ] give/lose/get ability
- [ ] activate
- [ ] place
- [ ] retire
- [ ] remove
- [ ] heal
- [ ] drive check
- [ ] damage check
- [ ] Counter-Blast
- [ ] Soul-Blast
- [ ] Counter-Charge
- [ ] Soul-Charge
- [ ] increase/decrease
- [ ] bind
- [ ] lock
- [ ] unlock
- [ ] delete
- [ ] time leap
- [ ] move/put unit
- [ ] deal damage
- [ ] attack
- [ ] battle
- [ ] change attack target
- [ ] dominate
- [ ] gauge operations
- [ ] Final Rush / Final Burst state
- [ ] world state
- [ ] Alchemagic
- [ ] Stand Up Vanguard
- [ ] Energy-Charge
- [ ] Energy-Blast
- [ ] choose/cannot be chosen

## G. Mechanic Modules

- [ ] Limit Break
- [ ] Forerunner
- [ ] Lord
- [ ] Sentinel
- [ ] Lock / Unlock
- [ ] Delete
- [ ] Legion
- [ ] Seek Mate
- [ ] D-Legion
- [ ] D-Seek Mate
- [ ] Stride
- [ ] Heart
- [ ] Stride Skill
- [ ] Generation Break
- [ ] G Guardian
- [ ] Ultimate Stride
- [ ] Gyze
- [ ] G-era keywords
- [ ] Imaginary Gift
- [ ] Front Trigger support
- [ ] Protect pseudo-card
- [ ] Ride Deck
- [ ] Persona Ride
- [ ] Order cards
- [ ] Set Order
- [ ] Blitz Order
- [ ] overDress
- [ ] XoverDress
- [ ] Final Rush / Final Burst
- [ ] World
- [ ] Alchemagic
- [ ] Energy
- [ ] Divine Skill
- [ ] Regalis Piece
- [ ] New DZ keywords

## H. Card Scripting

- [ ] Effect DSL or scripting API
- [ ] Cost parser
- [ ] Condition parser
- [ ] Timing parser
- [ ] Target selector parser
- [ ] Duration parser
- [ ] Keyword-to-module binding
- [ ] Manual override mechanism for complex cards
- [ ] Test fixture per card/mechanic

## I. Testing Priorities

- [ ] Deck validation tests
- [ ] Setup/mulligan tests
- [ ] Ride/call tests
- [ ] Battle hit/no-hit tests
- [ ] Trigger tests
- [ ] AUTO timing tests
- [ ] Continuous effect layer tests
- [ ] Replacement effect tests
- [ ] Rule action tests
- [ ] Legion attack tests
- [ ] Stride/Heart/GB tests
- [ ] G Guardian tests
- [ ] Imaginary Gift tests
- [ ] Ride Deck/Persona Ride tests
- [ ] overDress/XoverDress tests
- [ ] Energy/Divine Skill tests
- [ ] Title-specific edge tests

## J. Do Not Hardcode

- Do not hardcode trigger power globally; read from trigger icon/card data
- Do not hardcode all grade 3 rides as Persona Ride; require matching name and icon
- Do not treat Stride as Ride
- Do not treat overDress/XoverDress as Call
- Do not resolve AUTO immediately; put into standby and resolve through check timing
- Do not move cards directly between zones from card scripts; call specific-action APIs
- Do not mix owner and master
- Do not ignore hidden-zone search optionality
- Do not ignore public-zone mandatory selection
