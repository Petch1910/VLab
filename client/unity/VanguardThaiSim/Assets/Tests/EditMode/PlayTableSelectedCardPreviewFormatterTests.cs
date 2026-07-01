using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableSelectedCardPreviewFormatterTests
    {
        [Test]
        public void NullSelectionUsesInstructionText()
        {
            Assert.AreEqual(
                PlayTableSelectedCardPreviewFormatter.NoSelectionText,
                PlayTableSelectedCardPreviewFormatter.Format(null, GameZone.Hand, null, null));
        }

        [Test]
        public void VisibleCardPreviewIncludesReadableFieldsAndThaiText()
        {
            string preview = PlayTableSelectedCardPreviewFormatter.Format(
                new GameCardInstance("c1", "BT01-001TH", 0, true),
                GameZone.Hand,
                new CardDetail
                {
                    CardId = "BT01-001TH",
                    NameTh = "King of Knights Alfred",
                    Type1 = "Normal Unit",
                    Type2 = "Human",
                    Grade = 3,
                    Power = 10000,
                    Shield = 0,
                    Trigger = "Critical",
                    Clan = "Royal Paladin",
                    Nation = "United Sanctuary",
                    TextTh = "[CONT](VC): sample Thai text"
                },
                "Legal now: Ride to Vanguard.");

            StringAssert.Contains("Card ID: BT01-001TH", preview);
            StringAssert.Contains("Name: King of Knights Alfred", preview);
            StringAssert.Contains("Zone: Hand", preview);
            StringAssert.Contains("Type: Normal Unit / Human", preview);
            StringAssert.Contains("Grade: 3 | Power: 10000 | Shield: 0", preview);
            StringAssert.Contains("Trigger: Critical", preview);
            StringAssert.Contains("Clan/Nation: Royal Paladin / United Sanctuary", preview);
            StringAssert.Contains("Legal now: Ride to Vanguard.", preview);
            StringAssert.Contains("[CONT](VC): sample Thai text", preview);
        }

        [Test]
        public void HiddenCardPreviewDoesNotLeakDetail()
        {
            string preview = PlayTableSelectedCardPreviewFormatter.Format(
                new GameCardInstance("hidden", GameStateViewFactory.HiddenCardId, 1, false),
                GameZone.Hand,
                new CardDetail
                {
                    CardId = "BT01-001TH",
                    NameTh = "Should Not Leak",
                    TextTh = "Hidden skill"
                },
                "Legal now: Guard.");

            StringAssert.Contains("Hidden card", preview);
            StringAssert.Contains(PlayTableSelectedCardPreviewFormatter.HiddenDetailsText, preview);
            Assert.IsFalse(preview.Contains("BT01-001TH"));
            Assert.IsFalse(preview.Contains("Should Not Leak"));
            Assert.IsFalse(preview.Contains("Hidden skill"));
            Assert.IsFalse(preview.Contains("Guard"));
        }

        [Test]
        public void ActionHintSummarizesCardSpecificLegalActions()
        {
            List<LegalGameAction> actions = new List<LegalGameAction>
            {
                new LegalGameAction
                {
                    action_type = GameActionType.MoveCard,
                    card_instance_id = "c1",
                    from_zone = GameZone.Hand,
                    to_zone = GameZone.Vanguard
                },
                new LegalGameAction
                {
                    action_type = GameActionType.MoveCard,
                    card_instance_id = "c1",
                    from_zone = GameZone.Hand,
                    to_zone = GameZone.RearGuard
                },
                new LegalGameAction
                {
                    action_type = GameActionType.Guard,
                    card_instance_id = "c1"
                }
            };

            Assert.AreEqual(
                "Legal now: Ride to Vanguard, Call to Rear-guard, Guard.",
                PlayTableSelectedCardPreviewFormatter.FormatActionHint(actions, "c1"));
        }

        [Test]
        public void ActionHintDoesNotInventActionWhenSelectionHasNoMatch()
        {
            Assert.AreEqual(
                PlayTableSelectedCardPreviewFormatter.NoCardSpecificActionText,
                PlayTableSelectedCardPreviewFormatter.FormatActionHint(new List<LegalGameAction>(), "missing"));
        }
    }
}
