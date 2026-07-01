using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class BotExplanationPanelFormatterTests
    {
        [Test]
        public void EmptyTraceFormatsPlayerFacingPlaceholder()
        {
            Assert.AreEqual(
                BotExplanationPanelFormatter.EmptyMessage,
                BotExplanationPanelFormatter.Format(null));
        }

        [Test]
        public void TraceFormatsChoiceWithoutDeveloperScoresOrIds()
        {
            BotDebugTrace trace = new BotDebugTrace
            {
                candidate_count = 2,
                playbook_id = "youth_line",
                selected_action_summary =
                    "MoveCard Hand->RearGuard playbook=youth_line base=5 bias=3 total=8 card=CALL-A",
                lines = new List<BotDebugTraceLine>
                {
                    new BotDebugTraceLine
                    {
                        selected = true,
                        action_summary = "MoveCard Hand->RearGuard playbook=youth_line base=5 bias=3 total=8 card=CALL-A"
                    },
                    new BotDebugTraceLine
                    {
                        selected = false,
                        action_summary = "SetPhase Battle playbook=youth_line base=1 bias=0.4 total=1.4"
                    }
                }
            };

            string text = BotExplanationPanelFormatter.Format(trace);

            Assert.IsTrue(text.Contains("Choice: Move a card from hand to rear-guard."));
            Assert.IsTrue(text.Contains("Why: compared 2 legal option(s)."));
            Assert.IsTrue(text.Contains("Style: youth line"));
            Assert.IsTrue(text.Contains("* Move a card from hand to rear-guard."));
            Assert.IsTrue(text.Contains("- Change to battle phase."));
            Assert.IsFalse(text.Contains("playbook="));
            Assert.IsFalse(text.Contains("base="));
            Assert.IsFalse(text.Contains("bias="));
            Assert.IsFalse(text.Contains("total="));
            Assert.IsFalse(text.Contains("CALL-A"));
        }

        [Test]
        public void FormatterLimitsOptionCount()
        {
            BotDebugTrace trace = new BotDebugTrace
            {
                candidate_count = 3,
                selected_action_summary = "Draw",
                lines = new List<BotDebugTraceLine>
                {
                    new BotDebugTraceLine { selected = true, action_summary = "Draw" },
                    new BotDebugTraceLine { action_summary = "SetPhase Battle" },
                    new BotDebugTraceLine { action_summary = "SetPhase End" }
                }
            };

            string text = BotExplanationPanelFormatter.Format(trace, 2);

            Assert.IsTrue(text.Contains("* Draw a card."));
            Assert.IsTrue(text.Contains("- Change to battle phase."));
            Assert.IsFalse(text.Contains("Change to end phase."));
        }
    }
}
