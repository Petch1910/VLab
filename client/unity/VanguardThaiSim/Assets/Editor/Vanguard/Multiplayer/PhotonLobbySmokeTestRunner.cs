using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.EditorTools
{
    public static class PhotonLobbySmokeTestRunner
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
                        "Photon lobby smoke test skipped: set " + PhotonRealtimeConfigLoader.AppIdEnv +
                        " or create " + PhotonRealtimeConfigLoader.LocalConfigRelativePath + " from the example file.");
                    exitCode = 2;
                    return;
                }

                RunSmoke(config);
                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogError("Photon lobby smoke test failed: " + exception);
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
            MultiplayerLobbyController host = new MultiplayerLobbyController(hostTransport, Pack());
            MultiplayerLobbyController guest = new MultiplayerLobbyController(guestTransport, Pack());
            string roomId = "VGTH-LOBBY-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

            try
            {
                RequireOk(host.Connect(), "host connect");
                RequireOk(guest.Connect(), "guest connect");
                PumpUntil(
                    "both controllers connected",
                    () => host.Status == MultiplayerTransportStatus.ConnectedToLobby &&
                          guest.Status == MultiplayerTransportStatus.ConnectedToLobby,
                    host,
                    guest);

                RequireOk(host.CreateRoom(roomId, "Lobby Host", CreateSampleDeck("host"), 9001), "host create room");
                PumpUntil("host entered room", () => host.Status == MultiplayerTransportStatus.InRoom, host, guest);

                RequireOk(guest.JoinRoom(roomId, "Lobby Guest", CreateSampleDeck("guest")), "guest join room");
                PumpUntil(
                    "guest received room",
                    () => guest.CurrentRoom != null &&
                          string.Equals(guest.CurrentRoom.room_id, roomId, StringComparison.Ordinal),
                    host,
                    guest);
                PumpUntil(
                    "host received guest room state",
                    () => host.CurrentRoom != null && host.CurrentRoom.players.Count >= 2,
                    host,
                    guest);

                RequireOk(guest.RequestReconnectBatch(1), "guest reconnect request");
                PumpUntil(
                    "host received reconnect request",
                    () => host.LastReconnectRequest != null &&
                          host.LastReconnectRequest.from_event_index == 1,
                    host,
                    guest);

                NetworkEventBatch batch = new NetworkEventBatch
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    room_id = roomId,
                    from_event_index = 1
                };
                RequireOk(host.SendReconnectBatch(batch), "host reconnect batch");
                PumpUntil(
                    "guest received reconnect batch",
                    () => guest.LastReconnectBatch != null &&
                          guest.LastReconnectBatch.from_event_index == 1,
                    host,
                    guest);

                Debug.Log("Photon lobby smoke test passed for room " + roomId + ".");
            }
            finally
            {
                host.Disconnect();
                guest.Disconnect();
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
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "lobby-smoke");
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
                source_version = "lobby-smoke",
                definition_hash = "lobby-smoke-definition",
                image_manifest_hash = "lobby-smoke-image-manifest",
                image_content_hash = "lobby-smoke-image-content"
            };
        }
    }
}
