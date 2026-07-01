using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class OnlineRoomTestRollupReport
    {
        public string suite_status = "covered";
        public List<OnlineRoomTestRollupItem> items = new List<OnlineRoomTestRollupItem>();

        public static OnlineRoomTestRollupReport CreateDefault()
        {
            OnlineRoomTestRollupReport report = new OnlineRoomTestRollupReport();
            report.items.Add(Item(
                "deck_identity_privacy",
                "Lobby and reveal status do not expose deck codes or revealed deck codes.",
                "MultiplayerLobbyStatusFormatterTests.RoomSummaryDoesNotExposeDeckCode",
                "MultiplayerLobbyStatusFormatterTests.RevealSummaryDoesNotExposeRevealedDeckCode"));
            report.items.Add(Item(
                "stale_cursor_reject",
                "Command envelope cursor and ownership checks reject stale/out-of-turn commands without mutating state.",
                "NetworkCommandEnvelopeTests.StateValidatorRejectsStaleCursorWithoutMutatingState",
                "NetworkCommandEnvelopeTests.StateValidatorRejectsOutOfTurnAndActorMismatch"));
            report.items.Add(Item(
                "reconnect_display",
                "Reconnect request, batch, room mismatch, cursor gap, and Start Table rejection text are player-facing.",
                "MultiplayerLobbyStatusFormatterTests.ReconnectSummaryShowsRequestAndBatchCounts",
                "MultiplayerLobbyStatusFormatterTests.ReconnectSummaryExplainsCursorGap",
                "MultiplayerLobbyStatusFormatterTests.StartTableRejectionFormatsReconnectCursorMismatch"));
            report.items.Add(Item(
                "masked_event_delivery",
                "Public events and spectator replay sync do not leak private draw identities.",
                "MultiplayerProtocolTests.PublicReplayFromTrueEventsDoesNotLeakPrivateDrawIdentity",
                "NetworkPublicGameReplayPlayerTests.StepForwardAppliesPublicEventsIntoSpectatorState",
                "NetworkPublicGameReplayPlayerTests.VisibleEventLogReturnsClonedPublicEvents"));
            return report;
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static OnlineRoomTestRollupReport FromJson(string json)
        {
            OnlineRoomTestRollupReport report = JsonUtility.FromJson<OnlineRoomTestRollupReport>(json);
            if (report == null)
            {
                throw new ArgumentException("Online room test rollup report JSON could not be parsed.", nameof(json));
            }

            report.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (items == null)
            {
                items = new List<OnlineRoomTestRollupItem>();
            }
        }

        private static OnlineRoomTestRollupItem Item(string categoryId, string summary, params string[] tests)
        {
            OnlineRoomTestRollupItem item = new OnlineRoomTestRollupItem
            {
                category_id = categoryId,
                summary = summary
            };
            if (tests != null)
            {
                item.covering_tests.AddRange(tests);
            }

            return item;
        }
    }

    [Serializable]
    public sealed class OnlineRoomTestRollupItem
    {
        public string category_id;
        public string summary;
        public List<string> covering_tests = new List<string>();
    }
}
