# Vanguard Dear Days 2 Archive Notes

Date: 2026-06-19

Scope: non-extracting archive inventory of the folder:
`F:\New folder (4)`.

This note is for architecture awareness only. Do not use these files as a
source for assets, code, card data, or proprietary game resources.

## Safe Inspection Boundary

- Allowed: list archive contents, file names, sizes, high-level package type.
- Not allowed: decrypting Switch content, converting `.xci`/`.nsp`/`.nsz`,
  unpacking game files, extracting assets, bypassing platform protection, or
  copying proprietary data into this project.

## Observed Archive Set

The folder contains seven RAR archives:

- Base game as a multipart XCI archive:
  - `Cardfight!! Vanguard Dear Days 2 (XCI).part1.rar`
  - `Cardfight!! Vanguard Dear Days 2 (XCI).part2.rar`
  - `Cardfight!! Vanguard Dear Days 2 (XCI).part3.rar`
- Update archives:
  - `Cardfight!! Vanguard Dear Days 2 (NSP)(Update 1.2.1) (1).rar`
  - `Cardfight Vanguard Dear Days 2 (NSP)(Update 1.3.1).rar`
- DLC archives:
  - `Cardfight Vanguard Dear Days 2 (NSP)(25 DLCs).rar`
  - `Cardfight Vanguard Dear Days 2 (NSP)(25 DLCs) (1).rar`

All RAR files use a RAR5 header.

## Non-Extracting Listing Result

Using `UnRAR l` only:

- The XCI multipart archive lists one base `.xci` file around 15.1 GB.
- The 1.3.1 update archive lists one `.nsp` update file around 5.58 GB.
- The DLC archive lists 25 small `.nsz` DLC unlocker files plus one folder.

No files were extracted.

## Implication For Our Project

This archive set is not useful as a clean data source for our simulator because
opening the Switch package layer would require platform-specific extraction or
decryption workflows that are outside the allowed scope.

Use official/public references, our KK Card Fight export, and manually authored
ability schemas instead.

For architecture inspiration, prefer the already inspected Steam installation of
Dear Days 1, which exposed high-level action and ability vocabulary without
unpacking protected game content. See `docs/VGDD_RESEARCH_NOTES.md`.
