# Cardfight!! Vanguard Rule Engine Development Spec

## Source References

Primary source registry: `source_references.md`

This spec is derived mainly from:

- `SRC-RULES-455` - Comprehensive Rules 4.55 for current rules, zones, timing, actions, keywords, rule actions, markers, and set-specific rules
- `SRC-CORPUS-HEADINGS` - section index used to locate rule groups across versions
- `SRC-CORPUS-MECHANICS` - mechanic presence matrix used to map modules by era
- `SRC-RULES-119` through `SRC-RULES-230` - historical rule versions used to separate OG, Legion, G, and V-era mechanics

แหล่งอ้างอิงหลัก: `outputs/vanguard_rules_markdown/versions/13_dz_rules_4_55.md` และ corpus เวอร์ชันก่อนหน้าใน `outputs/vanguard_rules_markdown/`

เป้าหมายของเอกสารนี้คือเปลี่ยน Comprehensive Rules ให้เป็น blueprint สำหรับพัฒนาระบบ ไม่ใช่สรุปเล่นเกมแบบย่อ

## 1. Design Principles

ระบบควรแยกกติกาเป็น 4 ชั้น:

1. **Core engine**: โครงเกมที่ทุกยุคใช้ร่วมกัน เช่น zones, events, check timing, play timing, battle, card movement
2. **Mechanic modules**: rule module ตามยุค เช่น Legion, Stride, Imaginary Gift, Ride Deck, overDress, Energy
3. **Format rules**: deck construction/card pool เช่น Standard, V Premium, Premium, title booster restrictions
4. **Card scripts**: effect เฉพาะใบ เช่น draw, retire, give power, search, conditional effects

ห้าม hardcode text เฉพาะการ์ดลง core engine เว้นแต่เป็น rule-defined action/keyword ที่ Comprehensive Rules กำหนด

## 2. Core Domain Objects

### Game

```text
Game
- players: [Player, Player]
- turn_player_id
- non_turn_player_id
- current_phase
- current_step
- current_attack: AttackContext | null
- event_queue
- standby_auto_abilities
- continuous_effects
- replacement_effects
- rule_action_queue
- format_context
- game_result
```

### Player

```text
Player
- id
- deck_zone
- hand
- drop_zone
- field
- soul
- damage_zone
- bind_zone
- trigger_zone
- g_zone
- gauge_zone
- order_area
- order_zone
- ride_deck_zone
- crest_zone
- energy_value
- vanguard_damage
- performed_gift_types
- used_divine_skill
- used_unique_skill
- regalis_piece_resolution_count
- world_state
- final_rush_state
- final_burst_state
- order_play_count_this_turn
```

### CardDefinition

```text
CardDefinition
- card_no
- names: string[]
- clan
- nation
- race
- grade
- power
- shield
- critical
- drive
- trigger_icons
- main_type
- subtypes
- special_types
- skill_icons
- special_icons
- abilities
- deck_construction_abilities
- format_legality
```

### CardInstance

```text
CardInstance
- instance_id
- definition_id
- owner_player_id
- master_player_id
- current_zone
- current_circle
- face_state: face_up | face_down
- orientation: stand | rest | landscape | none
- main_type_override
- associated_cards
- states
- temporary_abilities
- temporary_modifiers
- timestamps
- visibility
```

CardInstance ต้องถือว่าเป็น object ใหม่เมื่อย้ายข้ามโซน ยกเว้น movement แบบ circle-to-circle หรือกติกาเฉพาะระบุให้รักษา identity/association

## 3. Zones

ควร implement zones เป็น object ที่ระบุ:

```text
Zone
- zone_type
- owner_player_id
- visibility: public | hidden | conditional
- ordering: ordered | unordered | stack
- allows_face_down
- erase_zone_for_pseudocard_types
- cards
```

Zones ที่ต้องรองรับ:

- deck
- hand
- drop
- field
- circles
- vanguard circle
- rear-guard circles
- guardian circle
- soul
- damage
- bind
- trigger
- G zone
- gauge
- order area
- order zone
- ride deck
- crest
- removed-from-game area

## 4. Setup Flow

Setup ปัจจุบัน:

1. Validate deck construction
2. Present decks
3. Choose first vanguard from ride deck if present, otherwise main deck
4. Put main deck into deck zone and shuffle
5. Put ride deck into ride deck zone if present
6. Put G deck face down into G zone if present
7. Randomly determine a player; that player chooses who goes first
8. Set each player’s vanguard damage to 0
9. Draw initial 5
10. Mulligan in first-player order: put selected cards bottom deck, draw same count, shuffle if returned cards
11. Turn first vanguards face up
12. Start game with first player as turn player

Deck validation should be format-aware:

- Main deck exactly 50 non-G cards
- Ride deck 4 or more cards according to construction regulation
- G deck up to 16 G units
- Main + ride deck share 4-copy name limit
- Main + ride deck exactly 16 triggers
- Heal max 4
- Over max 1
- Sentinel max 4
- Regalis Piece max 1

## 5. Turn Loop

```pseudo
while game.not_over:
  run_stand_phase()
  run_draw_phase()
  run_g_assist_step()
  run_ride_phase()
  run_main_phase()
  run_battle_phase()
  run_end_phase()
  pass_turn()
```

Current 4.55 phase order:

1. Stand Phase
2. Draw Phase
3. G Assist Step
4. Ride Phase
5. Ride Step
6. Stride Step
7. Main Phase
8. Battle Phase
9. End Phase

ทุก phase/step ต้องสร้าง events เช่น `begin_phase`, `begin_step`, `end_step`, `end_phase` เพื่อให้ AUTO trigger ทำงาน

## 6. Check Timing

Check timing คือหัวใจของ Vanguard engine

```pseudo
function resolve_check_timing():
  loop:
    collect_rule_actions()
    if rule_actions exist:
      resolve_rule_actions_simultaneously()
      continue

    if turn_player has standby auto abilities:
      ability = turn_player.choose_standby_auto()
      play_and_resolve_auto(ability)
      continue

    if non_turn_player has standby auto abilities:
      ability = non_turn_player.choose_standby_auto()
      play_and_resolve_auto(ability)
      continue

    break
```

Rule action ต้องถูกตรวจซ้ำหลังทุก ability เพราะ ability อาจสร้าง board state ใหม่ เช่น damage เพิ่ม, unit ซ้อน, order ผิดโซน, energy เกิน

## 7. Play Timing

```pseudo
function grant_play_timing(player, available_actions):
  resolve_check_timing()
  action = player.choose_action_or_pass(available_actions)
  if action == pass:
    return
  perform(action)
  grant_play_timing(player, available_actions)
```

Main phase play timing:

- Normal call unit to RC
- Switch front/back rear-guards in same column
- Play non-blitz order within order limit
- Play active ACT abilities
- Perform overDress from hand if legal
- Pass

Guard step play timing:

- Call guardians from hand
- Use G Guardian if legal
- Intercept
- Play blitz order within order limit
- Pass

## 8. Ability Model

```text
Ability
- ability_id
- source_card
- type: ACT | AUTO | CONT
- effective_zones
- restriction_icons
- formal_keywords
- trigger_condition
- activation_condition
- requirement
- cost
- effect_program
- duration
- once_scope
- created_by_effect_id
```

### ACT

- Played by player during play timing
- Cost is paid before resolution
- If cost cannot be paid, play attempt is invalid/canceled

### AUTO

- Triggers when event/situation condition is fulfilled
- Enters standby
- If it has cost, cost is paid during resolution
- Must be resolved if legal unless rules allow choosing it to cease without playing, such as certain restriction icons

### CONT

- Not played
- Applies continuously while active
- Must participate in continuous-effect layer system

## 9. Effect Model

```text
Effect
- effect_type: one_shot | continuous | replacement
- source
- master
- target_selector
- instructions
- duration
- timestamp
- dependency_inputs
```

Effect categories:

- **One-shot**: draw, retire, search, call, heal, bind
- **Continuous**: power +10000 until end of turn, cannot attack, gains ability
- **Replacement**: if event A would happen, do event B instead

Continuous effects must apply in rule-defined order:

1. Printed/base information
2. Original information modifications
3. Losing original information
4. Rule modifications not from effects
5. Ability gain/loss and active/inactive changes
6. Non-numeric modifications
7. Numeric increase/decrease
8. Boost/guardian/legion/attach/Link Attack modifications
9. Set numeric value

## 10. Cost Model

```text
Cost
- components: CostComponent[]
- payment_timing: on_play | on_resolution
- simultaneous: true
- optional: false
```

Cost components:

- Counter-Blast
- Soul-Blast
- Energy-Blast
- discard
- retire own unit
- reveal
- bind
- remove
- rest
- turn face up/down in G zone
- return cards from drop to deck
- other card-defined costs

ถ้า cost ใดจ่ายไม่ได้ ห้ามจ่ายทั้งหมดและ ability/card ไม่ถูกเล่นหรือไม่ resolve ส่วนนั้น

## 11. Event Model

Events ต้องละเอียดพอให้ AUTO trigger ได้ถูกต้อง

ตัวอย่าง event:

```text
game_started
turn_started
phase_started
step_started
card_moved
card_placed
unit_called
unit_ridden
unit_rode_upon
unit_legioned
unit_strode
unit_became_heart
unit_attacks
unit_is_attacked
unit_boosts
unit_is_boosted
drive_check_performed
damage_check_performed
trigger_revealed
attack_hits
attack_does_not_hit
battle_ended
card_retired
card_bound
card_locked
card_unlocked
card_deleted
order_played
persona_ride_performed
energy_changed
divine_skill_used
```

Zone-change trigger ต้องเก็บ previous information และ current information เพราะบาง AUTO ต้องอ้างอิงข้อมูลก่อนออกจาก field

## 12. Specific Actions

Specific actions คือ primitive API ของ engine:

- stand/rest
- turn face up/down
- put
- shuffle
- draw / move cards from deck
- look at deck
- switch
- discard
- reveal
- search
- declare
- give/lose/get
- activate
- trigger unit is revealed
- place
- retire
- remove
- heal
- losing cost
- perform drive check
- perform damage check
- Counter-Blast
- Soul-Blast
- Counter-Charge
- Soul-Charge
- increase/decrease
- bind
- lock
- unlock
- delete
- time leap
- move/put a unit
- deal damage
- attack
- battle
- change attack target
- dominate
- put/move gauge
- become the stage
- Final Rush / Final Burst
- world changes
- Alchemagic
- Stand Up the Vanguard
- perform Twin Drive
- boost
- Energy-Charge
- Energy-Blast
- choose/cannot be chosen

Card scripts should call these APIs rather than directly mutating zones.

## 13. Battle Engine

```pseudo
function battle_phase():
  while true:
    start_step()
    if turn_player_declines_attack_or_cannot_attack:
      break
    attack_step()
    guard_step()
    drive_step_if_applicable()
    damage_step()
    close_step()
```

### Attack Step responsibilities

- Choose specified attacking unit
- Confirm legality
- Rest attacking unit
- Emit `unit_attacks`
- Choose unit(s) being attacked
- Emit `unit_is_attacked`
- Assign guardian mapping for later guardians
- Choose boost if legal
- Apply boost continuous modifier
- Emit boost events

### Guard Step responsibilities

- Allow hand guardians
- Allow G Guardian
- Allow intercept
- Allow blitz order
- Apply shield as continuous modifier while guardian exists

### Drive Step responsibilities

- If attacker is vanguard or effect grants drive checks, perform drive checks
- Put top card to trigger zone
- Resolve trigger ability if applicable
- Move card from trigger zone to hand unless replacement/special rule changes it

### Damage Step responsibilities

- Compare power
- Determine hit/not hit
- If vanguard hit, deal damage equal to critical
- For each damage, damage check
- Move damage check card to damage zone unless replacement/special rule changes it
- Retire hit rear-guards and guardians
- Emit hit/not-hit events at correct timing

### Close Step responsibilities

- Resolve end-of-battle AUTO
- Clear battle-duration effects
- Clear attacking/being attacked/boosting designations
- Return to start step

## 14. Trigger Engine

```text
TriggerAbility
- trigger_type
- power_value_from_icon
- sub_abilities
- drive_check_bonus_text
```

Trigger types:

- Critical: +critical and +power
- Draw: draw and +power
- Stand: stand rear-guard and +power
- Heal: conditional heal and +power
- Front: front row +power
- Over: remove self, draw, +100000000, drive-check-only extra text

Implementation details:

- Trigger effect order can matter when multiple sub-abilities exist
- Trigger effects can be nullified by effects
- Power value comes from icon, not hardcoded globally, because old/new triggers differ
- Over Trigger has special destination handling because it removes itself

## 15. Placement / Major Mechanics

### Ride

- Normal ride from hand or ride deck according to grade rules
- Superior ride from effect
- Old vanguard goes to soul
- Persona Ride check occurs during ride process if names and icons match
- Ride is different from Stride

### Call

- Normal call during main/guard step
- Superior call from effects
- Guardian call enters guardian circle in rest state
- overDress/XoverDress placement is not a call

### Legion / D-Legion

- Associates two vanguards
- Leader + Mate relation must be stored
- Both are vanguards
- Legion Mate cannot be attacked
- Attacking in legion uses leader/mate rules
- D-Legion has additional icon/persona ride handling

### Stride / Heart

- G unit from G zone enters VC
- Previous vanguard(s) become heart
- G unit gains selected heart name and power
- Not a ride
- End phase returns G unit to G zone face up
- Face-up G units enable Generation Break

### overDress / XoverDress

- Places a unit on RC using originalDress association
- Not a call
- OriginalDress cards must remain associated
- Effects that move/imprison unit may also move associated originalDress according to rules

### Attach / Armed

- Attached units/orders are associated with another unit
- Associated cards may grant ability/power
- End phase or movement may detach/move according to rules

## 16. Keyword Architecture

แบ่ง keyword เป็น implementation classes:

### Built-in ability keywords

- Drive abilities
- Intercept
- Boost
- Restraint
- Sentinel
- Resist

### Activation condition keywords

- Limit Break
- Generation Break
- Brave
- Oracle
- Ritual
- Unite
- Thunderstrike
- Darkness
- Dark Device
- White Wings / Black Wings
- Powerful
- Glitter

### Procedure keywords

- Seek Mate / D-Seek Mate
- Stride Skill
- Ultimate Stride Skill
- G Guardian
- overDress
- XoverDress
- Regalis Piece
- DressUp
- Ace Unit
- Rewrite

### State/marker keywords

- Blaze
- Afterimage
- Engorge
- Rush
- Charge
- Hollow
- Harmony
- Success
- Melody
- Friend
- RevolDress
- UnisonDress
- Overcharge
- Malwyrm
- Mental Pollution
- Happy Toys
- SweetLink
- Arma Arms
- Fantôme Skill

### Pattern names, not formal keyword modules

- Break Ride
- Cross Ride
- Ultimate Break
- Mega Blast
- Persona Blast
- Stride Bonus

These should be modeled as card-script patterns using formal keywords/events.

## 17. Rule Actions

Rule actions to implement:

- Losing the Game
- Overloaded Units
- Illegal Guardians
- Having No Vanguard
- Damage Application Process
- Damage Resolution Process
- Erasure of pseudo-cards
- Illegal Gauge
- Illegal Trigger
- Illegal Order
- Illegal Attached Card
- Excess Energy
- Buddyfight-related Illegal Associations

Rule actions must be deterministic and should not be implemented as optional card scripts.

## 18. Marker / Pseudo-card System

Need object type:

```text
PseudoCard
- pseudo_type: protect | token_unit | token_set_order | ticket | crest | ride_deck_crest
- name
- owner
- master
- effective_zones
- erase_zones
- generated_information
```

Pseudo-cards may be erased when moved to invalid zones. Token units and token set orders have specific effective zones.

## 19. Format Support

Support should not be hardcoded into card logic.

```text
Format
- name
- legal_card_sets
- deck_size
- ride_deck_rule
- g_deck_rule
- clan_or_nation_rule
- trigger_limits
- sentinel_limit
- regalis_piece_limit
- banned_restricted_list
```

Likely formats:

- Standard / D
- V Premium
- Premium
- Title-specific variants

## 20. Recommended Engine Module Layout

```text
core/
  game_state
  zones
  card_instances
  events
  check_timing
  play_timing
  continuous_effects
  replacement_effects
  rule_actions
  actions
  battle
  triggers
  deck_validation

mechanics/
  limit_break
  lock_delete
  legion
  stride
  g_guardian
  imaginary_gift
  order
  ride_deck
  persona_ride
  overdress
  energy
  divine_skill
  markers
  title_specific

cards/
  parser
  effect_dsl
  scripts

formats/
  standard
  v_premium
  premium
```

## 21. Minimum Viable Implementation Order

1. Card objects and zones
2. Deck validation for one target format
3. Setup and mulligan
4. Turn loop
5. Ride/call/move/retire basic actions
6. Battle with guard/drive/damage
7. Trigger engine
8. Cost engine: CB/SB/discard
9. Ability system ACT/AUTO/CONT
10. Continuous and replacement effects
11. Rule actions
12. Mechanic modules by era
13. Full card scripting

For fastest working prototype, start with OG core before adding D/DZ modules.
