# Unity Setup

เอกสารนี้ใช้ปลดล็อก `M0-02` และ `M0-03`

## สถานะเครื่องล่าสุด

- พบ `winget`
- พบ `choco`
- Unity Hub ติดตั้งแล้ว
- Unity CLI ติดตั้งแล้ว
- Unity Editor `6000.5.0f1` ติดตั้งแล้ว
- Android Build Support, Android SDK/NDK Tools, OpenJDK ติดตั้งแล้ว

## ติดตั้ง Unity Hub

ใช้ `winget`:

```powershell
winget install --id Unity.UnityHub --source winget --accept-package-agreements --accept-source-agreements
```

หรือใช้ Chocolatey:

```powershell
choco install unity-hub -y
```

## Unity Editor ที่ต้องติดตั้ง

ติดตั้ง Unity LTS ผ่าน Unity Hub พร้อม modules:

- Windows Build Support
- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

iOS ค่อยเพิ่มทีหลังเมื่อมีเครื่อง build macOS

## Project Path

Unity project อยู่ที่:

```text
client/unity/VanguardThaiSim/
```

สร้างด้วย template `com.unity.template.2d`

## หลังติดตั้ง Unity

ให้ทำลำดับนี้:

1. เปิด project ที่ `client/unity/VanguardThaiSim/`
2. ตรวจว่า `Assets/Scripts/Vanguard/Cards/` ยังอยู่
3. เพิ่ม SQLite runtime/package สำหรับ Unity
4. สร้าง scene แรก
5. ทำ test dummy ให้ผ่าน
6. เริ่ม implement `M2-01` ตาม `docs/UNITY_DATA_ACCESS_CONTRACT.md`

## Verification

ตรวจว่า Unity CLI ใช้ได้:

```powershell
Get-ChildItem "C:\Program Files\Unity\Hub\Editor" -Recurse -Filter Unity.exe
```

batchmode import/compile ผ่านแล้วจาก log:

```text
work/unity_import.log
```
