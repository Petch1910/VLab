using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class WindowsOnlineRoomUsabilityCloseoutReport
    {
        public int schema_version = 1;
        public string milestone = "M25-08";
        public string suite_status = "closed";
        public string player_outcome = "friend_room_on_windows_is_easier_to_use";
        public string next_target = "M26-01";
        public List<WindowsOnlineRoomCloseoutTask> completed_tasks =
            new List<WindowsOnlineRoomCloseoutTask>();
        public List<string> preserved_guardrails = new List<string>();
        public List<string> verification_artifacts = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static WindowsOnlineRoomUsabilityCloseoutReport FromJson(string json)
        {
            WindowsOnlineRoomUsabilityCloseoutReport report =
                JsonUtility.FromJson<WindowsOnlineRoomUsabilityCloseoutReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Windows online room closeout report JSON could not be parsed.",
                    nameof(json));
            }

            report.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (completed_tasks == null)
            {
                completed_tasks = new List<WindowsOnlineRoomCloseoutTask>();
            }

            if (preserved_guardrails == null)
            {
                preserved_guardrails = new List<string>();
            }

            if (verification_artifacts == null)
            {
                verification_artifacts = new List<string>();
            }
        }
    }

    [Serializable]
    public sealed class WindowsOnlineRoomCloseoutTask
    {
        public string task_id;
        public string status;
        public string summary;
        public string evidence;
    }

    [Serializable]
    public sealed class WindowsOnlineRoomUsabilityCloseoutValidationResult
    {
        public bool accepted;
        public List<string> errors = new List<string>();
    }

    public static class WindowsOnlineRoomUsabilityCloseoutReportBuilder
    {
        private static readonly string[] RequiredTasks =
        {
            "M25-01",
            "M25-02",
            "M25-03",
            "M25-04",
            "M25-05",
            "M25-06",
            "M25-07"
        };

        private static readonly string[] RequiredGuardrails =
        {
            "photon_trusted_client_kept",
            "no_transport_switch_without_adr",
            "hidden_state_masking_preserved",
            "default_online_ui_hides_payload_debug",
            "windows_only_verification",
            "comparator_reference_only"
        };

        public static WindowsOnlineRoomUsabilityCloseoutReport CreateCurrent()
        {
            WindowsOnlineRoomUsabilityCloseoutReport report =
                new WindowsOnlineRoomUsabilityCloseoutReport();
            report.completed_tasks.Add(Task(
                "M25-01",
                "Photon trusted-client room policy kept for casual friend-room play.",
                "docs/history/M25_01_PHOTON_TRUSTED_CLIENT_ROOM_POLICY_CLOSEOUT.md"));
            report.completed_tasks.Add(Task(
                "M25-02",
                "Lobby flow covers create, join, ready, start, rematch, and return-home UX.",
                "docs/history/M25_02_LOBBY_FLOW_CLOSEOUT.md"));
            report.completed_tasks.Add(Task(
                "M25-03",
                "Room status shows connection, player count, deck hash readiness, pack hash, and public cursor.",
                "docs/history/M25_03_ROOM_STATUS_CLOSEOUT.md"));
            report.completed_tasks.Add(Task(
                "M25-04",
                "Reconnect UX explains request, batch, room mismatch, and cursor-gap failures.",
                "docs/history/M25_04_RECONNECT_UX_CLOSEOUT.md"));
            report.completed_tasks.Add(Task(
                "M25-05",
                "Online PlayTable default UI keeps payload and debug detail in Advanced only.",
                "docs/history/M25_05_ONLINE_PLAYTABLE_DEFAULT_UI_CLOSEOUT.md"));
            report.completed_tasks.Add(Task(
                "M25-06",
                "Replay sync/status shows online cursor, public replay event count, and reconnect results.",
                "docs/history/M25_06_REPLAY_SYNC_STATUS_CLOSEOUT.md"));
            report.completed_tasks.Add(Task(
                "M25-07",
                "Online room test rollup tracks privacy, stale-cursor, reconnect, and public event coverage.",
                "docs/history/M25_07_ONLINE_ROOM_TEST_ROLLUP_CLOSEOUT.md"));

            report.preserved_guardrails.AddRange(RequiredGuardrails);
            report.verification_artifacts.Add(
                "client/unity/VanguardThaiSim/work/unity_compile_m25_08_online_room_closeout.log");
            report.verification_artifacts.Add(
                "client/unity/VanguardThaiSim/work/unity_editmode_m25_08_online_room_closeout.xml");
            report.verification_artifacts.Add(
                "client/unity/VanguardThaiSim/work/windows_build_m25_08_online_room_closeout.log");
            report.verification_artifacts.Add(
                "client/unity/VanguardThaiSim/work/player_smoke_m25_08_online_room_closeout.json");
            return report;
        }

        public static WindowsOnlineRoomUsabilityCloseoutValidationResult Validate(
            WindowsOnlineRoomUsabilityCloseoutReport report)
        {
            WindowsOnlineRoomUsabilityCloseoutValidationResult result =
                new WindowsOnlineRoomUsabilityCloseoutValidationResult();
            if (report == null)
            {
                result.errors.Add("report_missing");
                return result;
            }

            report.EnsureLists();
            if (report.schema_version != 1)
            {
                result.errors.Add("schema_version_must_be_1");
            }

            if (report.milestone != "M25-08")
            {
                result.errors.Add("milestone_must_be_M25-08");
            }

            if (report.next_target != "M26-01")
            {
                result.errors.Add("next_target_must_be_M26-01");
            }

            foreach (string requiredTask in RequiredTasks)
            {
                WindowsOnlineRoomCloseoutTask task = FindTask(report, requiredTask);
                if (task == null)
                {
                    result.errors.Add("missing_task_" + requiredTask);
                    continue;
                }

                if (task.status != "done")
                {
                    result.errors.Add("task_not_done_" + requiredTask);
                }
            }

            foreach (string requiredGuardrail in RequiredGuardrails)
            {
                if (!report.preserved_guardrails.Contains(requiredGuardrail))
                {
                    result.errors.Add("missing_guardrail_" + requiredGuardrail);
                }
            }

            if (report.verification_artifacts.Count == 0)
            {
                result.errors.Add("verification_artifacts_missing");
            }

            result.accepted = result.errors.Count == 0;
            return result;
        }

        private static WindowsOnlineRoomCloseoutTask Task(string taskId, string summary, string evidence)
        {
            return new WindowsOnlineRoomCloseoutTask
            {
                task_id = taskId,
                status = "done",
                summary = summary,
                evidence = evidence
            };
        }

        private static WindowsOnlineRoomCloseoutTask FindTask(
            WindowsOnlineRoomUsabilityCloseoutReport report,
            string taskId)
        {
            foreach (WindowsOnlineRoomCloseoutTask task in report.completed_tasks)
            {
                if (task != null && task.task_id == taskId)
                {
                    return task;
                }
            }

            return null;
        }
    }
}
