using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class MultiplayerLobbyDeckReadinessTests
    {
        [Test]
        public void ReadinessAcceptsCountReadyDeck()
        {
            MultiplayerLobbyDeckReadinessResult result =
                MultiplayerLobbyDeckReadiness.Evaluate(CreateDeck("ready", 50, 4, 0));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(50, result.main_count);
            Assert.AreEqual(4, result.ride_count);
        }

        [Test]
        public void ReadinessRejectsMissingOrCountUnreadyDecks()
        {
            Assert.IsFalse(MultiplayerLobbyDeckReadiness.Evaluate(null).accepted);
            Assert.IsFalse(MultiplayerLobbyDeckReadiness.Evaluate(CreateDeck("short", 49, 4, 0)).accepted);
            Assert.IsFalse(MultiplayerLobbyDeckReadiness.Evaluate(CreateDeck("ride", 50, 5, 0)).accepted);
            Assert.IsFalse(MultiplayerLobbyDeckReadiness.Evaluate(CreateDeck("g", 50, 4, 17)).accepted);
        }

        private static VanguardDeck CreateDeck(string prefix, int main, int ride, int g)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "test");
            for (int i = 0; i < main; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i, 1);
            }

            for (int i = 0; i < ride; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i, 1);
            }

            for (int i = 0; i < g; i++)
            {
                deck.AddCard(DeckZone.G, prefix + "-G-" + i, 1);
            }

            return deck;
        }
    }
}
