using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftStatusMessageFormatterTests
    {
        [Test]
        public void TriggerTypeStatusMatchesExistingText()
        {
            Assert.AreEqual(
                "Draft trigger type: Critical.",
                TriggerCheckDraftStatusMessageFormatter.FormatTriggerTypeChanged(TriggerType.Critical));
        }

        [Test]
        public void CheckSourceStatusMatchesExistingText()
        {
            Assert.AreEqual(
                "Draft check source: Drive.",
                TriggerCheckDraftStatusMessageFormatter.FormatCheckSourceChanged(TriggerCheckSource.Drive));
        }

        [Test]
        public void CheckIndexStatusMatchesExistingText()
        {
            Assert.AreEqual(
                "Draft check index: 1.",
                TriggerCheckDraftStatusMessageFormatter.FormatCheckIndexChanged(1));
        }

        [Test]
        public void ClearSelectionStatusMatchesExistingText()
        {
            Assert.AreEqual(
                "Draft selection cleared.",
                TriggerCheckDraftStatusMessageFormatter.FormatSelectionCleared());
        }
    }
}
