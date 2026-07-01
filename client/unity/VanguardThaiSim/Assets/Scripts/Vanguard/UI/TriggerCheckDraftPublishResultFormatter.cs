using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckDraftPublishResultFormatter
    {
        public const string SuccessMessage = "Published manual trigger draft.";

        public static string Format(ManualTriggerCheckDraftResult result)
        {
            if (result == null)
            {
                return string.Empty;
            }

            if (result.accepted && result.sent)
            {
                return SuccessMessage;
            }

            return FirstNonEmpty(
                result.rejection_reason,
                result.transport_error_code,
                result.transport_message);
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    return values[i];
                }
            }

            return string.Empty;
        }
    }
}
