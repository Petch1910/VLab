# M12 Structured Card Ability Data Closeout

## Status

Closed in `M12-12`.

## Completed Scope

M12 established the first correctness-first structured ability pipeline:

- `M12-01` Ability schema v1
- `M12-02` Python/Pydantic-aware validator with built-in fallback
- `M12-03` Unity runtime ability registry
- `M12-04` Structured cost template
- `M12-05` Structured target template
- `M12-06` Structured draw/move effect template
- `M12-07` Structured resource ops for CounterBlast/CounterCharge
- `M12-08` Structured modifier effects for PowerPlus/CriticalPlus
- `M12-09` Structured ability fixture DSL
- `M12-10` First 20-entry template smoke ability pack
- `M12-11` Manual fallback bridge to pending AUTO manual resolution artifacts

## Supported Automated Template Set

Supported now:

- costs: `none`, `counter_blast`, `soul_blast`, `energy_blast`,
  `once_per_turn`, `once_per_fight`
- targets: safe visible `self`, `unit`, and `card` candidates through public or
  owner-visible zones
- effects: `draw`, `move_zone`, `counter_blast`, `counter_charge`,
  `power_plus`, `critical_plus`
- manual bridge: unsupported structured paths can create pending AUTO manual
  `Resolve` artifacts

Manual placeholders remain for:

- `discard`
- `soul_charge`
- `soul_blast` live execution
- `circle`
- `Deck`, `Soul`, `GZone`, and unsafe private-zone target shapes
- arbitrary card text

## Important Boundary

The M12 pack at
`data/packs/vanguard_th/abilities/structured_ability_pack_m12_10.json` is
template smoke data only. It is not a real transcription of official card
effects.

## Verification

Closeout regression:

```powershell
python tools\verification\verify_vanguard_th_pack.py
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack
python tools\data\import_custom_pack.py data\templates\custom_pack --output-dir work\custom_pack_import --overwrite
python tools\data\validate_ability_schema.py data\packs\vanguard_th\abilities\structured_ability_pack_m12_10.json
python -m unittest discover -s tests -p "test_*.py"
```

Results:

- Vanguard TH pack verification: `10836/10836` cards, `10836/10836` images, all OK
- Custom pack schema validation: all OK with the known fallback-image warnings
- Custom pack import: succeeded
- M12 structured ability pack validation: 20 abilities, all OK
- Python tests: `24/24`
- Unity compile: passed after M12-11
- Unity EditMode: `657/657`

## Next Target

Move to `M13-01` Owner-private room initialization.

Keep owner-private commitment-room gameplay blocked until
`OWNER_PRIVATE_ROOM_INITIALIZATION_SPEC.md` is implemented with explicit
client-trust UX.
