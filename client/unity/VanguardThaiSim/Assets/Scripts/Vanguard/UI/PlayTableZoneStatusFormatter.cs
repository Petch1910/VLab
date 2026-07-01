using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableZoneStatusFormatter
    {
        public const string MissingPlayerText = "Zone status unavailable.";

        public static string Format(PlayerGameState player)
        {
            if (player == null)
            {
                return MissingPlayerText;
            }

            player.EnsureLists();
            return "Deck: " + Count(player, GameZone.Deck) +
                   " | Hand: " + Count(player, GameZone.Hand) +
                   " | Drop: " + Count(player, GameZone.Drop) +
                   "\nDamage: " + Count(player, GameZone.Damage) +
                   " | Bind: " + Count(player, GameZone.Bind) +
                   " | Order: " + Count(player, GameZone.Order) +
                   "\nRide Deck: " + Count(player, GameZone.RideDeck) +
                   " | Trigger Zone: " + Count(player, GameZone.Trigger) +
                   "\nSoul: " + Count(player, GameZone.Soul) +
                   " | G Zone: " + Count(player, GameZone.GZone) +
                   " | Guardian: " + Count(player, GameZone.Guardian);
        }

        private static int Count(PlayerGameState player, GameZone zone)
        {
            return player.GetZone(zone).Count;
        }
    }
}
