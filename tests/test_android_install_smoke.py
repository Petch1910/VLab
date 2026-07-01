import json
import tempfile
import unittest
from pathlib import Path

from tools.smoke.android_install_smoke import (
    CommandResult,
    active_devices,
    parse_adb_devices,
    parse_aapt_package_name,
    run_smoke,
)


class AndroidInstallSmokeTests(unittest.TestCase):
    def test_parse_adb_devices_ignores_header_and_keeps_states(self):
        devices = parse_adb_devices(
            "List of devices attached\n"
            "emulator-5554\tdevice\n"
            "offline-1\toffline\n"
            "\n"
        )

        self.assertEqual(["emulator-5554", "offline-1"], [device.serial for device in devices])
        self.assertEqual(["emulator-5554"], [device.serial for device in active_devices(devices)])

    def test_parse_aapt_package_name_reads_badging_output(self):
        package_name = parse_aapt_package_name(
            "package: name='com.DefaultCompany.VanguardThaiSim' versionCode='1' versionName='1.0'\n"
        )

        self.assertEqual("com.DefaultCompany.VanguardThaiSim", package_name)

    def test_no_device_writes_waiting_report_without_failing_by_default(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="adb",
                output_path=output,
                runner=lambda command, timeout: CommandResult(0, "List of devices attached\n", ""),
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(0, exit_code)
            self.assertEqual("waiting", report["status"])
            self.assertIn("NO_ADB_DEVICE", report["blockers"])

    def test_missing_apk_fails_without_running_adb(self):
        calls = []

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=root / "missing.apk",
                adb_path="adb",
                output_path=output,
                runner=lambda command, timeout: calls.append(command) or CommandResult(0, "", ""),
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(1, exit_code)
            self.assertEqual("failed", report["status"])
            self.assertIn("APK_MISSING", report["blockers"])
            self.assertEqual([], calls)

    def test_device_installs_apk_and_can_launch_package(self):
        calls = []

        def fake_runner(command, timeout):
            calls.append(list(command))
            if command[1:] == ["devices"]:
                return CommandResult(0, "List of devices attached\nemulator-5554\tdevice\n", "")
            return CommandResult(0, "OK", "")

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="adb",
                output_path=output,
                package_name="com.defaultcompany.vanguardthaisim",
                launch=True,
                runner=fake_runner,
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(0, exit_code)
            self.assertEqual("passed", report["status"])
            self.assertEqual("emulator-5554", report["selected_device"])
            self.assertIn(["adb", "-s", "emulator-5554", "install", "-r", str(apk)], calls)
            self.assertIn(
                [
                    "adb",
                    "-s",
                    "emulator-5554",
                    "shell",
                    "monkey",
                    "-p",
                    "com.defaultcompany.vanguardthaisim",
                    "1",
                ],
                calls,
            )

    def test_launch_can_use_detected_package_name(self):
        calls = []

        def fake_runner(command, timeout):
            calls.append(list(command))
            if "dump" in command and "badging" in command:
                return CommandResult(
                    0,
                    "package: name='com.DefaultCompany.VanguardThaiSim' versionCode='1' versionName='1.0'\n",
                    "",
                )
            if command[1:] == ["devices"]:
                return CommandResult(0, "List of devices attached\nemulator-5554\tdevice\n", "")
            return CommandResult(0, "OK", "")

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="adb",
                aapt_path="aapt",
                output_path=output,
                launch=True,
                runner=fake_runner,
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(0, exit_code)
            self.assertEqual("passed", report["status"])
            self.assertEqual("com.DefaultCompany.VanguardThaiSim", report["detected_package_name"])
            self.assertEqual("com.DefaultCompany.VanguardThaiSim", report["package_name"])
            self.assertIn(
                [
                    "adb",
                    "-s",
                    "emulator-5554",
                    "shell",
                    "monkey",
                    "-p",
                    "com.DefaultCompany.VanguardThaiSim",
                    "1",
                ],
                calls,
            )

    def test_launch_requires_package_name(self):
        def fake_runner(command, timeout):
            if command[1:] == ["devices"]:
                return CommandResult(0, "List of devices attached\nemulator-5554\tdevice\n", "")
            return CommandResult(0, "OK", "")

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="adb",
                output_path=output,
                launch=True,
                runner=fake_runner,
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(1, exit_code)
            self.assertEqual("failed", report["status"])
            self.assertIn("PACKAGE_NAME_REQUIRED_FOR_LAUNCH", report["blockers"])

    def test_auto_adb_uses_first_candidate_with_active_device(self):
        calls = []

        def fake_runner(command, timeout):
            calls.append(list(command))
            if command == ["adb-one", "devices"]:
                return CommandResult(0, "List of devices attached\n", "")
            if command == ["adb-two", "devices"]:
                return CommandResult(0, "List of devices attached\nldplayer-1\tdevice\n", "")
            return CommandResult(0, "OK", "")

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="auto",
                output_path=output,
                detect_package=False,
                adb_candidates=["adb-one", "adb-two"],
                runner=fake_runner,
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(0, exit_code)
            self.assertEqual("passed", report["status"])
            self.assertEqual("auto", report["adb_requested_path"])
            self.assertEqual("adb-two", report["adb_path"])
            self.assertEqual("adb-two", report["selected_adb_path"])
            self.assertEqual("ldplayer-1", report["selected_device"])
            self.assertIn(["adb-two", "-s", "ldplayer-1", "install", "-r", str(apk)], calls)

    def test_auto_adb_waiting_report_includes_probe_results(self):
        def fake_runner(command, timeout):
            return CommandResult(0, "List of devices attached\n", "")

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="auto",
                output_path=output,
                detect_package=False,
                adb_candidates=["adb-one", "adb-two"],
                runner=fake_runner,
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(0, exit_code)
            self.assertEqual("waiting", report["status"])
            self.assertIn("NO_ADB_DEVICE", report["blockers"])
            self.assertEqual(["adb-one", "adb-two"], [probe["adb_path"] for probe in report["adb_probe_results"]])
            self.assertEqual([], report["devices"])

    def test_install_timeout_writes_failure_report(self):
        def fake_runner(command, timeout):
            if command[1:] == ["devices"]:
                return CommandResult(0, "List of devices attached\nemulator-5554\tdevice\n", "")
            return CommandResult(-9, "", "Command timed out after 1 seconds.")

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="adb",
                output_path=output,
                detect_package=False,
                timeout=1,
                runner=fake_runner,
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(1, exit_code)
            self.assertEqual("failed", report["status"])
            self.assertIn("ADB_INSTALL_FAILED", report["blockers"])
            self.assertEqual(-9, report["install_returncode"])
            self.assertIn("timed out", report["install_stderr"])

    def test_push_pack_and_force_stop_run_before_launch(self):
        calls = []

        def fake_runner(command, timeout):
            calls.append(list(command))
            if command[1:] == ["devices"]:
                return CommandResult(0, "List of devices attached\nemulator-5554\tdevice\n", "")
            return CommandResult(0, "OK", "")

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            pack = root / "vanguard_th"
            pack.mkdir()
            (pack / "manifest.json").write_text("{}", encoding="utf-8")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="adb",
                output_path=output,
                detect_package=False,
                package_name="com.DefaultCompany.VanguardThaiSim",
                push_pack=True,
                pack_source=pack,
                force_stop_before_launch=True,
                launch=True,
                runner=fake_runner,
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            target_parent = "/sdcard/Android/data/com.DefaultCompany.VanguardThaiSim/files/data/packs"
            self.assertEqual(0, exit_code)
            self.assertEqual("passed", report["status"])
            self.assertEqual(f"{target_parent}/vanguard_th", report["pack_push_target"])

            install_call = ["adb", "-s", "emulator-5554", "install", "-r", str(apk)]
            mkdir_call = ["adb", "-s", "emulator-5554", "shell", "mkdir", "-p", target_parent]
            push_call = ["adb", "-s", "emulator-5554", "push", str(pack), target_parent]
            force_stop_call = [
                "adb",
                "-s",
                "emulator-5554",
                "shell",
                "am",
                "force-stop",
                "com.DefaultCompany.VanguardThaiSim",
            ]
            launch_call = [
                "adb",
                "-s",
                "emulator-5554",
                "shell",
                "monkey",
                "-p",
                "com.DefaultCompany.VanguardThaiSim",
                "1",
            ]
            self.assertLess(calls.index(install_call), calls.index(mkdir_call))
            self.assertLess(calls.index(mkdir_call), calls.index(push_call))
            self.assertLess(calls.index(push_call), calls.index(force_stop_call))
            self.assertLess(calls.index(force_stop_call), calls.index(launch_call))

    def test_push_pack_reports_missing_source(self):
        def fake_runner(command, timeout):
            if command[1:] == ["devices"]:
                return CommandResult(0, "List of devices attached\nemulator-5554\tdevice\n", "")
            return CommandResult(0, "OK", "")

        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            apk = root / "VanguardThaiSim.apk"
            apk.write_bytes(b"apk")
            output = root / "report.json"

            exit_code = run_smoke(
                apk_path=apk,
                adb_path="adb",
                output_path=output,
                detect_package=False,
                package_name="com.DefaultCompany.VanguardThaiSim",
                push_pack=True,
                pack_source=root / "missing_pack",
                runner=fake_runner,
            )

            report = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(1, exit_code)
            self.assertEqual("failed", report["status"])
            self.assertIn("PACK_SOURCE_MISSING", report["blockers"])


if __name__ == "__main__":
    unittest.main()
