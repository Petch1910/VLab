using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public sealed class MultiplayerGameSessionController
    {
        private readonly IMultiplayerTransport transport;
        private readonly MultiplayerRoomState room;
        private readonly string localPlayerId;
        private readonly GameState state;
        private readonly List<NetworkPublicGameEvent> publicEventLog = new List<NetworkPublicGameEvent>();
        private readonly List<NetworkTriggerCheckReplayLogPayload> triggerCheckReplayLogPayloads =
            new List<NetworkTriggerCheckReplayLogPayload>();
        private readonly List<NetworkPendingAutoAbilityQueuePayload> pendingAutoAbilityQueuePayloads =
            new List<NetworkPendingAutoAbilityQueuePayload>();
        private readonly List<NetworkPendingAutoAbilityResolutionRequestPayload> pendingAutoAbilityResolutionRequestPayloads =
            new List<NetworkPendingAutoAbilityResolutionRequestPayload>();
        private readonly List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> pendingAutoAbilityManualResolutionDecisionPayloads =
            new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>();
        private readonly List<PendingAutoAbilityManualResolutionApplyPreviewLogEntry> pendingAutoAbilityManualResolutionApplyPreviewLogs =
            new List<PendingAutoAbilityManualResolutionApplyPreviewLogEntry>();

        public MultiplayerGameSessionController(
            IMultiplayerTransport transport,
            MultiplayerRoomState room,
            GameState initialState,
            string localPlayerId)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.room = room ?? throw new ArgumentNullException(nameof(room));
            state = initialState ?? throw new ArgumentNullException(nameof(initialState));
            this.localPlayerId = string.IsNullOrWhiteSpace(localPlayerId) ? "local-player" : localPlayerId;
            this.transport.GameEventReceived += HandleGameEventReceived;
            this.transport.PublicGameEventReceived += HandlePublicGameEventReceived;
            this.transport.TriggerCheckReplayLogReceived += HandleTriggerCheckReplayLogReceived;
            this.transport.PendingAutoAbilityQueueReceived += HandlePendingAutoAbilityQueueReceived;
            this.transport.PendingAutoAbilityResolutionRequestReceived +=
                HandlePendingAutoAbilityResolutionRequestReceived;
            this.transport.PendingAutoAbilityManualResolutionDecisionReceived +=
                HandlePendingAutoAbilityManualResolutionDecisionReceived;
            this.transport.ReconnectRequestReceived += HandleReconnectRequestReceived;
            this.transport.ReconnectBatchReceived += HandleReconnectBatchReceived;
        }

        public GameState State
        {
            get { return state; }
        }

        public MultiplayerTransportStatus Status
        {
            get { return transport.Status; }
        }

        public string TransportName
        {
            get { return transport.TransportName; }
        }

        public int LocalPlayerIndex
        {
            get
            {
                return string.Equals(room.host_player_id ?? "", localPlayerId ?? "", StringComparison.Ordinal) ? 0 : 1;
            }
        }

        public int EventCursor
        {
            get
            {
                state.EnsureLists();
                return state.event_log.Count;
            }
        }

        public string LastMessage { get; private set; }

        public IReadOnlyList<NetworkPublicGameEvent> PublicEventLog
        {
            get { return publicEventLog; }
        }

        public IReadOnlyList<NetworkTriggerCheckReplayLogPayload> TriggerCheckReplayLogPayloads
        {
            get { return triggerCheckReplayLogPayloads; }
        }

        public IReadOnlyList<NetworkPendingAutoAbilityQueuePayload> PendingAutoAbilityQueuePayloads
        {
            get { return pendingAutoAbilityQueuePayloads; }
        }

        public IReadOnlyList<NetworkPendingAutoAbilityResolutionRequestPayload> PendingAutoAbilityResolutionRequestPayloads
        {
            get { return pendingAutoAbilityResolutionRequestPayloads; }
        }

        public IReadOnlyList<NetworkPendingAutoAbilityManualResolutionDecisionPayload> PendingAutoAbilityManualResolutionDecisionPayloads
        {
            get { return pendingAutoAbilityManualResolutionDecisionPayloads; }
        }

        public IReadOnlyList<PendingAutoAbilityManualResolutionApplyPreviewLogEntry> PendingAutoAbilityManualResolutionApplyPreviewLogs
        {
            get { return pendingAutoAbilityManualResolutionApplyPreviewLogs; }
        }

        public PendingAutoAbilityManualResolutionApplyPreviewLogEntry LatestPendingAutoAbilityManualResolutionApplyPreviewLog
        {
            get
            {
                return pendingAutoAbilityManualResolutionApplyPreviewLogs.Count == 0
                    ? null
                    : pendingAutoAbilityManualResolutionApplyPreviewLogs[
                        pendingAutoAbilityManualResolutionApplyPreviewLogs.Count - 1];
            }
        }

        public bool CanPublishTriggerCheckReplayLog
        {
            get { return triggerCheckReplayLogPayloads.Count > 0; }
        }

        public bool CanPublishPendingAutoAbilityQueue
        {
            get { return pendingAutoAbilityQueuePayloads.Count > 0; }
        }

        public bool CanPublishPendingAutoAbilityResolutionRequest
        {
            get { return pendingAutoAbilityResolutionRequestPayloads.Count > 0; }
        }

        public bool CanPublishPendingAutoAbilityManualResolutionDecision
        {
            get { return pendingAutoAbilityManualResolutionDecisionPayloads.Count > 0; }
        }

        public bool CanCreatePendingAutoAbilityManualResolutionDecisionDraft
        {
            get { return pendingAutoAbilityResolutionRequestPayloads.Count > 0; }
        }

        public int LastReconnectAppliedCount { get; private set; }

        public int LastReconnectFromEventIndex { get; private set; } = -1;

        public event Action StateChanged;

        public void Tick()
        {
            transport.Tick();
        }

        public RulesCommandResult ExecuteLocalAction(LegalGameAction action)
        {
            bool sendTrueEvent = ShouldSendTrueGameEvents();
            GameState stateBeforeEvent = sendTrueEvent ? null : GameState.FromJson(state.ToJson(false));
            RulesCommandResult result = RulesCore.TryExecute(state, action);
            if (!result.accepted)
            {
                LastMessage = result.rejection_reason;
                return result;
            }

            if (sendTrueEvent)
            {
                NetworkEventEnvelope envelope = MultiplayerProtocol.CreateEnvelope(room, localPlayerId, state, result.game_event);
                MultiplayerTransportResult sendResult = transport.SendGameEvent(envelope);
                LastMessage = sendResult.ok
                    ? "Published event " + envelope.event_index + "."
                    : sendResult.error_code + ": " + sendResult.message;
            }
            else
            {
                NetworkPublicGameEvent publicEvent = NetworkPublicGameEventFactory.Create(
                    stateBeforeEvent,
                    result.game_event,
                    FindRoomPlayer(localPlayerId),
                    room.room_id,
                    room.pack == null ? "" : room.pack.definition_hash);
                publicEventLog.Add(publicEvent);
                MultiplayerTransportResult sendResult = transport.SendPublicGameEvent(publicEvent);
                LastMessage = sendResult.ok
                    ? "Published public event " + publicEvent.event_id + "."
                    : sendResult.error_code + ": " + sendResult.message;
            }

            RaiseStateChanged();
            return result;
        }

        public bool TryApplyEnvelope(NetworkEventEnvelope envelope, out string rejectionReason)
        {
            bool accepted = MultiplayerProtocol.TryApplyEnvelope(state, envelope, out rejectionReason);
            LastMessage = accepted ? "Applied event " + envelope.event_index + "." : rejectionReason;
            if (accepted)
            {
                RaiseStateChanged();
            }

            return accepted;
        }

        public NetworkEventBatch CreateReconnectBatch(int fromEventIndex)
        {
            if (!ShouldSendTrueGameEvents())
            {
                LastMessage = NetworkPublicReconnectRecovery.CommitmentTrueReconnectBlocked;
                return new NetworkEventBatch
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    room_id = room.room_id,
                    from_event_index = Math.Max(0, fromEventIndex)
                };
            }

            return MultiplayerProtocol.CreateReconnectBatch(room, localPlayerId, state, fromEventIndex);
        }

        public MultiplayerTransportResult PublishLatestTriggerCheckReplayLog()
        {
            if (triggerCheckReplayLogPayloads.Count == 0)
            {
                LastMessage = "TRIGGER_CHECK_REPLAY_LOG_MISSING";
                return MultiplayerTransportResult.Fail(
                    "TRIGGER_CHECK_REPLAY_LOG_MISSING",
                    "No trigger check replay log payload is available to publish.");
            }

            NetworkTriggerCheckReplayLogPayload payload =
                triggerCheckReplayLogPayloads[triggerCheckReplayLogPayloads.Count - 1];
            MultiplayerTransportResult result = transport.SendTriggerCheckReplayLog(payload);
            LastMessage = result.ok
                ? "Published trigger check replay log " + payload.payload_id + "."
                : result.error_code + ": " + result.message;
            RaiseStateChanged();
            return result;
        }

        public MultiplayerTransportResult PublishLatestPendingAutoAbilityQueue()
        {
            if (pendingAutoAbilityQueuePayloads.Count == 0)
            {
                LastMessage = "PENDING_AUTO_ABILITY_QUEUE_MISSING";
                return MultiplayerTransportResult.Fail(
                    "PENDING_AUTO_ABILITY_QUEUE_MISSING",
                    "No pending auto ability queue payload is available to publish.");
            }

            NetworkPendingAutoAbilityQueuePayload payload =
                pendingAutoAbilityQueuePayloads[pendingAutoAbilityQueuePayloads.Count - 1];
            MultiplayerTransportResult result = transport.SendPendingAutoAbilityQueue(payload);
            LastMessage = result.ok
                ? "Published pending auto ability queue " + payload.payload_id + "."
                : result.error_code + ": " + result.message;
            RaiseStateChanged();
            return result;
        }

        public MultiplayerTransportResult PublishLatestPendingAutoAbilityResolutionRequest()
        {
            if (pendingAutoAbilityResolutionRequestPayloads.Count == 0)
            {
                LastMessage = "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING";
                return MultiplayerTransportResult.Fail(
                    "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING",
                    "No pending auto ability resolution request payload is available to publish.");
            }

            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                pendingAutoAbilityResolutionRequestPayloads[pendingAutoAbilityResolutionRequestPayloads.Count - 1];
            MultiplayerTransportResult result = transport.SendPendingAutoAbilityResolutionRequest(payload);
            LastMessage = result.ok
                ? "Published pending auto ability resolution request " + payload.payload_id + "."
                : result.error_code + ": " + result.message;
            RaiseStateChanged();
            return result;
        }

        public MultiplayerTransportResult PublishLatestPendingAutoAbilityManualResolutionDecision()
        {
            if (pendingAutoAbilityManualResolutionDecisionPayloads.Count == 0)
            {
                LastMessage = "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_MISSING";
                return MultiplayerTransportResult.Fail(
                    "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_MISSING",
                    "No pending auto ability manual resolution decision payload is available to publish.");
            }

            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                pendingAutoAbilityManualResolutionDecisionPayloads[
                    pendingAutoAbilityManualResolutionDecisionPayloads.Count - 1];
            MultiplayerTransportResult result = transport.SendPendingAutoAbilityManualResolutionDecision(payload);
            LastMessage = result.ok
                ? "Published pending auto ability manual resolution decision " + payload.payload_id + "."
                : result.error_code + ": " + result.message;
            RaiseStateChanged();
            return result;
        }

        public MultiplayerTransportResult PublishPendingAutoAbilityResolutionRequest(
            PendingAutoAbilityResolutionRequest request)
        {
            if (request == null)
            {
                LastMessage = "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING";
                return MultiplayerTransportResult.Fail(
                    "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING",
                    "No pending auto ability resolution request is available to publish.");
            }

            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                    room.room_id,
                    LocalPlayerIndex,
                    request,
                    GameStateViewPerspective.Player,
                    LocalPlayerIndex);
            MultiplayerTransportResult result = transport.SendPendingAutoAbilityResolutionRequest(payload);
            LastMessage = result.ok
                ? "Published pending auto ability resolution request " + payload.payload_id + "."
                : result.error_code + ": " + result.message;
            if (result.ok)
            {
                pendingAutoAbilityResolutionRequestPayloads.Add(payload);
                RaiseStateChanged();
            }

            return result;
        }

        public PendingAutoAbilityManualResolutionDecisionDraftResult CreatePendingAutoAbilityManualResolutionDecisionDraft(
            string decisionType,
            string reason = "")
        {
            if (pendingAutoAbilityResolutionRequestPayloads.Count == 0)
            {
                LastMessage = PendingAutoAbilityManualResolutionDecisionFactory.RequestMissingReason;
                return RejectPendingAutoAbilityManualResolutionDecisionDraft(
                    PendingAutoAbilityManualResolutionDecisionFactory.RequestMissingReason);
            }

            NetworkPendingAutoAbilityResolutionRequestPayload requestPayload =
                pendingAutoAbilityResolutionRequestPayloads[pendingAutoAbilityResolutionRequestPayloads.Count - 1];
            PendingAutoAbilityResolutionRequest request;
            string rejectionReason;
            if (!PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                    requestPayload,
                    out request,
                    out rejectionReason))
            {
                LastMessage = rejectionReason;
                return RejectPendingAutoAbilityManualResolutionDecisionDraft(rejectionReason);
            }

            PendingAutoAbilityManualResolutionDecisionDraftResult draft =
                PendingAutoAbilityManualResolutionDecisionDraftFactory.Create(
                    room.room_id,
                    LocalPlayerIndex,
                    request,
                    decisionType,
                    reason,
                    GameStateViewPerspective.Player,
                    LocalPlayerIndex);
            if (!draft.accepted)
            {
                LastMessage = draft.rejection_reason;
                return draft;
            }

            pendingAutoAbilityManualResolutionDecisionPayloads.Add(draft.payload);
            LastMessage = "Created pending auto ability manual resolution decision draft " +
                          draft.payload.payload_id +
                          ".";
            RaiseStateChanged();
            return draft;
        }

        public PendingAutoAbilityManualResolutionApplyExecutorResult PreviewApplyLatestPendingAutoAbilityManualResolutionDecision()
        {
            if (pendingAutoAbilityQueuePayloads.Count == 0)
            {
                LastMessage = PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing;
                PendingAutoAbilityManualResolutionApplyExecutorResult rejected =
                    RejectPendingAutoAbilityManualResolutionApply(
                        PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing,
                        null);
                AppendPendingAutoAbilityManualResolutionApplyPreviewLog(rejected.apply_result);
                RaiseStateChanged();
                return rejected;
            }

            if (pendingAutoAbilityManualResolutionDecisionPayloads.Count == 0)
            {
                LastMessage = PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing;
                PendingAutoAbilityManualResolutionApplyExecutorResult rejected =
                    RejectPendingAutoAbilityManualResolutionApply(
                        PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing,
                        null);
                AppendPendingAutoAbilityManualResolutionApplyPreviewLog(rejected.apply_result);
                RaiseStateChanged();
                return rejected;
            }

            PendingAutoAbilityQueue queue;
            string rejectionReason;
            if (!PendingAutoAbilityQueuePayloadCodec.TryDecode(
                    pendingAutoAbilityQueuePayloads[pendingAutoAbilityQueuePayloads.Count - 1],
                    out queue,
                    out rejectionReason))
            {
                LastMessage = rejectionReason;
                PendingAutoAbilityManualResolutionApplyExecutorResult rejected =
                    RejectPendingAutoAbilityManualResolutionApply(rejectionReason, null);
                AppendPendingAutoAbilityManualResolutionApplyPreviewLog(rejected.apply_result);
                RaiseStateChanged();
                return rejected;
            }

            PendingAutoAbilityManualResolutionDecisionValidationResult validation =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(
                    pendingAutoAbilityManualResolutionDecisionPayloads[
                        pendingAutoAbilityManualResolutionDecisionPayloads.Count - 1]);
            if (!validation.accepted)
            {
                LastMessage = validation.rejection_reason;
                PendingAutoAbilityManualResolutionApplyExecutorResult rejected =
                    RejectPendingAutoAbilityManualResolutionApply(validation.rejection_reason, queue);
                AppendPendingAutoAbilityManualResolutionApplyPreviewLog(rejected.apply_result);
                RaiseStateChanged();
                return rejected;
            }

            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                PendingAutoAbilityManualResolutionApplyExecutor.Apply(queue, validation.decision);
            LastMessage = result.apply_result != null && result.apply_result.accepted
                ? result.apply_result.summary
                : result.rejection_reason;
            AppendPendingAutoAbilityManualResolutionApplyPreviewLog(result.apply_result);
            if (result.accepted)
            {
                pendingAutoAbilityQueuePayloads.Add(
                    PendingAutoAbilityQueuePayloadCodec.Encode(
                        room.room_id,
                        LocalPlayerIndex,
                        result.queue,
                        GameStateViewPerspective.Player,
                        LocalPlayerIndex));
            }

            RaiseStateChanged();
            return result;
        }

        private void AppendPendingAutoAbilityManualResolutionApplyPreviewLog(
            PendingAutoAbilityManualResolutionApplyResult applyResult)
        {
            pendingAutoAbilityManualResolutionApplyPreviewLogs.Add(
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.FromApplyResult(
                    applyResult,
                    BuildPendingAutoAbilityManualResolutionApplyPreviewLogId(applyResult)));
        }

        private string BuildPendingAutoAbilityManualResolutionApplyPreviewLogId(
            PendingAutoAbilityManualResolutionApplyResult applyResult)
        {
            string acceptedPart = applyResult != null && applyResult.accepted ? "accepted" : "rejected";
            string pendingPart = applyResult == null ? string.Empty : applyResult.pending_id ?? string.Empty;
            string decisionPart = applyResult == null ? string.Empty : applyResult.decision_type ?? string.Empty;
            string rejectionPart = applyResult == null ? string.Empty : applyResult.rejection_reason ?? string.Empty;

            return "pending-auto-manual-apply-preview-log|" +
                   pendingAutoAbilityManualResolutionApplyPreviewLogs.Count +
                   "|" + acceptedPart +
                   "|" + pendingPart +
                   "|" + decisionPart +
                   "|" + rejectionPart;
        }

        public ManualTriggerCheckDraftResult PublishManualTriggerCheckDraft(
            ManualTriggerCheckDraftRequest request)
        {
            ManualTriggerCheckDraftResult draft = ManualTriggerCheckDraftPayloadBuilder.Build(
                state,
                CreateSessionDraftRequest(request));
            if (!draft.accepted)
            {
                LastMessage = draft.rejection_reason;
                return draft;
            }

            MultiplayerTransportResult sendResult = transport.SendTriggerCheckReplayLog(draft.payload);
            draft.sent = sendResult.ok;
            draft.transport_error_code = sendResult.error_code ?? string.Empty;
            draft.transport_message = sendResult.message ?? string.Empty;
            LastMessage = sendResult.ok
                ? "Published manual trigger check draft " + draft.payload.payload_id + "."
                : sendResult.error_code + ": " + sendResult.message;
            if (sendResult.ok)
            {
                triggerCheckReplayLogPayloads.Add(draft.payload);
                RaiseStateChanged();
            }

            return draft;
        }

        public int ApplyReconnectBatch(NetworkEventBatch batch)
        {
            if (batch == null)
            {
                LastMessage = "RECONNECT_BATCH_MISSING";
                LastReconnectAppliedCount = 0;
                LastReconnectFromEventIndex = -1;
                return 0;
            }

            if (!string.Equals(room.room_id ?? "", batch.room_id ?? "", StringComparison.Ordinal))
            {
                LastMessage = "RECONNECT_BATCH_ROOM_MISMATCH";
                LastReconnectAppliedCount = 0;
                LastReconnectFromEventIndex = batch.from_event_index;
                return 0;
            }

            batch.EnsureLists();
            LastReconnectAppliedCount = 0;
            LastReconnectFromEventIndex = batch.from_event_index;
            int applied = 0;
            for (int i = 0; i < batch.events.Count; i++)
            {
                string rejectionReason;
                if (!MultiplayerProtocol.TryApplyEnvelope(state, batch.events[i], out rejectionReason))
                {
                    LastMessage = rejectionReason;
                    LastReconnectAppliedCount = applied;
                    return applied;
                }

                applied++;
            }

            LastReconnectAppliedCount = applied;
            LastMessage = "Applied reconnect batch with " + applied + " events.";
            if (applied > 0)
            {
                RaiseStateChanged();
            }

            return applied;
        }

        private ManualTriggerCheckDraftRequest CreateSessionDraftRequest(ManualTriggerCheckDraftRequest request)
        {
            if (request == null)
            {
                return null;
            }

            return new ManualTriggerCheckDraftRequest
            {
                room_id = room.room_id ?? string.Empty,
                sender_player_id = localPlayerId ?? string.Empty,
                player_index = request.player_index,
                check_source = request.check_source,
                check_index = request.check_index,
                checked_card_instance_id = request.checked_card_instance_id ?? string.Empty,
                checked_card_id = request.checked_card_id ?? string.Empty,
                trigger_type = request.trigger_type,
                modifier_expiration = request.modifier_expiration,
                perspective = request.perspective,
                viewer_player_index = request.viewer_player_index
            };
        }

        private static PendingAutoAbilityManualResolutionDecisionDraftResult
            RejectPendingAutoAbilityManualResolutionDecisionDraft(string rejectionReason)
        {
            return new PendingAutoAbilityManualResolutionDecisionDraftResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                decision = null,
                payload = null
            };
        }

        private static PendingAutoAbilityManualResolutionApplyExecutorResult
            RejectPendingAutoAbilityManualResolutionApply(
                string rejectionReason,
                PendingAutoAbilityQueue queue)
        {
            PendingAutoAbilityManualResolutionApplyResult applyResult =
                PendingAutoAbilityManualResolutionApplyResult.Rejected(rejectionReason);
            return new PendingAutoAbilityManualResolutionApplyExecutorResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                queue = queue,
                apply_result = applyResult
            };
        }

        private void HandleGameEventReceived(NetworkEventEnvelope envelope)
        {
            string rejectionReason;
            TryApplyEnvelope(envelope, out rejectionReason);
        }

        private void HandlePublicGameEventReceived(NetworkPublicGameEvent publicEvent)
        {
            if (publicEvent == null)
            {
                return;
            }

            publicEventLog.Add(publicEvent);
            LastMessage = "Received public event " + publicEvent.event_id + ".";
            RaiseStateChanged();
        }

        private void HandleTriggerCheckReplayLogReceived(NetworkTriggerCheckReplayLogPayload payload)
        {
            if (payload == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(payload.room_id) &&
                !string.Equals(room.room_id ?? "", payload.room_id ?? "", StringComparison.Ordinal))
            {
                LastMessage = "TRIGGER_CHECK_REPLAY_LOG_ROOM_MISMATCH";
                return;
            }

            triggerCheckReplayLogPayloads.Add(payload);
            LastMessage = "Received trigger check replay log " + payload.payload_id + ".";
            RaiseStateChanged();
        }

        private void HandlePendingAutoAbilityQueueReceived(NetworkPendingAutoAbilityQueuePayload payload)
        {
            if (payload == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(payload.room_id) &&
                !string.Equals(room.room_id ?? "", payload.room_id ?? "", StringComparison.Ordinal))
            {
                LastMessage = "PENDING_AUTO_ABILITY_QUEUE_ROOM_MISMATCH";
                return;
            }

            pendingAutoAbilityQueuePayloads.Add(payload);
            LastMessage = "Received pending auto ability queue " + payload.payload_id + ".";
            RaiseStateChanged();
        }

        private void HandlePendingAutoAbilityResolutionRequestReceived(
            NetworkPendingAutoAbilityResolutionRequestPayload payload)
        {
            if (payload == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(payload.room_id) &&
                !string.Equals(room.room_id ?? "", payload.room_id ?? "", StringComparison.Ordinal))
            {
                LastMessage = "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_ROOM_MISMATCH";
                return;
            }

            pendingAutoAbilityResolutionRequestPayloads.Add(payload);
            LastMessage = "Received pending auto ability resolution request " + payload.payload_id + ".";
            RaiseStateChanged();
        }

        private void HandlePendingAutoAbilityManualResolutionDecisionReceived(
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
        {
            if (payload == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(payload.room_id) &&
                !string.Equals(room.room_id ?? "", payload.room_id ?? "", StringComparison.Ordinal))
            {
                LastMessage = "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_ROOM_MISMATCH";
                return;
            }

            pendingAutoAbilityManualResolutionDecisionPayloads.Add(payload);
            LastMessage = "Received pending auto ability manual resolution decision " + payload.payload_id + ".";
            RaiseStateChanged();
        }

        private void HandleReconnectRequestReceived(NetworkReconnectRequest request)
        {
            if (request == null || !string.Equals(room.room_id ?? "", request.room_id ?? "", StringComparison.Ordinal))
            {
                return;
            }

            NetworkEventBatch batch = CreateReconnectBatch(request.from_event_index);
            MultiplayerTransportResult result = transport.SendReconnectBatch(batch);
            LastMessage = result.ok
                ? "Sent reconnect batch from " + batch.from_event_index + "."
                : result.error_code + ": " + result.message;
        }

        private void HandleReconnectBatchReceived(NetworkEventBatch batch)
        {
            ApplyReconnectBatch(batch);
        }

        private void RaiseStateChanged()
        {
            Action handler = StateChanged;
            if (handler != null)
            {
                handler();
            }
        }

        private bool ShouldSendTrueGameEvents()
        {
            return string.IsNullOrWhiteSpace(room.deck_privacy_mode) ||
                string.Equals(room.deck_privacy_mode, DeckPrivacyModes.SharedDeckCode, StringComparison.Ordinal);
        }

        private RoomPlayerInfo FindRoomPlayer(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return null;
            }

            room.EnsureLists();
            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player != null && string.Equals(player.player_id ?? "", playerId, StringComparison.Ordinal))
                {
                    return player;
                }
            }

            return null;
        }
    }
}
