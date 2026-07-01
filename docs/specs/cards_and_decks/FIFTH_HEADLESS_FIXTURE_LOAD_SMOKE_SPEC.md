# Fifth Headless Fixture Load Smoke Spec

Milestone: `M54-03`

## Purpose

`M54-03` verifies that the fifth offline runtime/test fixture can be loaded
through non-UI headless preparation without entering the saved deck library,
PlayTable UI, bot runtime, G Zone runtime, or Stride runtime.

The smoke starts from the `M54-02` count-line deck text, parses it, checks it
against the runtime fixture and SQLite pack, creates a compact `VGTH1.` deck
code artifact, and records optional Unity headless evidence.

## Inputs

- `outputs/target_slice/runtime_fixtures/m52_recipe_001_gold_paladin_m53_05.json`
- `outputs/target_slice/m54_02_fifth_fixture_deck_text_export.json`
- `outputs/target_slice/m54_02_fifth_fixture_deck_text_export.txt`
- `data/packs/vanguard_th/cards.sqlite`

Unity evidence:

- `outputs/target_slice/m54_03_fifth_fixture_unity_result.json`
- `outputs/target_slice/m54_03_fifth_fixture_unity_replay.json`

## Outputs

- `outputs/target_slice/m54_03_fifth_fixture_deck_code.txt`
- `outputs/target_slice/m54_03_fifth_fixture_load_smoke.json`
- `outputs/target_slice/m54_03_fifth_fixture_load_smoke.md`

## Checks

The smoke must verify:

- `M54-02` export is ready
- count-line deck text parses with `[Main]`, `[Ride]`, and `[G]` sections
- deck text main entries exactly match the runtime fixture
- `[G]` stays empty because the accepted boundary is main-deck-only
- all card ids exist in SQLite
- generated `VGTH1.` deck code round-trips through the Python mirror of the
  Unity codec payload shape
- Unity headless result is accepted and uses `deck_source = deck_code`

## Boundary

This milestone must not:

- add a saved deck
- mutate the UI deck library
- enable bot playbooks
- enable G Zone or Stride runtime
- mutate `GameState` from Python
- promote the fixture into live runtime deck selection

## Verification

```powershell
python tools\deck\build_fifth_headless_fixture_load_smoke.py
python tools\deck\build_fifth_headless_fixture_load_smoke.py `
  --unity-result outputs\target_slice\m54_03_fifth_fixture_unity_result.json `
  --unity-replay outputs\target_slice\m54_03_fifth_fixture_unity_replay.json
python -m unittest tests.test_fifth_headless_fixture_load_smoke
python -m unittest discover -s tests -p "test_*.py"
```

Optional Unity headless CLI evidence:

```powershell
$deckCode = (Get-Content -Raw outputs\target_slice\m54_03_fifth_fixture_deck_code.txt).Trim()
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
& $unityExe -batchmode -nographics -projectPath $projectPath `
  -executeMethod VanguardThaiSim.EditorTools.HeadlessSimulationCliRunner.RunFromCommandLine `
  -headlessSeed 5403 -headlessRuleset Premium -headlessDeckCode $deckCode `
  -headlessResultPath (Resolve-Path outputs\target_slice).Path\m54_03_fifth_fixture_unity_result.json `
  -headlessReplayPath (Resolve-Path outputs\target_slice).Path\m54_03_fifth_fixture_unity_replay.json
```

## Done Rule

`M54-03` is done when offline load smoke passes, the deck code artifact is
created, Unity headless evidence is accepted, targeted/full Python tests pass,
docs are updated, and the next target is `M54-04`.
