using System;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkPublicReconnectApplyResult
    {
        public bool accepted;
        public string rejection_reason;
        public int applied_count;

        public static NetworkPublicReconnectApplyResult Accepted(int appliedCount)
        {
            return new NetworkPublicReconnectApplyResult
            {
                accepted = true,
                applied_count = appliedCount
            };
        }

        public static NetworkPublicReconnectApplyResult Rejected(string rejectionReason)
        {
            return new NetworkPublicReconnectApplyResult
            {
                accepted = false,
                rejection_reason = string.IsNullOrWhiteSpace(rejectionReason)
                    ? "PUBLIC_RECONNECT_REJECTED"
                    : rejectionReason
            };
        }
    }
}
