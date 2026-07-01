using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableBoardCardFaceFormatterTests
    {
        [Test]
        public void VisibleCardUsesThaiNameAndStats()
        {
            var card = new GameCardInstance("inst-1", "BT01-001TH", 0, true);
            var detail = new CardDetail
            {
                CardId = "BT01-001TH",
                NameTh = "King of Knights Alfred",
                Grade = 3,
                Power = 10000,
                Shield = 0,
                Trigger = "Critical"
            };

            Assert.AreEqual("King of Knights Alfred", PlayTableBoardCardFaceFormatter.FormatTitle(card, detail));
            Assert.AreEqual("G3 P10000 | Critical", PlayTableBoardCardFaceFormatter.FormatStats(detail));
        }

        [Test]
        public void HiddenCardDoesNotLeakDetailTitleOrThumbnail()
        {
            var card = new GameCardInstance("hidden", GameStateViewFactory.HiddenCardId, 1, false);
            var detail = new CardDetail
            {
                CardId = "BT01-SECRET",
                NameTh = "Should Not Leak",
                ImageExists = true,
                ImageRelativePath = "secret.png"
            };

            Assert.AreEqual(PlayTableBoardCardFaceFormatter.HiddenCardTitle,
                PlayTableBoardCardFaceFormatter.FormatTitle(card, detail));
            Assert.IsFalse(PlayTableBoardCardFaceFormatter.ShouldShowThumbnail(card, detail));
        }

        [Test]
        public void ThumbnailRequiresVisibleImagePath()
        {
            var card = new GameCardInstance("inst-1", "BT01-001TH", 0, true);

            Assert.IsFalse(PlayTableBoardCardFaceFormatter.ShouldShowThumbnail(card, null));
            Assert.IsFalse(PlayTableBoardCardFaceFormatter.ShouldShowThumbnail(
                card,
                new CardDetail { ImageExists = false, ImageRelativePath = "card.png" }));
            Assert.IsFalse(PlayTableBoardCardFaceFormatter.ShouldShowThumbnail(
                card,
                new CardDetail { ImageExists = true, ImageRelativePath = " " }));
            Assert.IsTrue(PlayTableBoardCardFaceFormatter.ShouldShowThumbnail(
                card,
                new CardDetail { ImageExists = true, ImageRelativePath = "card.png" }));
        }
    }
}
