using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityPublishResultFormatter
    {
        public const string SuccessMessage = "Published pending auto ability queue.";
        public const string NullResultMessage = "TRANSPORT_ERROR: no result returned.";

        public static string Format(MultiplayerTransportResult result)
        {
            if (result == null)
            {
                return NullResultMessage;
            }

            if (result.ok)
            {
                return SuccessMessage;
            }

            return result.error_code + ": " + result.message;
        }
    }
}
