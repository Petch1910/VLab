using System;

namespace VanguardThaiSim.Game
{
    public sealed class GameReplayPlayer
    {
        private readonly GameReplay replay;

        public GameState CurrentState { get; private set; }
        public int CurrentIndex { get; private set; }
        public int EventCount => replay.events.Count;
        public bool IsAtEnd => CurrentIndex >= EventCount;

        public GameReplayPlayer(GameReplay replay)
        {
            this.replay = replay ?? throw new ArgumentNullException(nameof(replay));
            JumpToStart();
        }

        public void JumpToStart()
        {
            CurrentState = replay.CreateInitialState();
            CurrentIndex = 0;
        }

        public bool StepForward()
        {
            if (IsAtEnd)
            {
                return false;
            }

            GameEventReducer.Apply(CurrentState, replay.events[CurrentIndex], true);
            CurrentIndex++;
            return true;
        }

        public void JumpToEnd()
        {
            while (StepForward())
            {
            }
        }

        public GameState CreateCurrentStateView(GameStateViewPerspective perspective, int viewerPlayerIndex = -1)
        {
            return GameStateViewFactory.CreateView(CurrentState, perspective, viewerPlayerIndex);
        }
    }
}
