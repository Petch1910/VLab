using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.EditorTools
{
    public static class PhotonGameSessionSmokeTestRunner
    {
        private const int TimeoutMs = 45000;
        private const int TickSleepMs = 50;

        public static void RunFromCommandLine()
        {
            int exitCode = 1;
            try
            {
                PhotonRealtimeTransportConfig config = PhotonRealtimeConfigLoader.Load();
                if (!config.IsConfigured)
                {
                    Debug.LogError(
                        "Photon game-session smoke test skipped: set " + PhotonRealtimeConfigLoader.AppIdEnv +
                        " or create " + PhotonRealtimeConfigLoader.LocalConfigRelativePath + " from the example file.");
                    exitCode = 2;
                    return;
                }

                RunSmoke(config);
                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogError("Photon game-session smoke test failed: " + exception);
                exitCode = 1;
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static void RunSmoke(PhotonRealtimeTransportConfig config)
        {
            PhotonRealtimeTransportAdapter hostTransport = new PhotonRealtimeTransportAdapter(config);
            PhotonRealtimeTransportAdapter guestTransport = new PhotonRealtimeTransportAdapter(config);
            MultiplayerLobbyController hostLobby = new MultiplayerLobbyController(hostTransport, Pack());
            MultiplayerLobbyController guestLobby = new MultiplayerLobbyController(guestTransport, Pack());
            string roomId = "VGTH-GAME-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

            try
            {
                RequireOk(hostLobby.Connect(), "host connect");
                RequireOk(guestLobby.Connect(), "guest connect");
                PumpUntil(
                    "both controllers connected",
                    () => hostLobby.Status == MultiplayerTransportStatus.ConnectedToLobby &&
                          guestLobby.Status == MultiplayerTransportStatus.ConnectedToLobby,
                    hostLobby,
                    guestLobby);

                VanguardDeck hostDeck = CreateSampleDeck("host");
                VanguardDeck guestDeck = CreateSampleDeck("guest");
                RequireOk(hostLobby.CreateRoom(roomId, "Game Host", hostDeck, 12001), "host create room");
                PumpUntil("host entered room", () => hostLobby.Status == MultiplayerTransportStatus.InRoom, hostLobby, guestLobby);

                RequireOk(guestLobby.JoinRoom(roomId, "Game Guest", guestDeck), "guest join room");
                PumpUntil(
                    "both room states contain two players",
                    () => hostLobby.CurrentRoom != null &&
                          guestLobby.CurrentRoom != null &&
                          hostLobby.CurrentRoom.players.Count >= 2 &&
                          guestLobby.CurrentRoom.players.Count >= 2,
                    hostLobby,
                    guestLobby);

                MultiplayerRoomState playingRoom = CreateStartedRoom(hostLobby.CurrentRoom);
                RequireOk(hostTransport.SendRoomState(playingRoom), "host send playing room state");
                PumpUntil(
                    "guest received playing room state",
                    () => guestLobby.CurrentRoom != null &&
                          string.Equals(guestLobby.CurrentRoom.state ?? "", RoomLifecycleStates.Playing, StringComparison.Ordinal),
                    hostLobby,
                    guestLobby);

                GameState initialState = GameStateFactory.CreateTwoPlayerGame(hostDeck, guestDeck, 12001);
                GameState hostState = GameState.FromJson(initialState.ToJson(false));
                GameState guestState = GameState.FromJson(initialState.ToJson(false));
                MultiplayerGameSessionController hostSession = new MultiplayerGameSessionController(
                    hostTransport,
                    playingRoom,
                    hostState,
                    hostLobby.LocalPlayer.player_id);
                MultiplayerGameSessionController guestSession = new MultiplayerGameSessionController(
                    guestTransport,
                    playingRoom,
                    guestState,
                    guestLobby.LocalPlayer.player_id);

                RequireAccepted(
                    hostSession.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostState, 0), GameActionType.Draw)),
                    "host draw");
                PumpUntil(
                    "guest received host draw event",
                    () => guestSession.State.event_log.Count == hostSession.State.event_log.Count &&
                          guestSession.State.GetPlayer(0).CountZone(GameZone.Deck) == hostSession.State.GetPlayer(0).CountZone(GameZone.Deck) &&
                          guestSession.State.GetPlayer(0).CountZone(GameZone.Hand) == hostSession.State.GetPlayer(0).CountZone(GameZone.Hand),
                    hostLobby,
                    guestLobby);

                RequireOk(
                    guestTransport.SendReconnectRequest(new NetworkReconnectRequest
                    {
                        protocol_version = MultiplayerProtocol.ProtocolVersion,
                        room_id = roomId,
                        player_id = guestLobby.LocalPlayer.player_id,
                        from_event_index = 0
                    }),
                    "guest reconnect request");
                PumpUntil(
                    "guest received reconnect batch",
                    () => !string.IsNullOrEmpty(guestSession.LastMessage) &&
                          guestSession.LastMessage.Contains("Applied reconnect batch"),
                    hostLobby,
                    guestLobby);

                RoomLifecycleTransitionResult endedRoom = RoomLifecycleController.End(playingRoom);
                RequireLifecycleAccepted(endedRoom, "end room");
                playingRoom = endedRoom.room;
                RequireOk(hostTransport.SendRoomState(playingRoom), "host send ended room state");
                PumpUntil(
                    "guest received ended room state",
                    () => guestLobby.CurrentRoom != null &&
                          string.Equals(guestLobby.CurrentRoom.state ?? "", RoomLifecycleStates.Ended, StringComparison.Ordinal),
                    hostLobby,
                    guestLobby);

                Debug.Log("Photon game-session smoke test passed for room " + roomId + ".");
            }
            finally
            {
                hostLobby.Disconnect();
                guestLobby.Disconnect();
            }
        }

        private static void RequireOk(MultiplayerTransportResult result, string action)
        {
            if (result == null || !result.ok)
            {
                string code = result == null ? "NO_RESULT" : result.error_code;
                string message = result == null ? "No result returned." : result.message;
                throw new InvalidOperationException(action + " failed: " + code + " " + message);
            }
        }

        private static void RequireAccepted(RulesCommandResult result, string action)
        {
            if (result == null || !result.accepted)
            {
                string message = result == null ? "No result returned." : result.rejection_reason;
                throw new InvalidOperationException(action + " rejected: " + message);
            }
        }

        private static void PumpUntil(string label, Func<bool> condition, params MultiplayerLobbyController[] controllers)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(TimeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                foreach (MultiplayerLobbyController controller in controllers)
                {
                    controller.Tick();
                }

                if (condition())
                {
                    return;
                }

                ThrowIfFailed(label, controllers);
                Thread.Sleep(TickSleepMs);
            }

            throw new TimeoutException("Timed out while waiting for " + label + ".");
        }

        private static MultiplayerRoomState CreateStartedRoom(MultiplayerRoomState room)
        {
            if (room == null)
            {
                throw new InvalidOperationException("Cannot start missing room.");
            }

            room.EnsureLists();
            MultiplayerRoomState cursor = room;
            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player == null || !player.connected)
                {
                    continue;
                }

                RoomLifecycleTransitionResult ready = RoomLifecycleController.SetPlayerReady(
                    cursor,
                    player.player_id,
                    true);
                RequireLifecycleAccepted(ready, "ready " + player.player_id);
                cursor = ready.room;
            }

            RoomLifecycleTransitionResult started = RoomLifecycleController.Start(cursor);
            RequireLifecycleAccepted(started, "start room");
            return started.room;
        }

        private static void RequireLifecycleAccepted(RoomLifecycleTransitionResult result, string action)
        {
            if (result == null || !result.accepted)
            {
                string reason = result == null ? "NO_RESULT" : result.rejection_reason;
                throw new InvalidOperationException(action + " rejected: " + reason);
            }
        }

        private static void ThrowIfFailed(string label, IEnumerable<MultiplayerLobbyController> controllers)
        {
            foreach (MultiplayerLobbyController controller in controllers)
            {
                if (controller.Status == MultiplayerTransportStatus.Failed)
                {
                    throw new InvalidOperationException(label + " failed through transport status: " + controller.LastMessage);
                }
            }
        }

        private static VanguardDeck CreateSampleDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "game-session-smoke");
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i, 1);
            }

            for (int i = 0; i < 4; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i, 1);
            }

            return deck;
        }

        private static PackSyncInfo Pack()
        {
            return new PackSyncInfo
            {
                pack_id = "vanguard_th",
                source_version = "game-session-smoke",
                definition_hash = "game-session-smoke-definition",
                image_manifest_hash = "game-session-smoke-image-manifest",
                image_content_hash = "game-session-smoke-image-content"
            };
        }

        private static LegalGameAction FirstAction(IReadOnlyList<LegalGameAction> actions, GameActionType actionType)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == actionType)
                {
                    return action;
                }
            }

            throw new InvalidOperationException("Missing action " + actionType + ".");
        }
    }
}
