using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableZoneStatusFormatterTests
    {
        [Test]
        public void NullPlayerUsesUnavailableText()
        {
            Assert.AreEqual(
                PlayTableZoneStatusFormatter.MissingPlayerText,
                PlayTableZoneStatusFormatter.Format(null));
        }

        [Test]
        public void FormatIncludesCoreZoneCountsAndSoulBoundary()
        {
            PlayerGameState player = new PlayerGameState();
            player.deck.Add(Card("deck-1"));
            player.hand.Add(Card("hand-1"));
            player.drop.Add(Card("drop-1"));
            player.damage.Add(Card("damage-1"));
            player.damage.Add(Card("damage-2"));
            player.bind.Add(Card("bind-1"));
            player.order.Add(Card("order-1"));
            player.ride_deck.Add(Card("ride-1"));
            player.trigger.Add(Card("trigger-1"));
            player.trigger.Add(Card("trigger-2"));
            player.soul.Add(Card("soul-1"));
            player.soul.Add(Card("soul-2"));
            player.g_zone.Add(Card("g-1"));
            player.guardian.Add(Card("guardian-1"));

            string status = PlayTableZoneStatusFormatter.Format(player);

            StringAssert.Contains("Deck: 1", status);
            StringAssert.Contains("Hand: 1", status);
            StringAssert.Contains("Drop: 1", status);
            StringAssert.Contains("Damage: 2", status);
            StringAssert.Contains("Bind: 1", status);
            StringAssert.Contains("Order: 1", status);
            StringAssert.Contains("Ride Deck: 1", status);
            StringAssert.Contains("Trigger Zone: 2", status);
            StringAssert.Contains("Soul: 2", status);
            StringAssert.Contains("G Zone: 1", status);
            StringAssert.Contains("Guardian: 1", status);
        }

        private static GameCardInstance Card(string id)
        {
            return new GameCardInstance
            {
                card_id = id,
                instance_id = id + "-instance",
                owner_index = 0,
                face_up = true
            };
        }
    }
}
