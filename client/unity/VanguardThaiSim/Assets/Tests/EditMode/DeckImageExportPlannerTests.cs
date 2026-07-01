using System;
using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class DeckImageExportPlannerTests
    {
        [Test]
        public void NullDeckRejects()
        {
            DeckImageExportPlan plan = DeckImageExportPlanner.CreatePlan(
                null,
                "C:/tmp/deck_exports",
                new DateTime(2026, 6, 28, 6, 0, 0));

            Assert.IsFalse(plan.accepted);
            StringAssert.Contains("Deck is not ready", plan.rejection_reason);
        }

        [Test]
        public void SafeFileNameRemovesUnsafeCharacters()
        {
            string safe = DeckImageExportPlanner.SafeFileName(" My Deck: ../Bad? Name ");

            Assert.AreEqual("my_deck_bad_name", safe);
        }

        [Test]
        public void PlanCreatesPngInsideExportRoot()
        {
            VanguardDeck deck = VanguardDeck.Create("Royal Build", "D", "vanguard_th", "2026.06");
            DeckImageExportPlan plan = DeckImageExportPlanner.CreatePlan(
                deck,
                "C:/tmp/deck_exports",
                new DateTime(2026, 6, 28, 6, 1, 2));

            Assert.IsTrue(plan.accepted, plan.rejection_reason);
            Assert.AreEqual("deck_royal_build_20260628_060102.png", plan.file_name);
            StringAssert.EndsWith("/deck_exports/deck_royal_build_20260628_060102.png", plan.file_path.Replace("\\", "/"));
        }

        [Test]
        public void StatusFormatsAcceptedAndRejected()
        {
            VanguardDeck deck = VanguardDeck.Create("Deck", "D", "pack", "version");
            DeckImageExportPlan accepted = DeckImageExportPlanner.CreatePlan(
                deck,
                "C:/tmp/deck_exports",
                new DateTime(2026, 6, 28, 6, 1, 2));
            DeckImageExportPlan rejected = DeckImageExportPlanner.CreatePlan(
                null,
                "C:/tmp/deck_exports",
                new DateTime(2026, 6, 28, 6, 1, 2));

            StringAssert.Contains("OK [Export Image]", DeckImageExportPlanner.FormatPlanStatus(accepted));
            StringAssert.Contains(".png", DeckImageExportPlanner.FormatPlanStatus(accepted));
            StringAssert.Contains("Rejected [Export Image]", DeckImageExportPlanner.FormatPlanStatus(rejected));
        }
    }
}

