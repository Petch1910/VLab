using System;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class GameEvent
    {
        public string event_id;
        public GameActionType action_type;
        public int actor_index;
        public string card_instance_id;
        public GameZone from_zone;
        public GameZone to_zone;
        public int from_index;
        public int to_index;
        public GamePhase previous_phase;
        public GamePhase new_phase;
        public GiftMarkerType gift_marker_type;
        public int marker_delta;
        public GameResourceOperationType resource_operation_type;
        public int resource_delta;
        public bool previous_face_up;
        public bool new_face_up;
        public string target_card_instance_id;
        public TriggerCheckSource trigger_check_source;
        public System.Collections.Generic.List<string> card_instance_ids;
        public System.Collections.Generic.List<string> pre_mulligan_hand_ids;
        public System.Collections.Generic.List<string> pre_mulligan_deck_ids;
    }
}
