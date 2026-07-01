using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableActionStatusFormatter
    {
        public const string CannotDrawMessage = "Cannot draw.";
        public const string CannotTriggerCheckMessage = "Cannot check trigger.";
        public const string CannotGuardMessage = "Cannot guard with the selected card.";
        public const string CannotAttackMessage = "Cannot attack with the selected card.";
        public const string CannotAttackTargetMessage = "Select your attacker and an opponent target first.";
        public const string UndoDisabledOnlineMessage = "Undo is disabled in online rooms.";

        public static string FormatCannotDraw()
        {
            return CannotDrawMessage;
        }

        public static string FormatCannotSetPhase(GamePhase phase)
        {
            return "Cannot set phase to " + phase + ".";
        }

        public static string FormatCannotAddGiftMarker(GiftMarkerType markerType)
        {
            return "Cannot add " + markerType + " marker.";
        }

        public static string FormatCannotTriggerCheck()
        {
            return CannotTriggerCheckMessage;
        }

        public static string FormatCannotGuard()
        {
            return CannotGuardMessage;
        }

        public static string FormatCannotAttack()
        {
            return CannotAttackMessage;
        }

        public static string FormatCannotAttackTarget()
        {
            return CannotAttackTargetMessage;
        }

        public static string FormatUndoDisabledOnline()
        {
            return UndoDisabledOnlineMessage;
        }
    }
}
