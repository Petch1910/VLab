using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Headless
{
    [Serializable]
    public sealed class HeadlessObservation
    {
        public int schema_version = 1;
        public string observation_id;
        public string perspective = GameStateViewPerspective.Player.ToString();
        public int player_index;
        public int seed;
        public string ruleset;
        public string phase;
        public int turn_number;
        public int turn_player_index;
        public int event_count;
        public int player_deck_count;
        public int player_hand_count;
        public int player_ride_deck_count;
        public int player_vanguard_count;
        public int player_rear_guard_count;
        public int player_damage_count;
        public int player_drop_count;
        public int player_bind_count;
        public int opponent_deck_count;
        public int opponent_hand_count;
        public int opponent_vanguard_count;
        public int opponent_rear_guard_count;
        public int opponent_damage_count;
        public int legal_action_count;
        public List<HeadlessActionMaskEntry> legal_actions = new List<HeadlessActionMaskEntry>();

        public string ToJson(bool prettyPrint = false)
        {
            if (legal_actions == null)
            {
                legal_actions = new List<HeadlessActionMaskEntry>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    [Serializable]
    public sealed class HeadlessActionMaskEntry
    {
        public int index;
        public string action_id;
        public string action_type;
        public int actor_index;
        public bool requires_card_selection;
        public string from_zone;
        public string to_zone;
        public string phase;
        public string gift_marker_type;
        public int marker_delta;
        public string resource_operation_type;
        public int resource_delta;
    }

    [Serializable]
    public sealed class HeadlessRewardSignal
    {
        public int schema_version = 1;
        public string reward_model = "m17_06_smoke_acceptance_v1";
        public bool terminal;
        public float reward;
        public string reason;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    [Serializable]
    public sealed class HeadlessObservationActionRewardSample
    {
        public int schema_version = 1;
        public HeadlessObservation observation;
        public HeadlessRewardSignal reward;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    public static class HeadlessObservationActionRewardApi
    {
        public static HeadlessObservation CreateObservation(
            GameState source,
            int playerIndex,
            string ruleset,
            int seed)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            GameState view = GameStateViewFactory.CreatePlayerView(source, playerIndex);
            PlayerGameState player = view.GetPlayer(playerIndex);
            PlayerGameState opponent = view.GetPlayer(playerIndex == 0 ? 1 : 0);
            IReadOnlyList<LegalGameAction> legalActions = RulesCore.GetLegalActions(view, playerIndex);

            HeadlessObservation observation = new HeadlessObservation
            {
                observation_id = BuildObservationId(seed, view.event_log.Count, playerIndex),
                player_index = playerIndex,
                seed = seed,
                ruleset = string.IsNullOrWhiteSpace(ruleset) ? HeadlessSimulationRunner.DefaultRuleset : ruleset.Trim(),
                phase = view.phase.ToString(),
                turn_number = view.turn_number,
                turn_player_index = view.turn_player_index,
                event_count = view.event_log.Count,
                player_deck_count = player.CountZone(GameZone.Deck),
                player_hand_count = player.CountZone(GameZone.Hand),
                player_ride_deck_count = player.CountZone(GameZone.RideDeck),
                player_vanguard_count = player.CountZone(GameZone.Vanguard),
                player_rear_guard_count = player.CountZone(GameZone.RearGuard),
                player_damage_count = player.CountZone(GameZone.Damage),
                player_drop_count = player.CountZone(GameZone.Drop),
                player_bind_count = player.CountZone(GameZone.Bind),
                opponent_deck_count = opponent.CountZone(GameZone.Deck),
                opponent_hand_count = opponent.CountZone(GameZone.Hand),
                opponent_vanguard_count = opponent.CountZone(GameZone.Vanguard),
                opponent_rear_guard_count = opponent.CountZone(GameZone.RearGuard),
                opponent_damage_count = opponent.CountZone(GameZone.Damage)
            };

            for (int i = 0; i < legalActions.Count; i++)
            {
                observation.legal_actions.Add(CreateActionEntry(i, legalActions[i]));
            }

            observation.legal_action_count = observation.legal_actions.Count;
            return observation;
        }

        public static HeadlessRewardSignal CreateReward(HeadlessSimulationResult result)
        {
            if (result == null)
            {
                return new HeadlessRewardSignal
                {
                    terminal = true,
                    reward = -1f,
                    reason = "missing_result"
                };
            }

            if (!result.accepted)
            {
                return new HeadlessRewardSignal
                {
                    terminal = true,
                    reward = -1f,
                    reason = string.IsNullOrWhiteSpace(result.failure_reason) ? "rejected" : result.failure_reason
                };
            }

            return new HeadlessRewardSignal
            {
                terminal = false,
                reward = 0f,
                reason = "non_terminal_no_match_result"
            };
        }

        public static HeadlessObservationActionRewardSample CreateSample(
            GameState source,
            int playerIndex,
            string ruleset,
            int seed,
            HeadlessSimulationResult result)
        {
            return new HeadlessObservationActionRewardSample
            {
                observation = CreateObservation(source, playerIndex, ruleset, seed),
                reward = CreateReward(result)
            };
        }

        private static HeadlessActionMaskEntry CreateActionEntry(int index, LegalGameAction action)
        {
            string actionType = action == null ? "None" : action.action_type.ToString();
            string fromZone = action == null ? "None" : action.from_zone.ToString();
            string toZone = action == null ? "None" : action.to_zone.ToString();
            string phase = action == null ? "None" : action.phase.ToString();
            string giftMarker = action == null ? "None" : action.gift_marker_type.ToString();
            string resourceOperation = action == null ? "None" : action.resource_operation_type.ToString();
            int actorIndex = action == null ? -1 : action.actor_index;
            int markerDelta = action == null ? 0 : action.marker_delta;
            int resourceDelta = action == null ? 0 : action.resource_delta;
            bool requiresCard = action != null && !string.IsNullOrWhiteSpace(action.card_instance_id);

            return new HeadlessActionMaskEntry
            {
                index = index,
                action_id = BuildActionId(index, actionType, fromZone, toZone, phase, giftMarker, resourceOperation),
                action_type = actionType,
                actor_index = actorIndex,
                requires_card_selection = requiresCard,
                from_zone = fromZone,
                to_zone = toZone,
                phase = phase,
                gift_marker_type = giftMarker,
                marker_delta = markerDelta,
                resource_operation_type = resourceOperation,
                resource_delta = resourceDelta
            };
        }

        private static string BuildObservationId(int seed, int eventCount, int playerIndex)
        {
            return "obs-s" + seed.ToString() + "-e" + eventCount.ToString("D4") + "-p" + playerIndex.ToString();
        }

        private static string BuildActionId(
            int index,
            string actionType,
            string fromZone,
            string toZone,
            string phase,
            string giftMarker,
            string resourceOperation)
        {
            return "a" + index.ToString("D4") + ":" + actionType + ":" + fromZone + ":" + toZone + ":" +
                   phase + ":" + giftMarker + ":" + resourceOperation;
        }
    }
}
