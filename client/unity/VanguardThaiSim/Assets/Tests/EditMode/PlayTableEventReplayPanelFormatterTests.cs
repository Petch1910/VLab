using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableEventReplayPanelFormatterTests
    {
        [Test]
        public void EmptyLogFormatsCompactPanel()
        {
            Assert.AreEqual(
                PlayTableEventReplayPanelFormatter.EmptyPanelMessage,
                PlayTableEventReplayPanelFormatter.Format(null));
            Assert.AreEqual(
                PlayTableEventReplayPanelFormatter.EmptyPanelMessage,
                PlayTableEventReplayPanelFormatter.Format(new List<GameEvent>()));
            Assert.AreEqual(
                PlayTableEventReplayPanelFormatter.EmptyCompactPanelMessage,
                PlayTableEventReplayPanelFormatter.FormatCompact(null));
            Assert.AreEqual(
                PlayTableEventReplayPanelFormatter.EmptyCompactPanelMessage,
                PlayTableEventReplayPanelFormatter.FormatCompact(new List<GameEvent>()));
        }

        [Test]
        public void PanelShowsCountLatestAndRecentNewestFirst()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.MoveCard,
                    from_zone = GameZone.Hand,
                    to_zone = GameZone.RearGuard,
                    card_instance_id = "card-1"
                },
                new GameEvent
                {
                    action_type = GameActionType.SetPhase,
                    previous_phase = GamePhase.Main,
                    new_phase = GamePhase.Battle
                }
            };

            string formatted = PlayTableEventReplayPanelFormatter.Format(events);

            Assert.IsTrue(formatted.Contains("Match log"));
            Assert.IsTrue(formatted.Contains("Events: 2"));
            Assert.IsTrue(formatted.Contains("Latest: #2 P1 changed phase from main to battle."));
            Assert.Less(formatted.IndexOf("#2 P1 changed phase"), formatted.IndexOf("#1 P1 moved a card"));
            Assert.IsFalse(formatted.Contains("SetPhase"));
            Assert.IsFalse(formatted.Contains("MoveCard"));
            Assert.IsFalse(formatted.Contains("card-1"));
        }

        [Test]
        public void ResourceFlipOmitsCardInstanceIdInPanel()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.ResourceFlip,
                    resource_operation_type = GameResourceOperationType.CounterBlast,
                    card_instance_id = "private-damage-instance"
                }
            };

            string formatted = PlayTableEventReplayPanelFormatter.Format(events);

            Assert.IsTrue(formatted.Contains("P1 resolved Counter-Blast."));
            Assert.IsFalse(formatted.Contains("private-damage-instance"));
            Assert.IsFalse(formatted.Contains("ResourceFlip"));
        }

        [Test]
        public void TriggerCheckPanelShowsSourceWithoutCardInstanceId()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.TriggerCheck,
                    trigger_check_source = TriggerCheckSource.Damage,
                    from_zone = GameZone.Deck,
                    to_zone = GameZone.Trigger,
                    card_instance_id = "private-trigger-card"
                }
            };

            string formatted = PlayTableEventReplayPanelFormatter.Format(events);

            Assert.IsTrue(formatted.Contains("P1 performed a damage check."));
            Assert.IsFalse(formatted.Contains("private-trigger-card"));
            Assert.IsFalse(formatted.Contains("TriggerCheck"));
        }

        [Test]
        public void DeclareAttackPanelDoesNotShowInstanceIds()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.DeclareAttack,
                    card_instance_id = "private-attacker",
                    target_card_instance_id = "private-target"
                }
            };

            string formatted = PlayTableEventReplayPanelFormatter.Format(events);

            Assert.IsTrue(formatted.Contains("P1 declared an attack."));
            Assert.IsFalse(formatted.Contains("private-attacker"));
            Assert.IsFalse(formatted.Contains("private-target"));
            Assert.IsFalse(formatted.Contains("DeclareAttack"));
        }

        [Test]
        public void PanelRespectsMaxEntries()
        {
            var events = new List<GameEvent>();
            for (int i = 0; i < 5; i++)
            {
                events.Add(new GameEvent
                {
                    action_type = GameActionType.Draw,
                    actor_index = 0,
                    previous_phase = GamePhase.StandAndDraw,
                    new_phase = GamePhase.Main
                });
            }

            string formatted = PlayTableEventReplayPanelFormatter.Format(events, 2);

            Assert.IsTrue(formatted.Contains("Events: 5"));
            Assert.IsTrue(formatted.Contains("#5 P1 drew 1 card."));
            Assert.IsTrue(formatted.Contains("#4 P1 drew 1 card."));
            Assert.IsFalse(formatted.Contains("#3 P1 drew 1 card."));
        }

        [Test]
        public void CompactPanelShowsLatestThreeAndAdvancedHint()
        {
            var events = new List<GameEvent>();
            for (int i = 0; i < 5; i++)
            {
                events.Add(new GameEvent
                {
                    action_type = GameActionType.Draw,
                    actor_index = 0
                });
            }

            string formatted = PlayTableEventReplayPanelFormatter.FormatCompact(events);

            Assert.IsTrue(formatted.Contains("Events: 5"));
            Assert.IsTrue(formatted.Contains("#5 P1 drew 1 card."));
            Assert.IsTrue(formatted.Contains("#4 P1 drew 1 card."));
            Assert.IsTrue(formatted.Contains("#3 P1 drew 1 card."));
            Assert.IsFalse(formatted.Contains("#2 P1 drew 1 card."));
            Assert.IsTrue(formatted.Contains("Full log: Advanced"));
        }

        [Test]
        public void FormattingDoesNotMutateEventLog()
        {
            var events = new List<GameEvent>
            {
                new GameEvent
                {
                    action_type = GameActionType.AddGiftMarker,
                    gift_marker_type = GiftMarkerType.Force,
                    marker_delta = 1
                }
            };
            string before = JsonUtility.ToJson(events[0], false);

            PlayTableEventReplayPanelFormatter.Format(events);

            Assert.AreEqual(before, JsonUtility.ToJson(events[0], false));
        }
    }
}
