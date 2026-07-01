using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public sealed class NetworkPublicGameReplayPlayer
    {
        private readonly NetworkPublicGameReplay replay;

        public GameState InitialStateView { get; private set; }
        public GameState CurrentStateView { get; private set; }
        public int CurrentIndex { get; private set; }
        public int EventCount => replay.events == null ? 0 : replay.events.Count;
        public bool IsAtEnd => CurrentIndex >= EventCount;

        public NetworkPublicGameReplayPlayer(NetworkPublicGameReplay replay)
        {
            this.replay = replay ?? throw new ArgumentNullException(nameof(replay));
            EnsureReplayEvents();
            JumpToStart();
        }

        public void JumpToStart()
        {
            EnsureReplayEvents();
            InitialStateView = replay.CreateInitialStateView();
            CurrentStateView = replay.CreateInitialStateView();
            CurrentIndex = 0;
        }

        public bool StepForward()
        {
            string rejectionReason;
            if (TryStepForward(out rejectionReason))
            {
                return true;
            }

            if (!IsAtEnd)
            {
                throw new InvalidOperationException(
                    "Public replay event rejected: " + (rejectionReason ?? "PUBLIC_REPLAY_EVENT_REJECTED"));
            }

            return false;
        }

        public bool TryStepForward(out string rejectionReason)
        {
            if (IsAtEnd)
            {
                rejectionReason = null;
                return false;
            }

            GameState workingState = CloneCurrentState();
            NetworkPublicGameEventApplyResult result =
                NetworkPublicGameEventApplier.ApplyToPublicView(workingState, replay.events[CurrentIndex]);
            if (!result.accepted)
            {
                rejectionReason = result.rejection_reason;
                return false;
            }

            CurrentStateView = workingState;
            CurrentIndex++;
            rejectionReason = null;
            return true;
        }

        public void JumpToEnd()
        {
            while (StepForward())
            {
            }
        }

        public IReadOnlyList<NetworkPublicGameEvent> CreateVisibleEventLog()
        {
            List<NetworkPublicGameEvent> visibleEvents = new List<NetworkPublicGameEvent>();
            for (int i = 0; i < CurrentIndex && i < replay.events.Count; i++)
            {
                visibleEvents.Add(NetworkPublicGameReplay.CloneEvent(replay.events[i]));
            }

            return visibleEvents;
        }

        public GameState CreateCurrentStateSnapshot()
        {
            return CloneCurrentState();
        }

        public NetworkPublicReconnectApplyResult ApplyBatch(NetworkPublicEventBatch batch)
        {
            EnsureReplayEvents();
            if (batch == null)
            {
                return NetworkPublicReconnectApplyResult.Rejected("PUBLIC_EVENT_BATCH_MISSING");
            }

            batch.EnsureLists();
            if (batch.protocol_version != MultiplayerProtocol.ProtocolVersion)
            {
                return NetworkPublicReconnectApplyResult.Rejected("PROTOCOL_VERSION_MISMATCH");
            }

            if (CurrentIndex != EventCount)
            {
                return NetworkPublicReconnectApplyResult.Rejected("PUBLIC_REPLAY_NOT_AT_CURSOR");
            }

            if (batch.from_event_index != CurrentIndex)
            {
                return NetworkPublicReconnectApplyResult.Rejected("PUBLIC_REPLAY_CURSOR_MISMATCH");
            }

            GameState workingState = CloneCurrentState();
            List<NetworkPublicGameEvent> acceptedEvents = new List<NetworkPublicGameEvent>();
            for (int i = 0; i < batch.events.Count; i++)
            {
                NetworkPublicGameEvent publicEvent = NetworkPublicGameReplay.CloneEvent(batch.events[i]);
                NetworkPublicGameEventApplyResult result =
                    NetworkPublicGameEventApplier.ApplyToPublicView(workingState, publicEvent);
                if (!result.accepted)
                {
                    return NetworkPublicReconnectApplyResult.Rejected(result.rejection_reason);
                }

                acceptedEvents.Add(publicEvent);
            }

            for (int i = 0; i < acceptedEvents.Count; i++)
            {
                replay.events.Add(acceptedEvents[i]);
            }

            CurrentStateView = workingState;
            CurrentIndex = replay.events.Count;
            return NetworkPublicReconnectApplyResult.Accepted(acceptedEvents.Count);
        }

        private void EnsureReplayEvents()
        {
            if (replay.events == null)
            {
                replay.events = new List<NetworkPublicGameEvent>();
            }
        }

        private GameState CloneCurrentState()
        {
            if (CurrentStateView == null)
            {
                return replay.CreateInitialStateView();
            }

            return GameState.FromJson(CurrentStateView.ToJson(false));
        }
    }
}
