using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class HeadlessReplayArtifact
    {
        public int schema_version = 1;
        public string hidden_state_policy = "local_headless_trace_card_instance_ids_redacted";
        public bool accepted;
        public int seed;
        public string ruleset;
        public string deck_source;
        public int event_count;
        public List<HeadlessReplayEventRecord> events = new List<HeadlessReplayEventRecord>();

        public string ToJson(bool prettyPrint = false)
        {
            if (events == null)
            {
                events = new List<HeadlessReplayEventRecord>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static HeadlessReplayArtifact FromResult(
            HeadlessSimulationResult result,
            IReadOnlyList<GameEvent> eventLog)
        {
            HeadlessReplayArtifact artifact = new HeadlessReplayArtifact
            {
                accepted = result != null && result.accepted,
                seed = result == null ? HeadlessSimulationRunner.DefaultSeed : result.seed,
                ruleset = result == null ? HeadlessSimulationRunner.DefaultRuleset : result.ruleset,
                deck_source = result == null ? "default" : result.deck_source,
                event_count = eventLog == null ? 0 : eventLog.Count
            };

            if (eventLog != null)
            {
                for (int i = 0; i < eventLog.Count; i++)
                {
                    artifact.events.Add(HeadlessReplayEventRecord.FromEvent(i, eventLog[i]));
                }
            }

            return artifact;
        }
    }

    [Serializable]
    public sealed class HeadlessReplayEventRecord
    {
        public int index;
        public string event_id;
        public string action_type;
        public int actor_index;
        public string from_zone;
        public string to_zone;
        public string previous_phase;
        public string new_phase;
        public string gift_marker_type;
        public int marker_delta;
        public string card_identity_policy;

        public static HeadlessReplayEventRecord FromEvent(int index, GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                return new HeadlessReplayEventRecord
                {
                    index = index,
                    card_identity_policy = "no_event"
                };
            }

            return new HeadlessReplayEventRecord
            {
                index = index,
                event_id = gameEvent.event_id,
                action_type = gameEvent.action_type.ToString(),
                actor_index = gameEvent.actor_index,
                from_zone = gameEvent.from_zone.ToString(),
                to_zone = gameEvent.to_zone.ToString(),
                previous_phase = gameEvent.previous_phase.ToString(),
                new_phase = gameEvent.new_phase.ToString(),
                gift_marker_type = gameEvent.gift_marker_type.ToString(),
                marker_delta = gameEvent.marker_delta,
                card_identity_policy = "card_instance_id_redacted"
            };
        }
    }

    public sealed class HeadlessSimulationOutput
    {
        public HeadlessSimulationResult result;
        public HeadlessReplayArtifact replay;
    }
}
