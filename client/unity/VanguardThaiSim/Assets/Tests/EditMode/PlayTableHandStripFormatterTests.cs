using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableHandStripFormatterTests
    {
        [Test]
        public void HeaderFormatsHandCount()
        {
            Assert.AreEqual("Hand (5)", PlayTableHandStripFormatter.FormatHeader(5));
        }

        [Test]
        public void VisibleCardFormatsCompactPlayableLabel()
        {
            string label = PlayTableHandStripFormatter.FormatCardButtonLabel(
                VisibleCard("c1", "BT01-001TH"),
                new CardDetail
                {
                    CardId = "BT01-001TH",
                    NameTh = "King of Knights Alfred",
                    Grade = 3,
                    Trigger = "Critical"
                },
                2,
                14);

            StringAssert.Contains("#2 G3 Critical", label);
            StringAssert.Contains("King of Kni...", label);
        }

        [Test]
        public void HiddenCardDoesNotLeakDetail()
        {
            string label = PlayTableHandStripFormatter.FormatCardButtonLabel(
                new GameCardInstance("hidden", GameStateViewFactory.HiddenCardId, 1, false),
                new CardDetail
                {
                    CardId = "BT01-001TH",
                    NameTh = "Should Not Leak",
                    Grade = 3
                },
                1);

            StringAssert.Contains(PlayTableHandStripFormatter.HiddenCardLabel, label);
            Assert.IsFalse(label.Contains("BT01-001TH"));
            Assert.IsFalse(label.Contains("Should Not Leak"));
        }

        private static GameCardInstance VisibleCard(string instanceId, string cardId)
        {
            return new GameCardInstance(instanceId, cardId, 0, true);
        }
    }
}
