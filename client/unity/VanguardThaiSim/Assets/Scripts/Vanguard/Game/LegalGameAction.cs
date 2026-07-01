using System;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class LegalGameAction
    {
        public string label;
        public GameActionType action_type;
        public int actor_index;
        public string card_instance_id;
        public GameZone from_zone;
        public GameZone to_zone;
        public GamePhase phase;
        public GiftMarkerType gift_marker_type;
        public int marker_delta;
        public GameResourceOperationType resource_operation_type;
        public int resource_delta;
        public string target_card_instance_id;
        public TriggerCheckSource trigger_check_source;
        public System.Collections.Generic.List<string> card_instance_ids;
    }
}
