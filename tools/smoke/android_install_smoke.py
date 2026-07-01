"""Android APK install smoke helper for VanguardThaiSim.

This tool is intentionally outside Unity so it can report "waiting" when no
ADB device is attached without opening the editor.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Callable, Iterable, List, Optional, Sequence


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_APK = ROOT / "client" / "unity" / "VanguardThaiSim" / "build" / "android" / "latest" / "VanguardThaiSim.apk"
DEFAULT_OUTPUT = ROOT / "client" / "unity" / "VanguardThaiSim" / "work" / "android_install_smoke_report.json"
DEFAULT_PACK_SOURCE = ROOT / "data" / "packs" / "vanguard_th"
DEFAULT_UNITY_AAPT = (
    Path(os.environ.get("LOCALAPPDATA", ""))
    / "Unity"
    / "Hub"
    / "Editor"
    / "6000.5.0f1"
    / "Editor"
    / "Data"
    / "PlaybackEngines"
    / "AndroidPlayer"
    / "SDK"
    / "build-tools"
    / "36.0.0"
    / "aapt.exe"
)
DEFAULT_LDPLAYER_ADB = Path("C:/LDPlayer/LDPlayer9/adb.exe")


@dataclass(frozen=True)
class AdbDevice:
    serial: str
    state: str


@dataclass(frozen=True)
class CommandResult:
    returncode: int
    stdout: str = ""
    stderr: str = ""


def parse_adb_devices(output: str) -> List[AdbDevice]:
    devices: List[AdbDevice] = []
    for raw_line in (output or "").splitlines():
        line = raw_line.strip()
        if not line or line.lower().startswith("list of devices"):
            continue

        parts = line.split()
        if len(parts) < 2:
            continue

        devices.append(AdbDevice(serial=parts[0], state=parts[1]))
    return devices


def active_devices(devices: Iterable[AdbDevice]) -> List[AdbDevice]:
    return [device for device in devices if device.state == "device"]


def parse_aapt_package_name(output: str) -> str:
    match = re.search(r"package:\s+name='([^']+)'", output or "")
    return match.group(1) if match else ""


def find_default_aapt() -> str:
    candidates: List[Path] = []
    for env_name in ("ANDROID_HOME", "ANDROID_SDK_ROOT"):
        sdk_root = os.environ.get(env_name)
        if sdk_root:
            candidates.extend(sorted(Path(sdk_root).glob("build-tools/*/aapt.exe"), reverse=True))

    candidates.append(DEFAULT_UNITY_AAPT)
    for candidate in candidates:
        if candidate.exists():
            return str(candidate)

    return ""


def find_default_adb_candidates() -> List[str]:
    candidates: List[str] = []
    path_adb = shutil.which("adb")
    if path_adb:
        candidates.append(path_adb)

    for env_name in ("ANDROID_HOME", "ANDROID_SDK_ROOT"):
        sdk_root = os.environ.get(env_name)
        if sdk_root:
            candidates.append(str(Path(sdk_root) / "platform-tools" / "adb.exe"))

    if DEFAULT_LDPLAYER_ADB.exists():
        candidates.append(str(DEFAULT_LDPLAYER_ADB))

    return unique_non_empty(candidates) or ["adb"]


def unique_non_empty(values: Iterable[str]) -> List[str]:
    result: List[str] = []
    seen = set()
    for value in values:
        safe_value = str(value or "").strip()
        if not safe_value:
            continue

        key = safe_value.lower()
        if key in seen:
            continue

        seen.add(key)
        result.append(safe_value)
    return result


def truncate(value: str, limit: int = 4000) -> str:
    value = value or ""
    if len(value) <= limit:
        return value
    return value[: limit - 3] + "..."


def run_subprocess(command: Sequence[str], timeout: int) -> CommandResult:
    try:
        completed = subprocess.run(
            list(command),
            text=True,
            capture_output=True,
            timeout=timeout,
            check=False,
        )
    except subprocess.TimeoutExpired as exception:
        stdout = exception.stdout or ""
        stderr = exception.stderr or ""
        if isinstance(stdout, bytes):
            stdout = stdout.decode(errors="replace")
        if isinstance(stderr, bytes):
            stderr = stderr.decode(errors="replace")
        return CommandResult(
            returncode=-9,
            stdout=stdout,
            stderr=(stderr + ("\n" if stderr else "") + f"Command timed out after {timeout} seconds."),
        )

    return CommandResult(
        returncode=completed.returncode,
        stdout=completed.stdout or "",
        stderr=completed.stderr or "",
    )


def run_smoke(
    apk_path: Path,
    adb_path: str,
    output_path: Path,
    package_name: Optional[str] = None,
    aapt_path: Optional[str] = None,
    detect_package: bool = True,
    launch: bool = False,
    device_serial: Optional[str] = None,
    fail_on_wait: bool = False,
    timeout: int = 120,
    adb_candidates: Optional[Sequence[str]] = None,
    push_pack: bool = False,
    pack_source: Optional[Path] = None,
    force_stop_before_launch: bool = False,
    runner: Callable[[Sequence[str], int], CommandResult] = run_subprocess,
) -> int:
    report = {
        "schema_version": 1,
        "created_at_utc": datetime.now(timezone.utc).isoformat(),
        "apk_path": str(apk_path),
        "adb_path": adb_path,
        "adb_requested_path": adb_path,
        "selected_adb_path": "",
        "adb_probe_results": [],
        "aapt_path": aapt_path or "",
        "package_name": package_name or "",
        "detected_package_name": "",
        "selected_device": "",
        "push_pack": push_pack,
        "pack_source_path": str(pack_source or DEFAULT_PACK_SOURCE),
        "pack_push_target": "",
        "force_stop_before_launch": force_stop_before_launch,
        "status": "unknown",
        "steps": [],
        "blockers": [],
        "devices": [],
    }

    if not apk_path.exists():
        report["status"] = "failed"
        report["blockers"].append("APK_MISSING")
        report["steps"].append("APK file was not found.")
        write_report(output_path, report)
        return 1

    report["steps"].append(f"APK found: {apk_path}")

    effective_package_name = package_name or ""
    if detect_package:
        effective_aapt = aapt_path or find_default_aapt()
        report["aapt_path"] = effective_aapt
        if effective_aapt:
            aapt_result = runner([effective_aapt, "dump", "badging", str(apk_path)], timeout)
            report["aapt_returncode"] = aapt_result.returncode
            report["aapt_stdout"] = truncate(aapt_result.stdout)
            report["aapt_stderr"] = truncate(aapt_result.stderr)
            if aapt_result.returncode == 0:
                detected = parse_aapt_package_name(aapt_result.stdout)
                report["detected_package_name"] = detected
                if detected:
                    report["steps"].append(f"Detected package id: {detected}")
                    if not effective_package_name:
                        effective_package_name = detected
                        report["package_name"] = detected
                else:
                    report["steps"].append("AAPT completed but package id was not found.")
            else:
                report["steps"].append("AAPT package detection failed.")
        else:
            report["steps"].append("AAPT package detection skipped: aapt.exe was not found.")

    candidate_paths = (
        unique_non_empty(adb_candidates or find_default_adb_candidates())
        if adb_path.strip().lower() == "auto"
        else [adb_path]
    )

    selected_adb_path = ""
    selected_devices_result: Optional[CommandResult] = None
    selected_devices: List[AdbDevice] = []
    selected_usable_devices: List[AdbDevice] = []
    successful_probe_count = 0
    combined_devices: List[dict] = []
    for candidate_path in candidate_paths:
        devices_result = runner([candidate_path, "devices"], timeout)
        parsed_devices = parse_adb_devices(devices_result.stdout)
        usable_devices = active_devices(parsed_devices)
        if device_serial:
            usable_devices = [device for device in usable_devices if device.serial == device_serial]

        probe = {
            "adb_path": candidate_path,
            "returncode": devices_result.returncode,
            "stdout": truncate(devices_result.stdout),
            "stderr": truncate(devices_result.stderr),
            "devices": [{"serial": device.serial, "state": device.state} for device in parsed_devices],
        }
        report["adb_probe_results"].append(probe)

        if devices_result.returncode == 0:
            successful_probe_count += 1
            for device in parsed_devices:
                combined_devices.append(
                    {"adb_path": candidate_path, "serial": device.serial, "state": device.state}
                )

        if devices_result.returncode == 0 and usable_devices and not selected_adb_path:
            selected_adb_path = candidate_path
            selected_devices_result = devices_result
            selected_devices = parsed_devices
            selected_usable_devices = usable_devices
            break

        if devices_result.returncode == 0 and selected_devices_result is None:
            selected_devices_result = devices_result
            selected_devices = parsed_devices

    if selected_devices_result is not None:
        report["adb_devices_returncode"] = selected_devices_result.returncode
        report["adb_devices_stdout"] = truncate(selected_devices_result.stdout)
        report["adb_devices_stderr"] = truncate(selected_devices_result.stderr)
    else:
        report["adb_devices_returncode"] = -1
        report["adb_devices_stdout"] = ""
        report["adb_devices_stderr"] = ""

    if successful_probe_count == 0:
        report["status"] = "failed"
        report["blockers"].append("ADB_DEVICES_FAILED")
        report["steps"].append("adb devices failed.")
        write_report(output_path, report)
        return 1

    report["devices"] = (
        combined_devices
        if adb_path.strip().lower() == "auto"
        else [{"serial": device.serial, "state": device.state} for device in selected_devices]
    )

    if not selected_usable_devices:
        report["status"] = "waiting"
        report["blockers"].append("NO_ADB_DEVICE")
        report["steps"].append("No ADB device/emulator in 'device' state.")
        write_report(output_path, report)
        return 1 if fail_on_wait else 0

    selected = selected_usable_devices[0]
    report["adb_path"] = selected_adb_path
    report["selected_adb_path"] = selected_adb_path
    report["selected_device"] = selected.serial
    report["steps"].append(f"ADB path selected: {selected_adb_path}")
    report["steps"].append(f"ADB device selected: {selected.serial}")

    install_command = [selected_adb_path, "-s", selected.serial, "install", "-r", str(apk_path)]
    install_result = runner(install_command, timeout)
    report["install_returncode"] = install_result.returncode
    report["install_stdout"] = truncate(install_result.stdout)
    report["install_stderr"] = truncate(install_result.stderr)
    if install_result.returncode != 0:
        report["status"] = "failed"
        report["blockers"].append("ADB_INSTALL_FAILED")
        report["steps"].append("adb install -r failed.")
        write_report(output_path, report)
        return 1

    report["steps"].append("APK installed with adb install -r.")

    if push_pack:
        if not effective_package_name:
            report["status"] = "failed"
            report["blockers"].append("PACKAGE_NAME_REQUIRED_FOR_PACK_PUSH")
            report["steps"].append("Pack push requested but no package name was supplied.")
            write_report(output_path, report)
            return 1

        effective_pack_source = pack_source or DEFAULT_PACK_SOURCE
        report["pack_source_path"] = str(effective_pack_source)
        if not effective_pack_source.exists():
            report["status"] = "failed"
            report["blockers"].append("PACK_SOURCE_MISSING")
            report["steps"].append("Pack push requested but pack source was not found.")
            write_report(output_path, report)
            return 1

        pack_target_parent = f"/sdcard/Android/data/{effective_package_name}/files/data/packs"
        report["pack_push_target"] = f"{pack_target_parent}/{effective_pack_source.name}"
        mkdir_result = runner(
            [selected_adb_path, "-s", selected.serial, "shell", "mkdir", "-p", pack_target_parent],
            timeout,
        )
        report["pack_mkdir_returncode"] = mkdir_result.returncode
        report["pack_mkdir_stdout"] = truncate(mkdir_result.stdout)
        report["pack_mkdir_stderr"] = truncate(mkdir_result.stderr)
        if mkdir_result.returncode != 0:
            report["status"] = "failed"
            report["blockers"].append("ADB_PACK_MKDIR_FAILED")
            report["steps"].append("adb shell mkdir for runtime pack failed.")
            write_report(output_path, report)
            return 1

        push_result = runner(
            [selected_adb_path, "-s", selected.serial, "push", str(effective_pack_source), pack_target_parent],
            timeout,
        )
        report["pack_push_returncode"] = push_result.returncode
        report["pack_push_stdout"] = truncate(push_result.stdout)
        report["pack_push_stderr"] = truncate(push_result.stderr)
        if push_result.returncode != 0:
            report["status"] = "failed"
            report["blockers"].append("ADB_PACK_PUSH_FAILED")
            report["steps"].append("adb push runtime pack failed.")
            write_report(output_path, report)
            return 1

        report["steps"].append("Runtime card pack pushed to app external files directory.")

    if launch:
        if not effective_package_name:
            report["status"] = "failed"
            report["blockers"].append("PACKAGE_NAME_REQUIRED_FOR_LAUNCH")
            report["steps"].append("Launch requested but no package name was supplied.")
            write_report(output_path, report)
            return 1

        if force_stop_before_launch:
            force_stop_result = runner(
                [selected_adb_path, "-s", selected.serial, "shell", "am", "force-stop", effective_package_name],
                timeout,
            )
            report["force_stop_returncode"] = force_stop_result.returncode
            report["force_stop_stdout"] = truncate(force_stop_result.stdout)
            report["force_stop_stderr"] = truncate(force_stop_result.stderr)
            if force_stop_result.returncode != 0:
                report["status"] = "failed"
                report["blockers"].append("ADB_FORCE_STOP_FAILED")
                report["steps"].append("adb shell am force-stop failed.")
                write_report(output_path, report)
                return 1

            report["steps"].append("Package force-stopped before launch.")

        launch_command = [
            selected_adb_path,
            "-s",
            selected.serial,
            "shell",
            "monkey",
            "-p",
            effective_package_name,
            "1",
        ]
        launch_result = runner(launch_command, timeout)
        report["launch_returncode"] = launch_result.returncode
        report["launch_stdout"] = truncate(launch_result.stdout)
        report["launch_stderr"] = truncate(launch_result.stderr)
        if launch_result.returncode != 0:
            report["status"] = "failed"
            report["blockers"].append("ADB_LAUNCH_FAILED")
            report["steps"].append("adb shell monkey launch failed.")
            write_report(output_path, report)
            return 1

        report["steps"].append("Package launch command completed.")

    report["status"] = "passed"
    write_report(output_path, report)
    return 0


def write_report(output_path: Path, report: dict) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(report, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Install and optionally launch the Android APK through ADB.")
    parser.add_argument("--apk", default=str(DEFAULT_APK), help="Path to VanguardThaiSim.apk.")
    parser.add_argument("--adb", default="adb", help="ADB executable path, command name, or 'auto' to probe common ADB paths.")
    parser.add_argument("--output", default=str(DEFAULT_OUTPUT), help="JSON report output path.")
    parser.add_argument("--package", default="", help="Optional Android package name for launch smoke.")
    parser.add_argument("--aapt", default="", help="Optional aapt.exe path for APK package-id detection.")
    parser.add_argument("--no-detect-package", action="store_true", help="Skip aapt package-id detection.")
    parser.add_argument("--launch", action="store_true", help="Run adb shell monkey after install. Requires --package.")
    parser.add_argument("--device", default="", help="Optional ADB serial to require.")
    parser.add_argument("--fail-on-wait", action="store_true", help="Return exit code 1 when no ADB device is attached.")
    parser.add_argument("--timeout", type=int, default=120, help="Per-command timeout in seconds.")
    parser.add_argument("--push-pack", action="store_true", help="Push the runtime card pack into app external files after install.")
    parser.add_argument("--pack-source", default=str(DEFAULT_PACK_SOURCE), help="Runtime pack directory to push with --push-pack.")
    parser.add_argument("--force-stop-before-launch", action="store_true", help="Force-stop the package before launch.")
    return parser


def main(argv: Optional[Sequence[str]] = None) -> int:
    args = build_arg_parser().parse_args(argv)
    return run_smoke(
        apk_path=Path(args.apk),
        adb_path=args.adb,
        output_path=Path(args.output),
        package_name=args.package or None,
        aapt_path=args.aapt or None,
        detect_package=not args.no_detect_package,
        launch=args.launch,
        device_serial=args.device or None,
        fail_on_wait=args.fail_on_wait,
        timeout=max(1, args.timeout),
        push_pack=args.push_pack,
        pack_source=Path(args.pack_source),
        force_stop_before_launch=args.force_stop_before_launch,
    )


if __name__ == "__main__":
    sys.exit(main())
