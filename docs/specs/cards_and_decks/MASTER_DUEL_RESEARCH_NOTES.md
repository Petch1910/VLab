# Master Duel Static Research Notes

## Scope

Local path inspected:

`C:\Program Files (x86)\Steam\steamapps\common\Yu-Gi-Oh!  Master Duel`

This note records architecture-level observations only. The game was not run,
debugged, patched, decompiled, or unpacked. Asset bundles and cached content
were not extracted.

Safe inspection performed:

- top-level file inventory
- Unity metadata/config text files
- PE header, hash, and Authenticode status
- readable exported/string symbols from binaries
- high-level cache/save directory layout

Not performed:

- running `masterduel.exe`
- loading native DLLs
- decompiling IL2CPP metadata
- extracting Unity asset bundles
- copying proprietary code, assets, or data into this project

## Top-Level Layout

Observed top-level files and directories:

- `masterduel.exe`
- `GameAssembly.dll`
- `UnityPlayer.dll`
- `baselib.dll`
- `UnityCrashHandler64.exe`
- `masterduel_Data`
- `D3D12`
- `LocalData`
- `LocalSave`

The client is a Unity IL2CPP game. The important structural point is that the
duel/rules layer appears to be separated into a native plugin:

- `masterduel_Data\Plugins\x86_64\duel.dll`

This is directly relevant to our Vanguard simulator architecture: the duel core
should be a distinct rules module with a narrow command/query surface, while the
Unity UI only displays state and sends commands.

## Steam Manifest

Text manifest read:

`C:\Program Files (x86)\Steam\steamapps\appmanifest_1449850.acf`

Relevant metadata:

- App ID: `1449850`
- Name: `Yu-Gi-Oh!  Master Duel`
- Installed depot: `1449853`
- Build ID: `23020425`
- Language: `english`
- Steam-reported base size: `455448440` bytes

## Unity Metadata

`masterduel_Data\app.info`:

- Company: `Konami Digital Entertainment Co., Ltd.`
- Product: `masterduel`

`masterduel_Data\boot.config`:

- `gfx-threading-mode=6`
- `wait-for-native-debugger=0`
- `hdr-display-enabled=0`
- `gc-max-time-slice=3`
- `build-guid=8fdbf9a3d58246a78fa8c2594b8d6b8f`

`masterduel_Data\ScriptingAssemblies.json` includes:

- `Assembly-CSharp.dll`
- `CardRenderSystem.dll`
- `com.rlabrecque.steamworks.net.dll`
- Spine runtime assemblies
- Firebase assemblies
- Unity Input System, Burst, URP, TextMeshPro, Timeline, Recorder
- `YgomDebug.Network.dll`
- `YgomDebug.Network.DummyResponseGenerator.dll`

Inference: the client has a broad Unity presentation/application layer, while
the duel rules are exposed through native plugin calls. Network/debug-related
assembly names also imply that API response boundaries are separated from local
presentation code, but this note does not inspect those internals.

## Binary Metadata

All three key binaries were PE64 x64 and were not Authenticode signed in this
local installation.

| File | SHA256 | PE timestamp UTC |
| --- | --- | --- |
| `masterduel.exe` | `4B2F6C368B30B7BF137BC4D1953863490E4CF7861F865C567EA578C2A5CF7E44` | `2025-10-22 07:59:28` |
| `GameAssembly.dll` | `4FD2F6EDACD98FEA0945DD237F8312A166CA5CAB3D0B0E4B77FE6196C362F1EE` | `2026-03-30 10:07:33` |
| `duel.dll` | `F9361B293F772EA7650891BD444184A11326DB089140AEF1410C04078202D738` | `2026-03-21 06:48:25` |

## Content And Save Layout

Base `StreamingAssets\AssetBundle`:

- 154 bucket directories
- 234 files
- about 110 MB total
- hashed filenames without descriptive extensions

Downloaded/local cache `LocalData`:

- root bucket: `95ffc08a`
- `0000` contains 257 bucket directories
- 38,361 files
- about 13.3 GB total
- hashed filenames without descriptive extensions

Local save `LocalSave`:

- root bucket: `95ffc08a`
- `0000` contains `00` through `ff` bucket directories plus `root`
- only a few small files were present in this fresh local state

Inference: Master Duel keeps the executable/client relatively small and stores
large evolving content in a content-addressed cache. For our project, card image
packs and future downloadable packs should follow a manifest plus hash-cache
model instead of baking all content into the executable.

## Duel Plugin Strings

Readable strings from `duel.dll` expose a useful command/query shape.

Card database/query examples:

- `DLL_CardGetCardName`
- `DLL_CardGetCardDesc`
- `DLL_CardGetType`
- `DLL_CardGetAttr`
- `DLL_CardGetLevel`
- `DLL_CardGetRank`
- `DLL_CardGetAtk`
- `DLL_CardGetDef`
- `DLL_CardGetLimitation`
- `DLL_CardGetOriginalID`
- `DLL_CardIsThisSameCard`
- `DLL_CardIsThisTunerMonster`

Duel state query examples:

- `DLL_DuelGetLP`
- `DLL_DuelWhichTurnNow`
- `DLL_DuelGetCurrentPhase`
- `DLL_DuelGetCurrentStep`
- `DLL_DuelGetCurrentDmgStep`
- `DLL_DuelGetTurnNum`
- `DLL_DuelGetCardUniqueID`
- `DLL_DuelGetCardNum`
- `DLL_DuelGetCardInHand`
- `DLL_DuelGetAttackTargetMask`
- `DLL_DuelGetThisCardEffectList`
- `DLL_DuelGetThisCardEffectFlags`
- `DLL_DuelIsThisZoneAvailable`
- `DLL_DuelIsReplayMode`

Command/legal-action examples:

- `DLL_DuelComGetCommandMask`
- `DLL_DuelComDoCommand`
- `DLL_DuelComCancelCommand`
- `DLL_DuelComGetMovablePhase`
- `DLL_DuelComMovePhase`
- `DLL_DuelCanIDoSummonMonster`
- `DLL_DuelCanIDoSpecialSummon`
- `DLL_DuelCanIDoDirectAttackByEffect`
- `DLL_DuelCanIDoSetMonster`
- `DLL_DuelCanIDoPutMonster`

Initialization/control examples:

- `DLL_DuelSysInitCustom`
- `DLL_DuelSysInitTutorial`
- `DLL_DuelSysInitRush`
- `DLL_DuelSysAct`
- `DLL_DuelSysClearWork`
- `DLL_DuelSysSetDeck2`
- `DLL_DuelSetRandomSeed`
- `DLL_DuelSetPlayerType`
- `DLL_DuelSetCpuParam`
- `DLL_DuelSetFirstPlayer`
- `DLL_DuelSetDuelLimitedType`
- `DLL_SetEffectDelegate`
- `DLL_SetUseCardDelegate`

Replay/backup examples:

- `DLL_DuelBackupDuel`
- `DLL_DuelRestoreDuel`
- `DLL_DuelGetBackupSize`
- `DLL_DuelIsReplay`
- `DLL_DuelIsReplayMode`

CPU-related examples:

- `CPU_SaveDuelInit`
- `CPU_SaveDuelTerm`
- `CPU_SaveSetPhase`
- `CPU_LoadSetPhase`
- `CPU_GetChainWorkEx`
- `CPU_RunChainForEffectEx`
- `CPU_GetSpSummonCost`
- `DLL_DuelSetCpuParam`

## Architecture Implications For Vanguard Thai Sim

### 1. Split Rules Core From UI

Our Unity client should call a rules service/module, not mutate card state
directly from buttons. This keeps desktop/mobile UI, CPU bot, replay, and future
online mode on the same rules path.

Target shape:

- `VanguardRulesCore`
- `VanguardCommandService`
- `VanguardQueryService`
- `VanguardEffectDelegateRegistry`
- `VanguardReplayService`

### 2. Legal Actions Should Be Command Masks

Master Duel exposes command mask style APIs. Our equivalent should be:

- `GetCommandMask(playerId)`
- `GetLegalActions(playerId)`
- `CanRide(...)`
- `CanCall(...)`
- `CanAttack(...)`
- `CanActivateAbility(...)`
- `ExecuteCommand(command)`
- `CancelCommand(commandId)`
- `MovePhase(nextPhase)`

The UI and bot should consume the same legal-action list.

### 3. Deterministic Seed And Replay Are Core Requirements

The presence of random seed, backup/restore, and replay-mode functions reinforces
that deterministic replay should be part of the core early, not a late feature.

For our project:

- all random choices must pass through a seeded RNG service
- every state mutation must emit a replayable event
- undo/debug should use snapshot or event replay
- bot evaluation should replay the same seed to compare behavior

### 4. Card Query API Should Be Rich

The many `CardGet*` functions suggest that the rules core queries structured card
attributes constantly. For Vanguard, card data should not be read from display
text during play.

Minimum query shape:

- `GetCardName`
- `GetCardText`
- `GetGrade`
- `GetPower`
- `GetShield`
- `GetCritical`
- `GetNation`
- `GetClan`
- `GetUnitType`
- `GetRace`
- `GetTriggerType`
- `GetKeywords`
- `GetAllowedCopies`
- `GetAbilityDefinitions`

### 5. CPU Is A Layer Above Legal Commands

The CPU-related strings point to saved duel state, phase handling, chain/effect
work, and special summon cost checks. The exact implementation is not inspected,
but the visible API shape supports our direction:

- CPU receives legal commands
- CPU has parameters/difficulty knobs
- CPU can run effect/chain/ability evaluations
- CPU should not directly mutate game state

For Vanguard:

- Easy bot: shallow scoring plus mistakes
- Normal bot: phase-aware heuristics
- Hard bot: one-to-three-ply simulation over legal commands
- Future training: replay/event logs from the same rules core

### 6. Content Cache Should Be Manifest-Based

The cache layout supports a future pack system:

- pack manifest with version, card count, image count, hashes
- content-addressed image files
- local cache repair/verify
- user save separate from card/image cache
- downloadable custom packs later

## Concrete Changes To Our Plan

Add or strengthen these milestones before smart CPU work:

1. `RulesCore` facade with command/query APIs.
2. `LegalCommandMask` and `LegalActionGenerator` as the only UI/bot entry point.
3. Seeded RNG service and deterministic event log.
4. Snapshot/restore support for undo, replay, and bot simulation.
5. Ability delegate registry for card-specific effects.
6. Content-addressed pack cache with manifest verification.

## Bottom Line

Master Duel confirms the direction already suggested by Dear Days:

Do not build the bot as a separate pile of shortcuts. Build a rules core that
answers "what can be done now?", accepts one command, mutates state through one
event/reducer path, and can replay the result deterministically. UI, CPU,
online, undo, and replay should all sit on that same core.
