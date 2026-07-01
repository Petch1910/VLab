using System;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class RoomLifecycleTransitionResult
    {
        public bool accepted;
        public string rejection_reason;
        public MultiplayerRoomState room;

        public static RoomLifecycleTransitionResult Accepted(MultiplayerRoomState room)
        {
            return new RoomLifecycleTransitionResult
            {
                accepted = true,
                room = room
            };
        }

        public static RoomLifecycleTransitionResult Rejected(string rejectionReason)
        {
            return new RoomLifecycleTransitionResult
            {
                accepted = false,
                rejection_reason = string.IsNullOrWhiteSpace(rejectionReason)
                    ? "ROOM_LIFECYCLE_REJECTED"
                    : rejectionReason
            };
        }
    }
}
