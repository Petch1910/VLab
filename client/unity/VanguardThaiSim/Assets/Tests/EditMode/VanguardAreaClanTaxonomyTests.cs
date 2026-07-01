using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Cards;

namespace VanguardThaiSim.Tests
{
    public sealed class VanguardAreaClanTaxonomyTests
    {
        [Test]
        public void ClassicClanOrderBeatsRawCardCountOrder()
        {
            IReadOnlyList<CardTaxonomyFilterOption> options =
                VanguardAreaClanTaxonomy.BuildFilterOptions(
                    new List<ClanOption>
                    {
                        new ClanOption("N/A", 1888),
                        new ClanOption("\u0e23\u0e2d\u0e22\u0e31\u0e25 \u0e1e\u0e32\u0e25\u0e32\u0e14\u0e34\u0e19", 556),
                        new ClanOption("\u0e04\u0e32\u0e40\u0e07\u0e42\u0e23\u0e48", 436)
                    },
                    new List<NationOption>());

            Assert.AreEqual("\u0e23\u0e2d\u0e22\u0e31\u0e25 \u0e1e\u0e32\u0e25\u0e32\u0e14\u0e34\u0e19", options[0].Value);
            Assert.IsTrue(options[0].DisplayLabel.StartsWith("US - "));
            Assert.AreEqual("\u0e04\u0e32\u0e40\u0e07\u0e42\u0e23\u0e48", options[1].Value);
            Assert.AreEqual("N/A", options[2].Value);
        }

        [Test]
        public void DNationsBecomeNationFilters()
        {
            IReadOnlyList<CardTaxonomyFilterOption> options =
                VanguardAreaClanTaxonomy.BuildFilterOptions(
                    new List<ClanOption>(),
                    new List<NationOption>
                    {
                        new NationOption("\u0e14\u0e23\u0e32\u0e01\u0e49\u0e2d\u0e19\u0e40\u0e2d\u0e21\u0e44\u0e1e\u0e23\u0e4c D", 259),
                        new NationOption("\u0e14\u0e23\u0e32\u0e01\u0e49\u0e2d\u0e19\u0e40\u0e2d\u0e21\u0e44\u0e1e\u0e23\u0e4c", 1513)
                    });

            Assert.AreEqual(1, options.Count);
            Assert.IsTrue(options[0].IsNation);
            Assert.AreEqual("\u0e14\u0e23\u0e32\u0e01\u0e49\u0e2d\u0e19\u0e40\u0e2d\u0e21\u0e44\u0e1e\u0e23\u0e4c D", options[0].Value);
            Assert.IsTrue(options[0].DisplayLabel.StartsWith("D - "));
        }

        [Test]
        public void NullInputsReturnEmptyList()
        {
            Assert.AreEqual(0, VanguardAreaClanTaxonomy.BuildFilterOptions(null, null).Count);
        }
    }
}
