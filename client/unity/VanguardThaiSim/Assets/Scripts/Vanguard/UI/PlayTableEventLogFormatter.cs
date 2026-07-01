using System;
using System.Collections.Generic;
using System.Text;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableEventLogFormatter
    {
        public const string EmptyLogMessage = "Match log is empty.";
        public const int DefaultMaxEntries = 18;

        public static string Format(IReadOnlyList<GameEvent> eventLog, int maxEntries = DefaultMaxEntries)
        {
            if (eventLog == null || eventLog.Count == 0)
            {
                return EmptyLogMessage;
            }

            StringBuilder builder = new StringBuilder();
            int safeMaxEntries = Math.Max(0, maxEntries);
            int start = Math.Max(0, eventLog.Count - safeMaxEntries);
            for (int i = eventLog.Count - 1; i >= start; i--)
            {
                builder.AppendLine(FormatEventLine(eventLog[i], i));
            }

            return builder.ToString();
        }

        public static string FormatEventLine(GameEvent gameEvent, int index)
        {
            string prefix = "#" + (index + 1) + " ";
            if (gameEvent == null)
            {
                return prefix + "Unknown event.";
            }

            string actor = FormatActor(gameEvent.actor_index);
            switch (gameEvent.action_type)
            {
                case GameActionType.Draw:
                    return prefix + actor + " drew 1 card.";
                case GameActionType.MoveCard:
                    return prefix + actor + " moved a card from " +
                        FormatZone(gameEvent.from_zone) + " to " +
                        FormatZone(gameEvent.to_zone) + ".";
                case GameActionType.SetPhase:
                    return prefix + actor + " changed phase from " +
                        FormatPhase(gameEvent.previous_phase) + " to " +
                        FormatPhase(gameEvent.new_phase) + ".";
                case GameActionType.AddGiftMarker:
                    return prefix + actor + " gained " + Math.Max(0, gameEvent.marker_delta) +
                        " " + gameEvent.gift_marker_type + " marker.";
                case GameActionType.ResourceFlip:
                    return prefix + actor + " resolved " +
                        FormatResourceOperation(gameEvent.resource_operation_type) + ".";
                case GameActionType.DeclareAttack:
                    return prefix + actor + " declared an attack.";
                case GameActionType.Guard:
                    return prefix + actor + " guarded with a card.";
                case GameActionType.TriggerCheck:
                    return prefix + actor + " performed a " +
                        FormatTriggerCheckSource(gameEvent.trigger_check_source) + " check.";
                case GameActionType.MulliganCards:
                    return prefix + actor + " mulliganed " +
                        Count(gameEvent.card_instance_ids) + " card(s).";
                default:
                    return prefix + actor + " resolved " + gameEvent.action_type + ".";
            }
        }

        private static string FormatActor(int actorIndex)
        {
            return actorIndex >= 0 ? "P" + (actorIndex + 1) : "P?";
        }

        private static int Count(IReadOnlyCollection<string> values)
        {
            return values == null ? 0 : values.Count;
        }

        private static string FormatResourceOperation(GameResourceOperationType operationType)
        {
            switch (operationType)
            {
                case GameResourceOperationType.CounterBlast:
                    return "Counter-Blast";
                case GameResourceOperationType.CounterCharge:
                    return "Counter-Charge";
                case GameResourceOperationType.SoulBlast:
                    return "Soul-Blast";
                case GameResourceOperationType.SoulCharge:
                    return "Soul-Charge";
                default:
                    return operationType.ToString();
            }
        }

        private static string FormatTriggerCheckSource(TriggerCheckSource source)
        {
            switch (source)
            {
                case TriggerCheckSource.Drive:
                    return "drive";
                case TriggerCheckSource.Damage:
                    return "damage";
                case TriggerCheckSource.Manual:
                    return "manual";
                default:
                    return "trigger";
            }
        }

        private static string FormatPhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.StandAndDraw:
                    return "stand & draw";
                case GamePhase.Ride:
                    return "ride";
                case GamePhase.Main:
                    return "main";
                case GamePhase.Battle:
                    return "battle";
                case GamePhase.End:
                    return "end";
                case GamePhase.Mulligan:
                    return "mulligan";
                default:
                    return phase.ToString();
            }
        }

        private static string FormatZone(GameZone zone)
        {
            switch (zone)
            {
                case GameZone.Deck:
                    return "deck";
                case GameZone.Hand:
                    return "hand";
                case GameZone.RideDeck:
                    return "ride deck";
                case GameZone.Vanguard:
                    return "vanguard";
                case GameZone.RearGuard:
                    return "rear-guard";
                case GameZone.Drop:
                    return "drop";
                case GameZone.Damage:
                    return "damage";
                case GameZone.Bind:
                    return "bind";
                case GameZone.Trigger:
                    return "trigger zone";
                case GameZone.Order:
                    return "order";
                case GameZone.Soul:
                    return "soul";
                case GameZone.GZone:
                    return "G zone";
                case GameZone.Guardian:
                    return "guardian";
                default:
                    return zone.ToString();
            }
        }
    }
}
