using System;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class OwnerPrivateRoomInitializationResult
    {
        public bool accepted;
        public string rejection_reason;
        public LocalOwnerPrivateSession session;

        public static OwnerPrivateRoomInitializationResult Accepted(LocalOwnerPrivateSession session)
        {
            return new OwnerPrivateRoomInitializationResult
            {
                accepted = true,
                session = session
            };
        }

        public static OwnerPrivateRoomInitializationResult Rejected(string rejectionReason)
        {
            return new OwnerPrivateRoomInitializationResult
            {
                accepted = false,
                rejection_reason = string.IsNullOrWhiteSpace(rejectionReason)
                    ? "OWNER_PRIVATE_ROOM_INITIALIZATION_REJECTED"
                    : rejectionReason,
                session = null
            };
        }
    }
}
