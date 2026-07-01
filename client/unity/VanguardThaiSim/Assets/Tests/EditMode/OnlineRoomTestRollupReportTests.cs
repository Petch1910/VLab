using NUnit.Framework;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class OnlineRoomTestRollupReportTests
    {
        [Test]
        public void DefaultReportIncludesRequiredM25Categories()
        {
            OnlineRoomTestRollupReport report = OnlineRoomTestRollupReport.CreateDefault();

            Assert.NotNull(Find(report, "deck_identity_privacy"));
            Assert.NotNull(Find(report, "stale_cursor_reject"));
            Assert.NotNull(Find(report, "reconnect_display"));
            Assert.NotNull(Find(report, "masked_event_delivery"));
        }

        [Test]
        public void DefaultReportRoundTripsWithoutDeckCodeLeakText()
        {
            OnlineRoomTestRollupReport report = OnlineRoomTestRollupReport.CreateDefault();

            string json = report.ToJson(false);
            OnlineRoomTestRollupReport roundTrip = OnlineRoomTestRollupReport.FromJson(json);

            Assert.AreEqual(report.items.Count, roundTrip.items.Count);
            Assert.IsFalse(json.Contains("VGTH1"));
            Assert.IsFalse(json.Contains("revealed_deck_code"));
            Assert.IsFalse(json.Contains("deck_code"));
        }

        [Test]
        public void EachCategoryListsAtLeastOneCoveringTest()
        {
            OnlineRoomTestRollupReport report = OnlineRoomTestRollupReport.CreateDefault();

            foreach (OnlineRoomTestRollupItem item in report.items)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.category_id));
                Assert.IsNotNull(item.covering_tests);
                Assert.Greater(item.covering_tests.Count, 0, item.category_id);
            }
        }

        private static OnlineRoomTestRollupItem Find(OnlineRoomTestRollupReport report, string categoryId)
        {
            foreach (OnlineRoomTestRollupItem item in report.items)
            {
                if (item.category_id == categoryId)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
