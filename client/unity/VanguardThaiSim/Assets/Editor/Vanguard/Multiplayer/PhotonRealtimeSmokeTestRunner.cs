using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.EditorTools
{
    public static class PhotonRealtimeSmokeTestRunner
    {
        private const string AppIdEnv = "VANGUARD_PHOTON_APP_ID";
        private const string RegionEnv = "VANGUARD_PHOTON_REGION";
        private const string AppVersionEnv = "VANGUARD_PHOTON_APP_VERSION";
        private const string LocalConfigRelativePath = "ProjectSettings/VanguardPhotonLocal.json";
        private const int TimeoutMs = 45000;
        private const int TickSleepMs = 50;

        public static void RunFromCommandLine()
        {
            int exitCode = 1;
            try
            {
                LivePhotonConfig liveConfig = LivePhotonConfig.Load();
                if (!liveConfig.HasAppId)
                {
                    Debug.LogError(
                        "Photon live smoke test skipped: set " + AppIdEnv +
                        " or create " + LocalConfigRelativePath + " from the example file.");
                    exitCode = 2;
                    return;
                }

                RunSmoke(liveConfig);
                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogError("Photon live smoke test failed: " + exception);
                exitCode = 1;
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static void RunSmoke(LivePhotonConfig liveConfig)
        {
            PhotonRealtimeTransportAdapter host = new PhotonRealtimeTransportAdapter(liveConfig.ToTransportConfig());
            PhotonRealtimeTransportAdapter guest = new PhotonRealtimeTransportAdapter(liveConfig.ToTransportConfig());
            MultiplayerRoomState guestRoomState = null;
            NetworkEventEnvelope guestEnvelope = null;

            guest.RoomStateReceived += room => guestRoomState = room;
            guest.GameEventReceived += envelope => guestEnvelope = envelope;

            try
            {
                RoomPlayerInfo hostPlayer = Player("smoke-host");
                RoomPlayerInfo guestPlayer = Player("smoke-guest");
                PackSyncInfo pack = Pack();
                string roomId = "VGTH-SMOKE-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(roomId, "D", hostPlayer.player_id, 9001, pack);
                room.players.Add(hostPlayer);

                RequireOk(host.Connect(), "host connect");
                RequireOk(guest.Connect(), "guest connect");
                PumpUntil("both clients connected", () =>
                    host.Status == MultiplayerTransportStatus.ConnectedToLobby &&
                    guest.Status == MultiplayerTransportStatus.ConnectedToLobby,
                    host,
                    guest);

                RequireOk(host.CreateRoom(room, hostPlayer), "host create room");
                PumpUntil("host joined created room", () => host.Status == MultiplayerTransportStatus.InRoom, host, guest);

                RequireOk(guest.JoinRoom(room.room_id, guestPlayer, pack), "guest join room");
                PumpUntil("guest joined room", () => guest.Status == MultiplayerTransportStatus.InRoom, host, guest);
                PumpUntil("guest received room state", () =>
                    guestRoomState != null && string.Equals(guestRoomState.room_id, room.room_id, StringComparison.Ordinal),
                    host,
                    guest);

                NetworkEventEnvelope envelope = CreateSmokeEnvelope(room, hostPlayer.player_id);
                RequireOk(host.SendGameEvent(envelope), "host send game event");
                PumpUntil("guest received game event", () =>
                    guestEnvelope != null &&
                    guestEnvelope.game_event != null &&
                    string.Equals(guestEnvelope.game_event.event_id, envelope.game_event.event_id, StringComparison.Ordinal),
                    host,
                    guest);

                Debug.Log("Photon live smoke test passed for room " + room.room_id + ".");
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

        private static void PumpUntil(string label, Func<bool> condition, params PhotonRealtimeTransportAdapter[] adapters)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(TimeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                PumpOnce(adapters);
                if (condition())
                {
                    return;
                }

                ThrowIfFailed(label, adapters);
                Thread.Sleep(TickSleepMs);
            }

            throw new TimeoutException("Timed out while waiting for " + label + ".");
        }

        private static void PumpOnce(params PhotonRealtimeTransportAdapter[] adapters)
        {
            foreach (PhotonRealtimeTransportAdapter adapter in adapters)
            {
                adapter.Tick();
            }
        }

        private static void ThrowIfFailed(string label, params PhotonRealtimeTransportAdapter[] adapters)
        {
            foreach (PhotonRealtimeTransportAdapter adapter in adapters)
            {
                if (adapter.Status == MultiplayerTransportStatus.Failed)
                {
                    throw new InvalidOperationException(label + " failed through transport status: " + adapter.LastError);
                }
            }
        }

        private static NetworkEventEnvelope CreateSmokeEnvelope(MultiplayerRoomState room, string playerId)
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("host"), CreateSampleDeck("guest"), room.random_seed);
            LegalGameAction draw = FirstAction(RulesCore.GetLegalActions(state, 0), GameActionType.Draw);
            GameEvent gameEvent = RulesCore.ExecuteOrThrow(state, draw);
            return MultiplayerProtocol.CreateEnvelope(room, playerId, state, gameEvent);
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

            throw new InvalidOperationException("No legal action found for " + actionType + ".");
        }

        private static VanguardDeck CreateSampleDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "smoke");
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
                source_version = "smoke",
                definition_hash = "smoke-definition",
                image_manifest_hash = "smoke-image-manifest",
                image_content_hash = "smoke-image-content"
            };
        }

        private static RoomPlayerInfo Player(string playerId)
        {
            return new RoomPlayerInfo
            {
                player_id = playerId,
                display_name = playerId,
                deck_id = playerId + "-deck",
                deck_hash = playerId + "-deck-hash",
                connected = true
            };
        }

        [Serializable]
        private sealed class LocalPhotonConfigJson
        {
            public string app_id;
            public string fixed_region;
            public string app_version;
        }

        private sealed class LivePhotonConfig
        {
            public string appId;
            public string fixedRegion;
            public string appVersion;

            public bool HasAppId
            {
                get { return !string.IsNullOrWhiteSpace(appId); }
            }

            public static LivePhotonConfig Load()
            {
                LocalPhotonConfigJson fileConfig = LoadFileConfig();
                return new LivePhotonConfig
                {
                    appId = FirstNonEmpty(Environment.GetEnvironmentVariable(AppIdEnv), fileConfig == null ? null : fileConfig.app_id),
                    fixedRegion = FirstNonEmpty(Environment.GetEnvironmentVariable(RegionEnv), fileConfig == null ? null : fileConfig.fixed_region, "asia"),
                    appVersion = FirstNonEmpty(Environment.GetEnvironmentVariable(AppVersionEnv), fileConfig == null ? null : fileConfig.app_version, "0.1.0-smoke")
                };
            }

            public PhotonRealtimeTransportConfig ToTransportConfig()
            {
                return new PhotonRealtimeTransportConfig
                {
                    app_id = appId,
                    app_version = appVersion,
                    fixed_region = fixedRegion,
                    send_reliable = true
                };
            }

            private static LocalPhotonConfigJson LoadFileConfig()
            {
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                string configPath = Path.Combine(projectPath, LocalConfigRelativePath);
                if (!File.Exists(configPath))
                {
                    return null;
                }

                string json = File.ReadAllText(configPath);
                return JsonUtility.FromJson<LocalPhotonConfigJson>(json);
            }

            private static string FirstNonEmpty(params string[] values)
            {
                foreach (string value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value.Trim();
                    }
                }

                return null;
            }
        }
    }
}
