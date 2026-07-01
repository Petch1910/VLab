# โครงสร้างระบบหลังปรับแผน M20-M27

## ภาพรวม

```mermaid
graph TB
    subgraph DATA["📦 Data Layer"]
        SRC["KK Card Fight Thai Export<br/>10,836 cards / 2.16 GiB images"]
        SQL["cards.sqlite"]
        MAN["manifest.json"]
        IMG["images/"]
        AI["asset_index.json<br/>SHA-256 per image"]
        ABL["structured_ability_pack<br/>20 template abilities"]
        SRC --> SQL
        SRC --> IMG
        SQL --> MAN
        IMG --> AI
    end

    subgraph TOOLS["🔧 Python Tools"]
        BLD["build_vanguard_th_pack.py"]
        VER["verify_vanguard_th_pack.py"]
        IMP["import_custom_pack.py"]
        VAL["validate_custom_pack_schema.py"]
        ABLV["validate_ability_schema.py"]
    end

    subgraph CORE["⚙️ RulesCore (C# Pure Logic)"]
        GS["GameState"]
        GAS["GameActionService"]
        RC["RulesCore Facade"]
        LAG["LegalActionGenerator"]
        PTM["PhaseTimingMatrix"]
        EVT["GameEvent / Reducer"]
        SNP["Snapshot / Rollback"]
        RNG["SeededRandomService"]
        HSM["GameStateViewFactory<br/>Hidden State Masking"]
        RL["ResourceLedger<br/>CB/SB/Energy"]
        RSP["RuleSetProfile<br/>Standard/V-Premium/Premium"]
    end

    subgraph ABILITY["🎯 AbilityCore"]
        AC["AbilityCore"]
        REG["RuntimeAbilityRegistry"]
        TRG["TriggerResolver"]
        TAP["TriggerAllocationPlan"]
        CML["CombatModifierLedger"]
        PAQ["PendingAutoAbilityQueue"]
        MFB["ManualFallbackBridge"]
    end

    subgraph BOT["🤖 AI / Bot Layer"]
        TPE["TriggerProbabilityEngine"]
        BRE["BoardResourceEvaluator"]
        BSS["BattleSequenceSearch"]
        OGE["OpponentGuardEstimator"]
        PBK["ArchetypePlaybook"]
        CD["ComboDiscovery"]
        PBC["ProfileBotController"]
        ADV["AdvancedSearchPrototype"]
    end

    subgraph MP["🌐 Multiplayer"]
        MPR["MultiplayerProtocol"]
        PHO["PhotonRealtimeAdapter"]
        LOB["MultiplayerLobbyController"]
        GSC["GameSessionController"]
        DPV["DeckPrivacy / Commitment"]
        PUB["PublicGameEvent / Masking"]
        REC["Reconnect / Batch"]
    end

    subgraph UI["🖥️ Unity UI (Windows-First)"]
        HOM["Home / Lobby"]
        CB["Card Browser"]
        DB["Deck Builder"]
        PT["PlayTable"]
        REP["Replay Viewer"]
        OR["Online Room"]
        SET["Settings"]
        MNL["In-App Manual"]
    end

    subgraph HL["🧪 Headless / Research"]
        HCL["HeadlessCLI Runner"]
        HBR["Batch Runner"]
        HDS["Dataset Export"]
        OAR["Observation/Action/Reward API"]
        HPF["Performance Profiler"]
    end

    subgraph CI["🔄 CI / Build"]
        GHA["GitHub Actions"]
        PYT["Python Tests"]
        UEC["Unity Compile"]
        UET["Unity EditMode"]
        WBA["Windows Build"]
        ABA["Android Build"]
    end

    DATA --> CORE
    TOOLS --> DATA
    CORE --> ABILITY
    CORE --> BOT
    CORE --> MP
    CORE --> UI
    CORE --> HL
    ABILITY --> CORE
    BOT --> CORE
    MP --> UI
    CI --> TOOLS
    CI --> CORE
```

---

## Layer-by-Layer

### 1. Data Layer — สิ่งที่มีอยู่แล้ว ✅

| Component | สถานะ | ไฟล์/ตำแหน่ง |
|-----------|--------|-------------|
| Card database (SQLite) | ✅ Done | `data/packs/vanguard_th/cards.sqlite` |
| Manifest | ✅ Done | `data/packs/vanguard_th/manifest.json` |
| Image files (10,836) | ✅ Done | `data/packs/vanguard_th/images/` |
| Asset index (SHA-256) | ✅ Done | `data/packs/vanguard_th/asset_index.json` |
| Structured ability pack | ✅ Done | `data/packs/vanguard_th/abilities/` |
| Custom pack template v1/v2 | ✅ Done | `data/templates/custom_pack[_v2]/` |

---

### 2. RulesCore — สถานะปัจจุบันกับส่วนที่ต้องเพิ่ม

```mermaid
graph LR
    subgraph DONE["✅ มีแล้ว"]
        GS["GameState"]
        GAS["GameActionService"]
        RC["RulesCore"]
        LAG["LegalActionGenerator"]
        EVT["GameEvent/Reducer"]
        SNP["Snapshot/Rollback"]
        RNG["SeededRNG"]
        HSM["HiddenState Masking"]
        RL["ResourceLedger"]
        PTM["PhaseTimingMatrix"]
        RSP["RuleSetProfile"]
    end

    subgraph NEW["🔴 ต้องเพิ่ม M21"]
        SOUL["GameZone.Soul"]
        RIDE["Ride → Soul Transfer"]
        SBL["SoulBlast ← Soul Count"]
        ATK["Attack Declaration Flow"]
        PHE["Phase Enforcement in UI"]
        SETUP["Game Setup / Mulligan"]
    end

    GS --> SOUL
    GAS --> RIDE
    RL --> SBL
    PTM --> PHE
    LAG --> ATK
    GS --> SETUP
```

#### รายละเอียด Gap Fix ใน Core

| Gap | Task | ผลกระทบ |
|-----|------|--------|
| **Soul zone ไม่มี** | M21-04b: เพิ่ม `GameZone.Soul` | Ride ต้องส่ง vanguard เดิมเข้า soul, SoulBlast ต้องนับจาก zone จริง |
| **Attack flow ไม่ครบ** | M21-05: wire attack declaration | ต้องมี attacker → target → guard → drive → battle → close |
| **Phase ไม่บังคับ** | M21-05: wire PhaseTimingMatrix | ผู้เล่นต้อง draw ใน Stand & Draw, ride ใน Ride Phase เท่านั้น |
| **Game setup ไม่มี** | M21-05b: guided setup wizard | choose vanguard → shuffle → draw 5 → mulligan → stand up |

---

### 3. Unity UI — สิ่งที่เปลี่ยนหลังปรับแผน

```mermaid
graph TB
    subgraph SCREENS["จอหลัก M20-M27"]
        HOM["🏠 Home"]
        CB["🔍 Card Browser"]
        DB["🃏 Deck Builder"]
        PT["⚔️ PlayTable"]
        OR["🌐 Online Room"]
        SET["⚙️ Settings"]
        MNL["📖 Manual"]
        REP["🔄 Replay"]
    end

    HOM -->|"เลือก deck + format"| DB
    HOM -->|"Solo Play NEW"| SOLO["เลือก deck → bot difficulty → bot deck"]
    HOM -->|"Online Room"| OR
    HOM -->|"Card Browser"| CB
    HOM -->|"Settings"| SET
    HOM -->|"Manual"| MNL

    DB -->|"เปิด PlayTable ด้วย deck"| PT
    SOLO -->|"เปิด PlayTable กับ Bot"| PT
    OR -->|"Lobby → Start Table"| PT

    PT -->|"Game Setup NEW"| SETUP["choose VG → shuffle → draw 5 → mulligan"]
    SETUP --> PLAY["Gameplay Loop"]
    PLAY -->|"phase-enforced NEW"| PHASES

    subgraph PHASES["Phase Flow (enforced)"]
        SP["Stand Phase"]
        SD["Stand & Draw"]
        RP["Ride Phase"]
        MP["Main Phase"]
        BP["Battle Phase"]
        EP["End Phase"]
        SP --> SD --> RP --> MP --> BP --> EP
    end
```

#### PlayTable UI Changes (M21)

| Before (M19) | After (M21+) |
|--------------|-------------|
| Debug-style buttons, all actions always available | Phase-enforced: actions enabled/disabled per phase |
| No card images on board | 🆕 Card thumbnail on circle slots |
| Text-only zone status, `Soul: not modeled` | 🆕 Soul zone count from real data |
| Raw event log | Player-readable: `P1 drew 1 card.` |
| No attack flow | 🆕 Attacker → Target → Guard → Drive → Battle |
| Open table directly | 🆕 Deck selection → Guided game setup → Play |
| No card text in preview | 🆕 Thai skill text in selected-card preview |

---

### 4. Multiplayer — ไม่เปลี่ยน Architecture

```text
สิ่งที่มีอยู่แล้ว (M8-M13):
├─ PhotonRealtimeAdapter (trusted-client)
├─ MultiplayerLobbyController (host/join/ready/start)
├─ MultiplayerGameSessionController (event sync)
├─ DeckPrivacy / Commitment (friend-room only)
├─ PublicGameEvent masking
├─ Reconnect / Batch recovery
├─ SpectatorReplay sync
└─ TournamentAuditLog export

M25 ปรับแค่ UX:
├─ Lobby flow ง่ายขึ้น
├─ Room status ชัดขึ้น
├─ Reconnect UX ดีขึ้น
└─ ซ่อน debug payload จาก default view
```

---

### 5. Bot / AI — ไม่เปลี่ยน Architecture

```text
สิ่งที่มีอยู่แล้ว (M5-M14):
├─ ProfileBotController (Aggro/Balanced/Defensive)
├─ TriggerProbabilityEngine
├─ BoardResourceEvaluator
├─ BattleSequenceSearch
├─ OpponentGuardEstimator
├─ ArchetypePlaybook
├─ ComboDiscovery
└─ AdvancedSearchPrototype (one-ply)

M26 เพิ่ม UX เท่านั้น:
├─ 🆕 Solo Play entry flow จาก Home
├─ 🆕 Bot difficulty selection UI
├─ 🆕 Bot deck selection
└─ Player-readable bot explanation panel
```

---

### 6. Full Component Map

```text
Scripts/Vanguard/
├── Cards/           ← SqliteCardRepository, CardImageCache, CardDefinition
├── Decks/           ← VanguardDeck, DeckValidator, DeckStorage, DeckCodeCodec
├── Game/            ← RulesCore, GameState, GameActionService, LegalAction,
│                       AbilityCore, TriggerResolver, CombatModifier, ResourceLedger,
│                       PhaseTimingMatrix, RuleSetProfile, Snapshot, SeededRNG,
│                       GameStateViewFactory, PendingAutoAbilityQueue, Replay
│                    🆕 GameZone.Soul, AttackDeclarationFlow, GameSetupWizard
├── Bots/            ← ProfileBotController, BattleSequenceSearch, BoardEvaluator,
│                       TriggerProbability, GuardEstimator, Playbook, ComboDiscovery
│                    🆕 SoloPlayEntryFlow
├── Multiplayer/     ← PhotonAdapter, LobbyController, GameSessionController,
│                       DeckPrivacy, PublicGameEvent, Reconnect, TournamentAudit
├── UI/              ← HomeBootstrap, CardBrowserBootstrap, DeckBuilderBootstrap,
│                       PlayTableBootstrap, MultiplayerLobbyBootstrap,
│                       ResponsiveLayoutProfile, UiGameSymbolRegistry
│                    🆕 SettingsScreen, ManualScreen, GameSetupUI, AttackFlowUI,
│                       CardThumbnailOnBoard, PhaseEnforcementUI, DeckSelectDialog,
│                       BotDifficultyDialog
├── Headless/        ← HeadlessCLI, BatchRunner, DatasetExport, ObservationAPI, Profiler
├── Smoke/           ← ClientSmokeFlowRunner, PlayerSmokeBootstrap
│                    🆕 PlayModeIntegrationTest (M27-06)

Assets/Tests/        ← 904 EditMode tests
                     🆕 PlayMode integration tests (M27-06)
```

---

### 7. Data Flow Diagram

```mermaid
sequenceDiagram
    participant U as Player
    participant UI as Unity UI
    participant RC as RulesCore
    participant GS as GameState
    participant EVT as EventLog
    participant BOT as Bot (masked view)
    participant NET as Photon (online)

    Note over U,UI: M21-05b: Game Setup Flow (NEW)
    U->>UI: Select Deck → Start Game
    UI->>RC: CreateGame(deck, seed)
    RC->>GS: Initialize zones + shuffle + draw 5
    GS-->>UI: Display setup state
    U->>UI: Mulligan choices
    UI->>RC: MulliganAction(cards)

    Note over U,UI: M21-05: Phase-Enforced Gameplay (EXPANDED)
    loop Each Turn
        RC->>UI: Current phase + legal actions
        U->>UI: Choose action (draw/ride/call/attack)
        UI->>RC: SubmitCommand(action)
        RC->>RC: Validate legality
        RC->>GS: Apply state change
        RC->>EVT: Emit GameEvent
        GS-->>UI: Updated display (masked view)
        
        alt Online Mode
            RC->>NET: Publish NetworkEventEnvelope
            NET-->>RC: Remote events
        end
        
        alt vs Bot
            RC->>BOT: GetLegalActions(masked state)
            BOT->>RC: ChooseAction
        end
    end
```

---

### 8. สรุป: อะไรเปลี่ยน อะไรไม่เปลี่ยน

| Layer | Architecture เปลี่ยน? | เปลี่ยนอะไร |
|-------|---------------------|------------|
| **Data** | ❌ ไม่เปลี่ยน | — |
| **Python Tools** | ❌ ไม่เปลี่ยน | — |
| **RulesCore** | ⚠️ เพิ่ม component | +Soul zone, +Attack flow, +Game setup, +Phase enforcement in UI |
| **AbilityCore** | ❌ ไม่เปลี่ยน | SoulBlast wire เข้า Soul zone แต่ architecture เดิม |
| **Bot / AI** | ❌ ไม่เปลี่ยน | เพิ่มแค่ UX entry flow |
| **Multiplayer** | ❌ ไม่เปลี่ยน | เพิ่มแค่ UX polish |
| **Unity UI** | ✅ เพิ่ม screens | +Settings, +Manual, +Game Setup, +Attack UI, +Phase UI, +Card-on-board |
| **Headless** | ❌ ไม่เปลี่ยน | — |
| **CI/Build** | ⚠️ เพิ่ม test type | +PlayMode integration test |

> **สรุป**: Architecture หลักไม่เปลี่ยน — ยังคง RulesCore เป็นศูนย์กลาง, UI ส่ง command ผ่าน facade, Bot อ่าน masked view เท่านั้น สิ่งที่เพิ่มคือ **component ที่ขาดใน Core** (Soul, Attack, Setup) และ **หน้าจอ UI ใหม่** (Settings, Manual, Game Setup) ไม่มีการเพิ่ม dependency ใหม่หรือเปลี่ยน stack
