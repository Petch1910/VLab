# Headless Fixture Load Smoke Spec

Milestone: `M39-03`

## Purpose

`M39-03` verifies that the first accepted offline runtime fixture can be loaded
through non-UI headless preparation without entering the saved deck library,
PlayTable UI, or bot runtime.

The smoke starts from the `M39-02` count-line deck text, parses it, checks it
against the runtime fixture and SQLite pack, then creates a compact `VGTH1.`
deck code artifact compatible with the existing Unity `DeckCodeCodec`.

## Inputs

- `outputs/target_slice/runtime_fixtures/recipe_003_classic_core_nova_grappler_m38_04.json`
- `outputs/target_slice/m39_02_fixture_deck_text_export.json`
- `outputs/target_slice/m39_02_fixture_deck_text_export.txt`
- `data/packs/vanguard_th/cards.sqlite`

Optional Unity evidence:

- `outputs/target_slice/m39_03_headless_fixture_unity_result.json`
- `outputs/target_slice/m39_03_headless_fixture_unity_replay.json`

## Outputs

- `outputs/target_slice/m39_03_headless_fixture_deck_code.txt`
- `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
- `outputs/target_slice/m39_03_headless_fixture_load_smoke.md`

## Checks

The smoke must verify:

- `M39-02` export is ready
- count-line deck text parses with `[Main]`, `[Ride]`, and `[G]` sections
- deck text main entries exactly match the runtime fixture
- all card ids exist in SQLite
- generated `VGTH1.` deck code round-trips through the Python mirror of the
  Unity codec payload shape
- optional Unity headless result, when provided, is accepted and uses
  `deck_source = deck_code`

## Runtime Boundary

This milestone must not:

- add a saved deck
- mutate the UI deck library
- enable bot playbooks
- mutate `GameState` from Python
- promote the fixture into live runtime deck selection

## Verification

```powershell
python tools\deck\build_headless_fixture_load_smoke.py
python -m unittest tests.test_headless_fixture_load_smoke
python -m unittest discover -s tests -p "test_*.py"
```

Optional Unity headless CLI evidence:

```powershell
$deckCode = (Get-Content -Raw outputs\target_slice\m39_03_headless_fixture_deck_code.txt).Trim()
$projectPath = (Resolve-Path "client\unity\VanguardThaiSim").Path
$unityExe = "$env:LOCALAPPDATA\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
& $unityExe -batchmode -nographics -projectPath $projectPath `
  -executeMethod VanguardThaiSim.EditorTools.HeadlessSimulationCliRunner.RunFromCommandLine `
  -headlessSeed 3903 -headlessRuleset Premium -headlessDeckCode $deckCode `
  -headlessResultPath (Resolve-Path outputs\target_slice).Path\m39_03_headless_fixture_unity_result.json `
  -headlessReplayPath (Resolve-Path outputs\target_slice).Path\m39_03_headless_fixture_unity_replay.json
```

After Unity evidence exists, rerun:

```powershell
python tools\deck\build_headless_fixture_load_smoke.py `
  --unity-result outputs\target_slice\m39_03_headless_fixture_unity_result.json `
  --unity-replay outputs\target_slice\m39_03_headless_fixture_unity_replay.json
```

## Done Rule

`M39-03` is done when offline load smoke passes, the deck code artifact is
created, targeted/full Python tests pass, and Unity headless evidence is either
accepted or explicitly recorded as not run with a blocker.
