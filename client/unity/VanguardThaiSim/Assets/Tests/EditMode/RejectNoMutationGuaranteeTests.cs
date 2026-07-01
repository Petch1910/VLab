using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class RejectNoMutationGuaranteeTests
    {
        [Test]
        public void RulesCoreRejectsNullActionWithoutMutatingState()
        {
            GameState state = CreateGameState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            RulesCommandResult result = RulesCore.TryExecute(state, null);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("Action is null.", result.rejection_reason);
            Assert.IsTrue(snapshot.Matches(state));
            Assert.AreEqual(0, state.event_log.Count);
        }

        [Test]
        public void RulesCoreRejectsIllegalActionWithoutMutatingState()
        {
            GameState state = CreateGameState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);
            var illegalAction = new LegalGameAction
            {
                action_type = GameActionType.MoveCard,
                actor_index = 0,
                card_instance_id = "missing-card",
                from_zone = GameZone.Hand,
                to_zone = GameZone.Vanguard
            };

            RulesCommandResult result = RulesCore.TryExecute(state, illegalAction);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("Action is not legal in the current state.", result.rejection_reason);
            Assert.IsTrue(snapshot.Matches(state));
            Assert.AreEqual(0, state.event_log.Count);
            Assert.AreEqual(1, state.GetPlayer(0).hand.Count);
            Assert.AreEqual(1, state.GetPlayer(0).deck.Count);
        }

        [Test]
        public void GameStateSnapshotDetectsAcceptedMutation()
        {
            GameState state = CreateGameState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);
            LegalGameAction draw = FirstAction(RulesCore.GetLegalActions(state, 0), GameActionType.Draw);

            RulesCommandResult result = RulesCore.TryExecute(state, draw);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsFalse(snapshot.Matches(state));
            Assert.AreEqual(1, state.event_log.Count);
            Assert.AreEqual(0, state.GetPlayer(0).deck.Count);
            Assert.AreEqual(2, state.GetPlayer(0).hand.Count);
        }

        [Test]
        public void PendingQueueCommitRejectsWithoutMutatingSourceQueue()
        {
            PendingAutoAbilityQueue queue = CreateQueue("pending-1");
            PendingAutoAbilityQueueNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(queue);

            PendingAutoAbilityQueueCommitResult result =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "other-pending"));

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitRejectionReasons.PendingIdMismatch, result.rejection_reason);
            Assert.IsTrue(snapshot.Matches(queue));
            Assert.AreEqual(1, queue.pending.Count);
            Assert.AreEqual("pending-1", queue.pending[0].pending_id);
        }

        [Test]
        public void PendingQueueCommitRejectsNullPendingListWithoutNormalizingSourceQueue()
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-null",
                pending = null
            };
            PendingAutoAbilityQueueNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(queue);

            PendingAutoAbilityQueueCommitResult result =
                PendingAutoAbilityQueueCommitHelper.Commit(
                    queue,
                    CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, "pending-1"));

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(PendingAutoAbilityQueueCommitRejectionReasons.QueueMissing, result.rejection_reason);
            Assert.IsTrue(snapshot.Matches(queue));
            Assert.IsNull(queue.pending);
        }

        [Test]
        public void SessionRejectsMissingPayloadPublishWithoutChangingPayloadHistories()
        {
            GameState state = CreateGameState();
            var transport = new NoMutationTestTransport();
            MultiplayerGameSessionController session =
                new MultiplayerGameSessionController(transport, CreateRoom(), state, "p1");
            GameStateNoMutationSnapshot stateSnapshot = NoMutationSnapshot.Capture(state);
            CollectionCountNoMutationSnapshot queuePayloads =
                NoMutationSnapshot.CaptureCount(session.PendingAutoAbilityQueuePayloads);
            CollectionCountNoMutationSnapshot requestPayloads =
                NoMutationSnapshot.CaptureCount(session.PendingAutoAbilityResolutionRequestPayloads);
            CollectionCountNoMutationSnapshot decisionPayloads =
                NoMutationSnapshot.CaptureCount(session.PendingAutoAbilityManualResolutionDecisionPayloads);
            CollectionCountNoMutationSnapshot applyPreviewLogs =
                NoMutationSnapshot.CaptureCount(session.PendingAutoAbilityManualResolutionApplyPreviewLogs);

            MultiplayerTransportResult result = session.PublishLatestPendingAutoAbilityQueue();

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PENDING_AUTO_ABILITY_QUEUE_MISSING", result.error_code);
            Assert.IsTrue(stateSnapshot.Matches(state));
            Assert.IsTrue(queuePayloads.Matches(session.PendingAutoAbilityQueuePayloads));
            Assert.IsTrue(requestPayloads.Matches(session.PendingAutoAbilityResolutionRequestPayloads));
            Assert.IsTrue(decisionPayloads.Matches(session.PendingAutoAbilityManualResolutionDecisionPayloads));
            Assert.IsTrue(applyPreviewLogs.Matches(session.PendingAutoAbilityManualResolutionApplyPreviewLogs));
            Assert.AreEqual(0, transport.sent_pending_queue_count);
            Assert.AreEqual(0, state.event_log.Count);
        }

        private static GameState CreateGameState()
        {
            return new GameState
            {
                game_id = "no-mutation-game",
                format = "D",
                random_seed = 11,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Mulligan,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-deck-1", "CARD-DECK-1", 0)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-hand-1", "CARD-HAND-1", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p2-deck-1", "CARD-DECK-1", 1)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p2-hand-1", "CARD-HAND-1", 1)
                        }
                    }
                },
                event_log = new List<GameEvent>(),
                pending_auto_abilities = CreateQueue("pending-state-1")
            };
        }

        private static PendingAutoAbilityQueue CreateQueue(params string[] pendingIds)
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>()
            };

            for (int i = 0; i < pendingIds.Length; i++)
            {
                queue.pending.Add(new PendingAutoAbility
                {
                    pending_id = pendingIds[i],
                    source_card_instance_id = "source-" + i,
                    source_card_id = "CARD-" + i,
                    player_index = 0,
                    timing_event = "OnDraw",
                    summary = "Pending " + pendingIds[i]
                });
            }

            return queue;
        }

        private static PendingAutoAbilityManualResolutionDecision CreateDecision(
            string decisionType,
            string pendingId)
        {
            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision|" + decisionType + "|" + pendingId,
                decision_type = decisionType,
                selected_index = 0,
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnDraw",
                reason = "test",
                summary = "Decision " + decisionType
            };
        }

        private static LegalGameAction FirstAction(
            IReadOnlyList<LegalGameAction> actions,
            GameActionType actionType)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == actionType)
                {
                    return actions[i];
                }
            }

            Assert.Fail("Missing action " + actionType);
            return null;
        }

        private static MultiplayerRoomState CreateRoom()
        {
            return MultiplayerProtocol.CreateRoom("ROOM-M11-05", "D", "p1", 11, new PackSyncInfo
            {
                pack_id = "vanguard_th",
                source_version = "test",
                definition_hash = "definition",
                image_manifest_hash = "image-manifest",
                image_content_hash = "image-content"
            });
        }

        private sealed class NoMutationTestTransport : IMultiplayerTransport
        {
            public int sent_pending_queue_count;

            public string TransportName
            {
                get { return "NoMutationTest"; }
            }

            public MultiplayerTransportStatus Status
            {
                get { return MultiplayerTransportStatus.InRoom; }
            }

            public string LastError
            {
                get { return string.Empty; }
            }

            public event Action<MultiplayerRoomState> RoomStateReceived;
            public event Action<NetworkEventEnvelope> GameEventReceived;
            public event Action<NetworkPublicGameEvent> PublicGameEventReceived;
            public event Action<NetworkTriggerCheckReplayLogPayload> TriggerCheckReplayLogReceived;
            public event Action<NetworkPendingAutoAbilityQueuePayload> PendingAutoAbilityQueueReceived;
            public event Action<NetworkPendingAutoAbilityResolutionRequestPayload> PendingAutoAbilityResolutionRequestReceived;
            public event Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> PendingAutoAbilityManualResolutionDecisionReceived;
            public event Action<NetworkReconnectRequest> ReconnectRequestReceived;
            public event Action<NetworkEventBatch> ReconnectBatchReceived;
            public event Action<NetworkDeckRevealRequest> DeckRevealRequestReceived;
            public event Action<NetworkDeckRevealResponse> DeckRevealResponseReceived;

            public MultiplayerTransportResult Connect()
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo localPlayer)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo localPlayer, PackSyncInfo localPack)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendRoomState(MultiplayerRoomState room)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                sent_pending_queue_count++;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response)
            {
                return MultiplayerTransportResult.Ok();
            }

            public void Tick()
            {
            }

            public void Disconnect()
            {
            }

            public void SilenceUnusedEvents()
            {
                RoomStateReceived = null;
                GameEventReceived = null;
                PublicGameEventReceived = null;
                TriggerCheckReplayLogReceived = null;
                PendingAutoAbilityQueueReceived = null;
                PendingAutoAbilityResolutionRequestReceived = null;
                PendingAutoAbilityManualResolutionDecisionReceived = null;
                ReconnectRequestReceived = null;
                ReconnectBatchReceived = null;
                DeckRevealRequestReceived = null;
                DeckRevealResponseReceived = null;
            }
        }
    }
}
