using System;
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
        public event Action<GameFlowState> StateChanged;

        public GameFlowState State { get; private set; } = GameFlowState.Ready;

        public void StartRun()
        {
            if (State == GameFlowState.Ready)
            {
                SetState(GameFlowState.Playing);
            }
        }

        public void Win()
        {
            if (State == GameFlowState.Playing)
            {
                SetState(GameFlowState.Won);
            }
        }

        public void Lose()
        {
            if (State == GameFlowState.Playing)
            {
                SetState(GameFlowState.Lost);
            }
        }

        private void SetState(GameFlowState state)
        {
            State = state;
            StateChanged?.Invoke(State);
        }
    }
}
