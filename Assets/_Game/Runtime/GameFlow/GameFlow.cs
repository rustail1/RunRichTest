using System;
using RunRich.Runtime.Level;

namespace RunRich.Runtime.Flow
{
    public enum GameFlowState
    {
        Ready,
        Playing,
        Won,
        Lost
    }

    public sealed class GameFlow
    {
        private readonly ILevelService _levelService;

        public GameFlow(ILevelService levelService)
        {
            _levelService = levelService ?? throw new ArgumentNullException(nameof(levelService));
        }

        public GameFlowState State { get; private set; } = GameFlowState.Ready;

        public void StartRun()
        {
            if (State == GameFlowState.Ready)
            {
                State = GameFlowState.Playing;
            }
        }

        public void Win()
        {
            if (State == GameFlowState.Playing)
            {
                State = GameFlowState.Won;
            }
        }

        public void Lose()
        {
            if (State == GameFlowState.Playing)
            {
                State = GameFlowState.Lost;
            }
        }

        public void Restart()
        {
            _levelService.Restart();
            State = GameFlowState.Ready;
        }

        public void LoadNext()
        {
            _levelService.LoadNext();
            State = GameFlowState.Ready;
        }
    }
}
