# Third Party Components

## SQLite

Purpose:

- Runtime local card database for Unity card browser/deck/game systems

Files:

- `client/unity/VanguardThaiSim/Assets/Plugins/Managed/Mono.Data.Sqlite.dll`
- `client/unity/VanguardThaiSim/Assets/Plugins/Managed/System.Data.dll`
- `client/unity/VanguardThaiSim/Assets/Plugins/x86_64/sqlite3.dll`

Source:

- Managed assemblies are from the installed Unity Editor `6000.5.0f1`
- Native Windows x64 `sqlite3.dll` is from official SQLite precompiled binaries:
  `https://www.sqlite.org/2026/sqlite-dll-win-x64-3530200.zip`

Verification:

- Official SQLite.org SHA3-256 for `sqlite-dll-win-x64-3530200.zip`:
  `b898ced2e0627999d7d0b9d554ea53086a9b165e52ae743277d115dcd39e6868`
- Downloaded zip was verified with Python `hashlib.sha3_256` before extraction

Notes:

- Current native plugin is Windows x64 for Editor/Windows development
- Android SQLite runtime strategy still needs a separate verification pass before Android builds

