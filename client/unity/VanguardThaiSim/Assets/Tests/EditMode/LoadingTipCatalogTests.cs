using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class LoadingTipCatalogTests
    {
        [Test]
        public void KnownContextsReturnPlayerFacingTips()
        {
            StringAssert.Contains("pack", LoadingTipCatalog.Get(LoadingTipCatalog.DataReload));
            StringAssert.Contains("fallback", LoadingTipCatalog.Get(LoadingTipCatalog.CardImages));
            StringAssert.Contains("legality", LoadingTipCatalog.Get(LoadingTipCatalog.DeckLoad));
        }

        [Test]
        public void AppendTipCompactsMessageAndAddsTipPrefix()
        {
            string formatted = LoadingTipCatalog.AppendTip(
                "Startup\n data\t reloaded.",
                LoadingTipCatalog.DataReload);

            StringAssert.Contains("Startup data reloaded.", formatted);
            StringAssert.Contains("Tip:", formatted);
            StringAssert.Contains("validation status", formatted);
        }
    }
}
