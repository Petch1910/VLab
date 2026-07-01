using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class MultiplayerPayloadNoLeakSuiteReport
    {
        public int schema_version = 1;
        public string milestone = "M18-07";
        public string suite_status = "inventory_ready";
        public int category_count;
        public int representative_test_count;
        public List<MultiplayerPayloadNoLeakSuiteCategory> categories =
            new List<MultiplayerPayloadNoLeakSuiteCategory>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static MultiplayerPayloadNoLeakSuiteReport FromJson(string json)
        {
            MultiplayerPayloadNoLeakSuiteReport report =
                JsonUtility.FromJson<MultiplayerPayloadNoLeakSuiteReport>(json);
            report?.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (categories == null)
            {
                categories = new List<MultiplayerPayloadNoLeakSuiteCategory>();
            }

            foreach (MultiplayerPayloadNoLeakSuiteCategory category in categories)
            {
                category?.EnsureLists();
            }
        }
    }

    [Serializable]
    public sealed class MultiplayerPayloadNoLeakSuiteCategory
    {
        public string id;
        public string description;
        public List<string> representative_tests = new List<string>();

        public void EnsureLists()
        {
            if (representative_tests == null)
            {
                representative_tests = new List<string>();
            }
        }
    }

    [Serializable]
    public sealed class MultiplayerPayloadNoLeakSuiteValidationResult
    {
        public bool accepted;
        public List<string> errors = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    public static class MultiplayerPayloadNoLeakSuiteReportBuilder
    {
        private static readonly string[] RequiredCategories =
        {
            "command_envelope_cursor",
            "owner_private_room_state",
            "public_event_masking",
            "public_reconnect_recovery",
            "spectator_replay_sync",
            "trigger_check_payload",
            "pending_auto_queue_payload",
            "pending_auto_resolution_request_payload",
            "manual_resolution_decision_payload",
            "session_storage_no_mutation"
        };

        public static MultiplayerPayloadNoLeakSuiteReport CreateCurrent()
        {
            MultiplayerPayloadNoLeakSuiteReport report = new MultiplayerPayloadNoLeakSuiteReport();
            report.categories.Add(Category(
                "command_envelope_cursor",
                "Online command envelope identity, sequence, room id, cursor, and stale/out-of-turn rejection.",
                "NetworkCommandEnvelopeTests.FactoryCapturesPlayerSequenceGameIdAndCursor",
                "NetworkCommandEnvelopeTests.JsonAndPhotonPayloadRoundTrip",
                "NetworkCommandEnvelopeTests.StateValidatorRejectsStaleCursorWithoutMutatingState",
                "NetworkCommandEnvelopeTests.StateValidatorRejectsOutOfTurnAndActorMismatch",
                "NetworkCommandEnvelopeTests.StateValidatorRejectsPlayerOwnershipMismatch"));
            report.categories.Add(Category(
                "owner_private_room_state",
                "Owner-private initialization keeps each local true state private and uses placeholders for opponent hidden zones.",
                "OwnerPrivateRoomInitializationTests.CreatesLocalTrueStateWithoutOpponentDeckIdentities",
                "OwnerPrivateRoomInitializationTests.CreatesPlayerTwoLocalSessionWithOpponentAsPlaceholder",
                "OwnerPrivateRoomInitializationTests.RejectsCommitmentMismatchWithoutMutatingInputs",
                "OwnerPrivateRoomInitializationTests.RejectsOpponentMissingPublicCountMetadata"));
            report.categories.Add(Category(
                "public_event_masking",
                "True events become public events without leaking private draw identity, while public reveals keep valid proof metadata.",
                "MultiplayerProtocolTests.PublicEventFactoryMasksPrivateDraw",
                "MultiplayerProtocolTests.PublicEventFactoryRevealsCardEnteringPublicZone",
                "MultiplayerProtocolTests.PublicEventFactoryAddsCommitmentRevealProofWhenContextExists",
                "MultiplayerProtocolTests.PublicReplayFromTrueEventsDoesNotLeakPrivateDrawIdentity",
                "NetworkPublicGameEventApplierTests.HiddenDrawUpdatesCountsWithoutCardIdentityLeak",
                "NetworkPublicGameEventApplierTests.PrivateToPublicRevealAddsOnlyPublicIdentity"));
            report.categories.Add(Category(
                "public_reconnect_recovery",
                "Reconnect recovery uses public batches for owner-private sessions and rejects cursor mismatches without mutation.",
                "NetworkPublicReconnectRecoveryTests.CreatesPublicBatchFromCursorAndRoundTripsPhotonPayload",
                "NetworkPublicReconnectRecoveryTests.AppliesPublicBatchToOwnerPrivateSession",
                "NetworkPublicReconnectRecoveryTests.CursorMismatchRejectsWithoutMutatingSession",
                "NetworkPublicReconnectRecoveryTests.CommitmentSessionDoesNotCreateTrueReconnectEvents"));
            report.categories.Add(Category(
                "spectator_replay_sync",
                "Spectator replay sync applies public events only and keeps visible event logs cloned.",
                "MultiplayerProtocolTests.PublicReplayPlayerStepsVisibleEventLog",
                "NetworkPublicGameReplayPlayerTests.StepForwardAppliesPublicEventsIntoSpectatorState",
                "NetworkPublicGameReplayPlayerTests.ApplyBatchAppendsAndAppliesFromCurrentPublicCursor",
                "NetworkPublicGameReplayPlayerTests.ApplyBatchRejectsCursorMismatchWithoutMutatingReplayState",
                "NetworkPublicGameReplayPlayerTests.VisibleEventLogReturnsClonedPublicEvents"));
            report.categories.Add(Category(
                "trigger_check_payload",
                "Trigger-check draft/replay payloads are masked for spectators and transported without mutating source logs or game state.",
                "ManualTriggerCheckDraftPayloadBuilderTests.DraftPayloadMasksSpectatorViewAndDoesNotMutateState",
                "TriggerCheckReplayLogPayloadCodecTests.PayloadRoundTripsMaskedReplayLog",
                "TriggerCheckReplayLogPayloadCodecTests.EncodingDoesNotMutateSourceLog",
                "TriggerCheckPhotonPayloadWrapperTests.PhotonWrappingDoesNotMutateSourceLog",
                "TriggerCheckTransportHookTests.TransportHookDoesNotMutateGameState"));
            report.categories.Add(Category(
                "pending_auto_queue_payload",
                "Pending AUTO queue payloads are masked, deterministic, transportable, and no-mutation.",
                "PendingAutoAbilityQueuePayloadCodecTests.PayloadRoundTripsMaskedPendingQueue",
                "PendingAutoAbilityQueuePayloadCodecTests.EncodingDoesNotMutateSourceQueue",
                "PendingAutoAbilityQueuePayloadCodecTests.EncodingNullPendingListDoesNotMutateSourceQueue",
                "PendingAutoAbilityQueuePhotonPayloadWrapperTests.PhotonWrappingDoesNotMutateSourcePayload",
                "PendingAutoAbilityQueueTransportHookTests.TransportHookDoesNotMutateGameState"));
            report.categories.Add(Category(
                "pending_auto_resolution_request_payload",
                "Pending AUTO resolution request payloads sanitize hidden source data and do not mutate source request/state.",
                "PendingAutoAbilityResolutionRequestPayloadCodecTests.PayloadRoundTripsResolutionRequest",
                "PendingAutoAbilityResolutionRequestPayloadCodecTests.HiddenRequestIsSanitizedWithoutMutatingSource",
                "PendingAutoAbilityResolutionRequestPhotonPayloadWrapperTests.PhotonWrappingDoesNotMutateSourcePayload",
                "PendingAutoAbilityResolutionRequestTransportHookTests.TransportHookDoesNotMutateGameState"));
            report.categories.Add(Category(
                "manual_resolution_decision_payload",
                "Manual resolution decision payloads sanitize hidden source data, wrap for Photon, and keep source data immutable.",
                "PendingAutoAbilityManualResolutionDecisionPayloadCodecTests.VisibleDecisionPayloadRoundTrips",
                "PendingAutoAbilityManualResolutionDecisionPayloadCodecTests.HiddenSourceIsSanitized",
                "PendingAutoAbilityManualResolutionDecisionPayloadCodecTests.EncodingDoesNotMutateSourceDecision",
                "PendingAutoAbilityManualResolutionDecisionPhotonPayloadWrapperTests.PhotonWrappingDoesNotMutateSourcePayload",
                "PendingAutoAbilityManualResolutionDecisionTransportHookTests.TransportHookDoesNotMutateGameState"));
            report.categories.Add(Category(
                "session_storage_no_mutation",
                "Multiplayer session stores diagnostic payloads outside true GameState and preserves normal event sync.",
                "MultiplayerGameSessionTests.IncomingPublicEventIsStoredWithoutMutatingTrueState",
                "MultiplayerGameSessionTests.IncomingTriggerCheckReplayPayloadIsStoredWithoutMutatingTrueState",
                "MultiplayerGameSessionTests.IncomingPendingAutoAbilityQueuePayloadIsStoredWithoutMutatingTrueState",
                "MultiplayerGameSessionTests.IncomingPendingAutoAbilityResolutionRequestPayloadIsStoredWithoutMutatingTrueState",
                "MultiplayerGameSessionTests.IncomingPendingAutoAbilityManualResolutionDecisionPayloadIsStoredWithoutMutatingTrueState",
                "MultiplayerGameSessionTests.CreatePendingAutoAbilityManualResolutionDecisionDraftKeepsHiddenSourceRedacted"));

            RefreshCounts(report);
            return report;
        }

        public static MultiplayerPayloadNoLeakSuiteValidationResult Validate(
            MultiplayerPayloadNoLeakSuiteReport report)
        {
            MultiplayerPayloadNoLeakSuiteValidationResult result =
                new MultiplayerPayloadNoLeakSuiteValidationResult();
            if (report == null)
            {
                result.errors.Add("report_missing");
                return result;
            }

            report.EnsureLists();
            if (report.schema_version != 1)
            {
                result.errors.Add("schema_version_must_be_1");
            }

            if (report.milestone != "M18-07")
            {
                result.errors.Add("milestone_must_be_M18-07");
            }

            foreach (string required in RequiredCategories)
            {
                MultiplayerPayloadNoLeakSuiteCategory category = FindCategory(report, required);
                if (category == null)
                {
                    result.errors.Add("missing_category_" + required);
                    continue;
                }

                if (category.representative_tests.Count == 0)
                {
                    result.errors.Add("category_has_no_tests_" + required);
                }
            }

            result.accepted = result.errors.Count == 0;
            return result;
        }

        private static MultiplayerPayloadNoLeakSuiteCategory Category(
            string id,
            string description,
            params string[] representativeTests)
        {
            return new MultiplayerPayloadNoLeakSuiteCategory
            {
                id = id,
                description = description,
                representative_tests = new List<string>(representativeTests)
            };
        }

        private static MultiplayerPayloadNoLeakSuiteCategory FindCategory(
            MultiplayerPayloadNoLeakSuiteReport report,
            string id)
        {
            for (int i = 0; i < report.categories.Count; i++)
            {
                MultiplayerPayloadNoLeakSuiteCategory category = report.categories[i];
                if (category != null && category.id == id)
                {
                    return category;
                }
            }

            return null;
        }

        private static void RefreshCounts(MultiplayerPayloadNoLeakSuiteReport report)
        {
            report.EnsureLists();
            report.category_count = report.categories.Count;
            report.representative_test_count = 0;
            foreach (MultiplayerPayloadNoLeakSuiteCategory category in report.categories)
            {
                if (category != null)
                {
                    report.representative_test_count += category.representative_tests.Count;
                }
            }
        }
    }
}
