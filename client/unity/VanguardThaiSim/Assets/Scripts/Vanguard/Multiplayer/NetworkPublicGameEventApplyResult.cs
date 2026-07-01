using System;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkPublicGameEventApplyResult
    {
        public bool accepted;
        public string rejection_reason;
        public string event_id;

        public static NetworkPublicGameEventApplyResult Accepted(NetworkPublicGameEvent publicEvent)
        {
            return new NetworkPublicGameEventApplyResult
            {
                accepted = true,
                event_id = publicEvent == null ? "" : publicEvent.event_id
            };
        }

        public static NetworkPublicGameEventApplyResult Rejected(string rejectionReason)
        {
            return new NetworkPublicGameEventApplyResult
            {
                accepted = false,
                rejection_reason = string.IsNullOrWhiteSpace(rejectionReason)
                    ? "PUBLIC_EVENT_APPLY_REJECTED"
                    : rejectionReason
            };
        }
    }
}
