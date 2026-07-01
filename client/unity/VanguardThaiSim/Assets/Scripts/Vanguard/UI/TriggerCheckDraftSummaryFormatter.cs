using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckDraftSummaryFormatter
    {
        public static string FormatSummary(
            bool isOnline,
            TriggerType triggerType,
            TriggerCheckSource checkSource,
            int checkIndex,
            string selectedCardId,
            string selectedCardInstanceId,
            GameZone selectedZone)
        {
            if (!isOnline)
            {
                return FormatLocalMode();
            }

            return FormatOnlineSummary(
                triggerType,
                checkSource,
                checkIndex,
                selectedCardId,
                selectedCardInstanceId,
                selectedZone);
        }

        public static string FormatLocalMode()
        {
            return "Draft: local mode";
        }

        public static string FormatOnlineSummary(
            TriggerType triggerType,
            TriggerCheckSource checkSource,
            int checkIndex,
            string selectedCardId,
            string selectedCardInstanceId,
            GameZone selectedZone)
        {
            return "Draft: " +
                   TriggerCheckDraftMetadataFormatter.FormatSummary(triggerType, checkSource, checkIndex) +
                   " / " +
                   TriggerCheckDraftStatusFormatter.FormatSelectedStatus(
                       selectedCardId,
                       selectedCardInstanceId,
                       selectedZone);
        }
    }
}
