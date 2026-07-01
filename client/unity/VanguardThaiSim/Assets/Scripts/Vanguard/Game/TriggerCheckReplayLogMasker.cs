using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public static class TriggerCheckReplayLogMasker
    {
        public static TriggerCheckReplayLog CreateView(
            TriggerCheckReplayLog source,
            GameStateViewPerspective perspective,
            int viewerPlayerIndex = -1,
            bool revealOwnerChecks = true)
        {
            TriggerCheckReplayLog view = CloneLog(source);
            if (perspective == GameStateViewPerspective.TrueState)
            {
                return view;
            }

            view.EnsureLists();
            for (int i = 0; i < view.entries.Count; i++)
            {
                TriggerCheckLogEntry entry = view.entries[i];
                if (entry == null)
                {
                    continue;
                }

                bool canSeeIdentity = perspective == GameStateViewPerspective.Player &&
                                      revealOwnerChecks &&
                                      entry.player_index == viewerPlayerIndex;
                if (!canSeeIdentity)
                {
                    MaskEntry(entry, i);
                }
            }

            return view;
        }

        private static TriggerCheckReplayLog CloneLog(TriggerCheckReplayLog source)
        {
            TriggerCheckReplayLog safeSource = source ?? new TriggerCheckReplayLog();
            safeSource.EnsureLists();
            return TriggerCheckReplayLog.FromJson(safeSource.ToJson());
        }

        private static void MaskEntry(TriggerCheckLogEntry entry, int entryIndex)
        {
            entry.EnsureLists();
            entry.hides_checked_card_identity = true;
            entry.checked_card_instance_id = "hidden-trigger-check-" + entryIndex.ToString("D4");
            entry.checked_card_id = GameStateViewFactory.HiddenCardId;
            entry.log_entry_id = BuildMaskedLogEntryId(entry, entryIndex);
            MaskModifierIds(entry.modifier_ids, entryIndex);
            entry.summary = BuildMaskedSummary(entry);
        }

        private static string BuildMaskedLogEntryId(TriggerCheckLogEntry entry, int entryIndex)
        {
            return "trigger-log-hidden|" +
                   entry.check_source +
                   "|" + entry.player_index +
                   "|" + entry.check_index +
                   "|" + entryIndex.ToString("D4") +
                   "|" + entry.trigger_type;
        }

        private static void MaskModifierIds(List<string> modifierIds, int entryIndex)
        {
            if (modifierIds == null)
            {
                return;
            }

            for (int i = 0; i < modifierIds.Count; i++)
            {
                modifierIds[i] = "hidden-trigger-modifier-" +
                                 entryIndex.ToString("D4") +
                                 "-" +
                                 i.ToString("D4");
            }
        }

        private static string BuildMaskedSummary(TriggerCheckLogEntry entry)
        {
            string status = entry.accepted ? "accepted" : "manual";
            return entry.check_source +
                   " check " + entry.check_index +
                   " " + GameStateViewFactory.HiddenCardId +
                   " " + entry.trigger_type +
                   " " + status +
                   "; modifiers=" + entry.modifier_count;
        }
    }
}
