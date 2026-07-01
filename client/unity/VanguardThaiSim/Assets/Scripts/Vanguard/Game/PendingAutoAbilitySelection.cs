namespace VanguardThaiSim.Game
{
    public sealed class PendingAutoAbilitySelectionState
    {
        public bool accepted;
        public bool has_selection;
        public int selected_index = -1;
        public PendingAutoAbility selected_ability;
        public string rejection_reason;
    }

    public static class PendingAutoAbilitySelection
    {
        public const string EmptyQueueReason = "PENDING_AUTO_ABILITY_SELECTION_EMPTY";
        public const string IndexOutOfRangeReason = "PENDING_AUTO_ABILITY_SELECTION_INDEX_OUT_OF_RANGE";

        public static PendingAutoAbilitySelectionState Select(PendingAutoAbilityQueue queue, int index)
        {
            PendingAutoAbilityQueue safeQueue = queue ?? new PendingAutoAbilityQueue();
            safeQueue.EnsureLists();

            if (safeQueue.pending.Count == 0)
            {
                return Reject(EmptyQueueReason);
            }

            if (index < 0 || index >= safeQueue.pending.Count)
            {
                return Reject(IndexOutOfRangeReason);
            }

            PendingAutoAbility ability = safeQueue.pending[index];
            if (ability == null)
            {
                return Reject(IndexOutOfRangeReason);
            }

            return new PendingAutoAbilitySelectionState
            {
                accepted = true,
                has_selection = true,
                selected_index = index,
                selected_ability = PendingAutoAbility.FromJson(ability.ToJson()),
                rejection_reason = string.Empty
            };
        }

        public static PendingAutoAbilitySelectionState Clear()
        {
            return new PendingAutoAbilitySelectionState
            {
                accepted = true,
                has_selection = false,
                selected_index = -1,
                selected_ability = null,
                rejection_reason = string.Empty
            };
        }

        private static PendingAutoAbilitySelectionState Reject(string reason)
        {
            return new PendingAutoAbilitySelectionState
            {
                accepted = false,
                has_selection = false,
                selected_index = -1,
                selected_ability = null,
                rejection_reason = reason ?? string.Empty
            };
        }
    }
}
