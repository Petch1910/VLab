namespace VanguardThaiSim.UI
{
    public static class PlayTableActionGroupLegendFormatter
    {
        public const string LegendText =
            "Turn: Stand / Draw / Ride / Main / Battle / End\n" +
            "Card: select card -> VG / Rear / Drop / Damage\n" +
            "Battle: Atk VG / Atk Target / Guard / Drive / Damage Check";

        public static string Format()
        {
            return LegendText;
        }
    }
}
