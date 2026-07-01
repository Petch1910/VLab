using System.Collections.Generic;
using System;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableEventLogFormatterTests
    {
        [Test]
        public void EmptyLogFormatsPlayerFacingText()
        {
            Assert.AreEqual(
                PlayTableEventLogFormatter.EmptyLogMessage,
                PlayTableEventLogFormatter.Format(null));
            Assert.AreEqual(
                PlayTableEventLogFormatter.EmptyLogMessage,
                PlayTableEventLogFormatter.Format(new List<GameEvent>()));
        }

        [Test]
        public void MoveCardEventFormatsPlayerFacingLine()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.MoveCard,
                    actor_index = 0,
                    card_instance_id = "card-1",
                    from_zone = GameZone.Hand,
                    to_zone = GameZone.RearGuard
                }
            };

            string formatted = PlayTableEventLogFormatter.Format(events);

            Assert.AreEqual(
                "#1 P1 moved a card from hand to rear-guard." + Environment.NewLine,
                formatted);
            Assert.IsFalse(formatted.Contains("MoveCard"));
            Assert.IsFalse(formatted.Contains("card-1"));
        }

        [Test]
        public void GiftMarkerEventFormatsExistingLine()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.AddGiftMarker,
                    actor_index = 1,
                    gift_marker_type = GiftMarkerType.Accel,
                    marker_delta = 1
                }
            };

            Assert.AreEqual(
                "#1 P2 gained 1 Accel marker." + Environment.NewLine,
                PlayTableEventLogFormatter.Format(events));
        }

        [Test]
        public void PhaseEventFormatsExistingFallbackLine()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.SetPhase,
                    actor_index = 0,
                    previous_phase = GamePhase.Main,
                    new_phase = GamePhase.Battle
                }
            };

            Assert.AreEqual(
                "#1 P1 changed phase from main to battle." + Environment.NewLine,
                PlayTableEventLogFormatter.Format(events));
        }

        [Test]
        public void ResourceFlipEventFormatsOperationAndCard()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.ResourceFlip,
                    actor_index = 0,
                    resource_operation_type = GameResourceOperationType.CounterBlast,
                    card_instance_id = "damage-1"
                }
            };

            string formatted = PlayTableEventLogFormatter.Format(events);

            Assert.AreEqual(
                "#1 P1 resolved Counter-Blast." + Environment.NewLine,
                formatted);
            Assert.IsFalse(formatted.Contains("damage-1"));
            Assert.IsFalse(formatted.Contains("ResourceFlip"));
        }

        [Test]
        public void TriggerCheckEventFormatsSourceWithoutCardInstanceId()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.TriggerCheck,
                    actor_index = 0,
                    trigger_check_source = TriggerCheckSource.Drive,
                    from_zone = GameZone.Deck,
                    to_zone = GameZone.Trigger,
                    card_instance_id = "private-trigger-card"
                }
            };

            string formatted = PlayTableEventLogFormatter.Format(events);

            Assert.AreEqual(
                "#1 P1 performed a drive check." + Environment.NewLine,
                formatted);
            Assert.IsFalse(formatted.Contains("private-trigger-card"));
            Assert.IsFalse(formatted.Contains("TriggerCheck"));
        }

        [Test]
        public void DeclareAttackEventFormatsWithoutInstanceIds()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.DeclareAttack,
                    actor_index = 0,
                    card_instance_id = "private-attacker",
                    target_card_instance_id = "private-target"
                }
            };

            string formatted = PlayTableEventLogFormatter.Format(events);

            Assert.AreEqual(
                "#1 P1 declared an attack." + Environment.NewLine,
                formatted);
            Assert.IsFalse(formatted.Contains("private-attacker"));
            Assert.IsFalse(formatted.Contains("private-target"));
            Assert.IsFalse(formatted.Contains("DeclareAttack"));
        }

        [Test]
        public void MulliganEventFormatsCountWithoutCardIds()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.MulliganCards,
                    actor_index = 0,
                    card_instance_ids = new List<string> { "secret-1", "secret-2" }
                }
            };

            string formatted = PlayTableEventLogFormatter.Format(events);

            Assert.AreEqual(
                "#1 P1 mulliganed 2 card(s)." + Environment.NewLine,
                formatted);
            Assert.IsFalse(formatted.Contains("secret-1"));
            Assert.IsFalse(formatted.Contains("MulliganCards"));
        }

        [Test]
        public void EventsAreNewestFirstAndLimitedByMaxEntries()
        {
            var events = new List<GameEvent>();
            for (int i = 0; i < 20; i++)
            {
                events.Add(new GameEvent
                {
                    action_type = GameActionType.Draw,
                    actor_index = 0,
                    previous_phase = GamePhase.StandAndDraw,
                    new_phase = GamePhase.Main
                });
            }

            string text = PlayTableEventLogFormatter.Format(events, 3);

            Assert.AreEqual(
                "#20 P1 drew 1 card." + Environment.NewLine +
                "#19 P1 drew 1 card." + Environment.NewLine +
                "#18 P1 drew 1 card." + Environment.NewLine,
                text);
        }
    }
}
