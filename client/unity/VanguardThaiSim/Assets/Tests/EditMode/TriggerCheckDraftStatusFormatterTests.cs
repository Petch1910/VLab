using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftStatusFormatterTests
    {
        [Test]
        public void SelectedStatusFormatsNoneWhenNoCardIsSelected()
        {
            Assert.AreEqual(
                "card none / zone none",
                TriggerCheckDraftStatusFormatter.FormatSelectedStatus(null, null, GameZone.Hand));
        }

        [Test]
        public void SelectedStatusFormatsVisibleCardAndZone()
        {
            Assert.AreEqual(
                "card BT01-001TH / zone Hand",
                TriggerCheckDraftStatusFormatter.FormatSelectedStatus(
                    "BT01-001TH",
                    "instance-1",
                    GameZone.Hand));
        }

        [Test]
        public void SelectedStatusFormatsHiddenCardWithoutRevealingIdentity()
        {
            Assert.AreEqual(
                "card hidden / zone Damage",
                TriggerCheckDraftStatusFormatter.FormatSelectedStatus(
                    GameStateViewFactory.HiddenCardId,
                    "instance-2",
                    GameZone.Damage));
        }

        [Test]
        public void ZoneLabelIsNoneWhenInstanceIsMissing()
        {
            Assert.AreEqual(
                "card BT01-001TH / zone none",
                TriggerCheckDraftStatusFormatter.FormatSelectedStatus(
                    "BT01-001TH",
                    string.Empty,
                    GameZone.RideDeck));
        }

        [Test]
        public void CardLabelShortensLongIdsDeterministically()
        {
            Assert.AreEqual(
                "BT01-001TH-EXTRA-L",
                TriggerCheckDraftStatusFormatter.FormatCardLabel("BT01-001TH-EXTRA-LONG-ID"));
        }
    }
}
