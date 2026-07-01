using NUnit.Framework;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class RoomLifecycleControllerTests
    {
        [Test]
        public void ReadyTransitionsUseCloneAndSourceIsUnchanged()
        {
            MultiplayerRoomState room = CreateRoom();
            string before = room.ToJson(false);

            RoomLifecycleTransitionResult first = RoomLifecycleController.SetPlayerReady(room, "p1", true);
            RoomLifecycleTransitionResult second = RoomLifecycleController.SetPlayerReady(first.room, "p2", true);

            Assert.IsTrue(first.accepted, first.rejection_reason);
            Assert.IsTrue(second.accepted, second.rejection_reason);
            Assert.AreEqual(RoomLifecycleStates.Waiting, first.room.state);
            Assert.AreEqual(RoomLifecycleStates.Ready, second.room.state);
            Assert.IsTrue(second.room.players[0].ready);
            Assert.IsTrue(second.room.players[1].ready);
            Assert.AreEqual(before, room.ToJson(false));
        }

        [Test]
        public void StartRejectsUntilAllConnectedPlayersReadyWithoutMutation()
        {
            MultiplayerRoomState room = CreateRoom();
            string before = room.ToJson(false);

            RoomLifecycleTransitionResult rejected = RoomLifecycleController.Start(room);

            Assert.IsFalse(rejected.accepted);
            Assert.AreEqual("PLAYERS_NOT_READY", rejected.rejection_reason);
            Assert.AreEqual(before, room.ToJson(false));
        }

        [Test]
        public void StartEndAndRematchFlow()
        {
            MultiplayerRoomState room = CreateRoom();
            room = RoomLifecycleController.SetPlayerReady(room, "p1", true).room;
            room = RoomLifecycleController.SetPlayerReady(room, "p2", true).room;

            RoomLifecycleTransitionResult started = RoomLifecycleController.Start(room);
            RoomLifecycleTransitionResult ended = RoomLifecycleController.End(started.room);
            ended.room.players[0].event_cursor = 3;
            ended.room.players[1].event_cursor = 4;
            RoomLifecycleTransitionResult rematch = RoomLifecycleController.Rematch(ended.room);

            Assert.IsTrue(started.accepted, started.rejection_reason);
            Assert.AreEqual(RoomLifecycleStates.Playing, started.room.state);
            Assert.IsTrue(ended.accepted, ended.rejection_reason);
            Assert.AreEqual(RoomLifecycleStates.Ended, ended.room.state);
            Assert.IsTrue(rematch.accepted, rematch.rejection_reason);
            Assert.AreEqual(RoomLifecycleStates.Waiting, rematch.room.state);
            Assert.IsFalse(rematch.room.players[0].ready);
            Assert.IsFalse(rematch.room.players[1].ready);
            Assert.AreEqual(0, rematch.room.players[0].event_cursor);
            Assert.AreEqual(0, rematch.room.players[1].event_cursor);
        }

        [Test]
        public void InvalidTransitionsRejectWithoutMutation()
        {
            MultiplayerRoomState room = CreateRoom();
            string before = room.ToJson(false);

            RoomLifecycleTransitionResult end = RoomLifecycleController.End(room);
            RoomLifecycleTransitionResult rematch = RoomLifecycleController.Rematch(room);

            Assert.IsFalse(end.accepted);
            Assert.AreEqual("ROOM_NOT_PLAYING", end.rejection_reason);
            Assert.IsFalse(rematch.accepted);
            Assert.AreEqual("ROOM_NOT_ENDED", rematch.rejection_reason);
            Assert.AreEqual(before, room.ToJson(false));
        }

        private static MultiplayerRoomState CreateRoom()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-LIFE",
                "D",
                "p1",
                3131,
                new PackSyncInfo
                {
                    pack_id = "vanguard_th",
                    source_version = "test",
                    definition_hash = "pack-hash",
                    image_manifest_hash = "image-manifest",
                    image_content_hash = "image-content"
                });
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "p1",
                connected = true
            });
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p2",
                display_name = "p2",
                connected = true
            });
            return room;
        }
    }
}
