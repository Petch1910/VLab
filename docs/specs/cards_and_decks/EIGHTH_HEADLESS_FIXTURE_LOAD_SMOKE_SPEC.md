# Eighth Headless Fixture Load Smoke Spec

Milestone: `M66-03`

## Purpose

`M66-03` verifies that the eighth offline runtime/test fixture can be prepared
for non-UI headless loading without entering saved decks, PlayTable UI, bot
runtime, Lock/Unlock runtime, Legion/Mate runtime, or direct `GameState`
mutation.

The smoke starts from the `M66-02` count-line deck text, parses it, checks it
against the runtime fixture and SQLite pack, creates a compact `VGTH1.` deck
code artifact, and records optional Unity headless evidence.

The implementation is scaffold-safe: tests may use an in-memory fixture,
M66-01 validation report, and M66-02 deck text export. Real CLI artifacts
remain gated until the M65-06, M66-01, and M66-02 real files exist.

## Inputs

- `outputs/target_slice/runtime_fixtures/m64_recipe_001_kagero_m65_06.json`
- `outputs/target_slice/m66_02_eighth_fixture_deck_text_export.json`
- `outputs/target_slice/m66_02_eighth_fixture_deck_text_export.txt`
- `data/packs/vanguard_th/cards.sqlite`

Unity evidence:

- `outputs/target_slice/m66_03_eighth_fixture_unity_result.json`
- `outputs/target_slice/m66_03_eighth_fixture_unity_replay.json`

## Outputs

- `outputs/target_slice/m66_03_eighth_fixture_deck_code.txt`
- `outputs/target_slice/m66_03_eighth_fixture_load_smoke.json`
- `outputs/target_slice/m66_03_eighth_fixture_load_smoke.md`

These outputs are written only when the CLI is run against existing upstream
artifacts.

## Checks

The smoke must verify:

- `M66-02` export is ready
- count-line deck text parses with `[Main]`, `[Ride]`, and `[G]` sections
- deck text main entries exactly match the runtime fixture
- `[G]` stays empty because no extra deck/G Zone runtime is enabled for this
  fixture
- all card ids exist in SQLite
- generated `VGTH1.` deck code round-trips through the Python mirror of the
  Unity codec payload shape
- Unity headless result is accepted and uses `deck_source = deck_code` when
  Unity evidence is supplied

## Boundary

This milestone must not:

- add a saved deck
- mutate the UI deck library
- enable bot playbooks
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- mutate `GameState` from Python
- promote the fixture into live runtime deck selection

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_eighth_headless_fixture_load_smoke
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M65-06, M66-01, and M66-02 outputs exist:

```powershell
python tools\deck\build_eighth_headless_fixture_load_smoke.py
python tools\deck\build_eighth_headless_fixture_load_smoke.py `
  --unity-result outputs\target_slice\m66_03_eighth_fixture_unity_result.json `
  --unity-replay outputs\target_slice\m66_03_eighth_fixture_unity_replay.json
```

Optional Unity headless CLI evidence:

```powershell
$deckCode = (Get-Content -Raw outputs\target_slice\m66_03_eighth_fixture_deck_code.txt).Trim()
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
& $unityExe -batchmode -nographics -projectPath $projectPath `
  -executeMethod VanguardThaiSim.EditorTools.HeadlessSimulationCliRunner.RunFromCommandLine `
  -headlessSeed 6603 -headlessRuleset Premium -headlessDeckCode $deckCode `
  -headlessResultPath (Resolve-Path outputs\target_slice).Path\m66_03_eighth_fixture_unity_result.json `
  -headlessReplayPath (Resolve-Path outputs\target_slice).Path\m66_03_eighth_fixture_unity_replay.json
```

## Done Rule

`M66-03` scaffold work is ready when offline load smoke passes, deck code can
be created from in-memory deck text, tests cover valid and invalid gates, docs
are updated, and real CLI/Unity artifacts remain gated until upstream files
exist.
