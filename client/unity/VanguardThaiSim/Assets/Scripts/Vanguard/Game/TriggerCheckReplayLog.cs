using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class TriggerCheckReplayLog
    {
        public string log_id = "trigger-check-replay-log";
        public List<TriggerCheckLogEntry> entries = new List<TriggerCheckLogEntry>();

        public void EnsureLists()
        {
            if (entries == null) entries = new List<TriggerCheckLogEntry>();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static TriggerCheckReplayLog FromJson(string json)
        {
            TriggerCheckReplayLog log = JsonUtility.FromJson<TriggerCheckReplayLog>(json);
            if (log == null)
            {
                throw new ArgumentException("Trigger check replay log JSON could not be parsed.", "json");
            }

            log.EnsureLists();
            for (int i = 0; i < log.entries.Count; i++)
            {
                log.entries[i]?.EnsureLists();
            }

            return log;
        }
    }

    public static class TriggerCheckReplayLogBuilder
    {
        public static TriggerCheckReplayLog Append(
            TriggerCheckReplayLog source,
            TriggerCheckLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            TriggerCheckReplayLog result = CloneLog(source);
            result.entries.Add(CloneEntry(entry));
            return result;
        }

        public static TriggerCheckReplayLog VisiblePrefix(TriggerCheckReplayLog source, int visibleCount)
        {
            TriggerCheckReplayLog safeSource = source ?? new TriggerCheckReplayLog();
            safeSource.EnsureLists();
            var result = new TriggerCheckReplayLog
            {
                log_id = safeSource.log_id ?? "trigger-check-replay-log"
            };

            int count = Math.Max(0, Math.Min(visibleCount, safeSource.entries.Count));
            for (int i = 0; i < count; i++)
            {
                TriggerCheckLogEntry entry = safeSource.entries[i];
                if (entry != null)
                {
                    result.entries.Add(CloneEntry(entry));
                }
            }

            return result;
        }

        private static TriggerCheckReplayLog CloneLog(TriggerCheckReplayLog source)
        {
            TriggerCheckReplayLog safeSource = source ?? new TriggerCheckReplayLog();
            safeSource.EnsureLists();
            var clone = new TriggerCheckReplayLog
            {
                log_id = safeSource.log_id ?? "trigger-check-replay-log"
            };

            for (int i = 0; i < safeSource.entries.Count; i++)
            {
                TriggerCheckLogEntry entry = safeSource.entries[i];
                if (entry != null)
                {
                    clone.entries.Add(CloneEntry(entry));
                }
            }

            return clone;
        }

        private static TriggerCheckLogEntry CloneEntry(TriggerCheckLogEntry entry)
        {
            entry.EnsureLists();
            return TriggerCheckLogEntry.FromJson(entry.ToJson());
        }
    }
}
