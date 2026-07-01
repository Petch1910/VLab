# Vanguard TH Runtime Pack

Runtime pack นี้สร้างจากข้อมูล KK Card Fight Vanguard TH ที่ export แล้ว

## Files

- `manifest.json` metadata ของ pack
- `cards.sqlite` ฐานข้อมูล runtime สำหรับ Unity/client
- `verification_report.json` ผลตรวจ pack ล่าสุด

## Current Counts

- Cards: `10,836`
- Images referenced: `10,836`
- Existing images: `10,836`
- Series: `206`
- Clans: `33`

## Source

- Cards JSON: `outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json`
- Image root: `outputs/kk_cardfight_export/data/images`

## Build

```powershell
python tools\data\build_vanguard_th_pack.py
python tools\data\build_vanguard_th_asset_index.py
python tools\verification\verify_vanguard_th_pack.py
python tools\data\query_vanguard_th_pack.py summary
python tools\data\query_vanguard_th_pack.py card BT01-001TH
```

Images are not copied into this folder. The SQLite database stores paths to the verified source image folder.
