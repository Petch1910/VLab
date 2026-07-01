using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableCardSelectionStatusFormatterTests
    {
        [Test]
        public void SelectCardFirstFormatsExistingText()
        {
            Assert.AreEqual(
                PlayTableCardSelectionStatusFormatter.SelectCardFirstMessage,
                PlayTableCardSelectionStatusFormatter.FormatSelectCardFirst());
        }

        [Test]
        public void SelectedCardFormatsCardIdAndZone()
        {
            Assert.AreEqual(
                "Selected BT01-001TH from Vanguard.",
                PlayTableCardSelectionStatusFormatter.FormatSelectedCard("BT01-001TH", GameZone.Vanguard));
        }

        [Test]
        public void SelectedTargetFormatsCardIdAndOpponentZone()
        {
            Assert.AreEqual(
                "Target BT01-002TH from opponent RearGuard.",
                PlayTableCardSelectionStatusFormatter.FormatSelectedTarget("BT01-002TH", GameZone.RearGuard));
        }

        [Test]
        public void NoCardSelectedFormatsExistingText()
        {
            Assert.AreEqual(
                PlayTableCardSelectionStatusFormatter.NoCardSelectedMessage,
                PlayTableCardSelectionStatusFormatter.FormatNoCardSelected());
        }

        [Test]
        public void NullCardIdPreservesStringConcatenationBehavior()
        {
            Assert.AreEqual(
                "Selected  from Drop.",
                PlayTableCardSelectionStatusFormatter.FormatSelectedCard(null, GameZone.Drop));
        }
    }
}
