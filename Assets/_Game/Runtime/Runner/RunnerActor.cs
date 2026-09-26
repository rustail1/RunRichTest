using RunRich.Runtime.Finish;
using RunRich.Runtime.Flow;
using RunRich.Runtime.Wealth;
using UnityEngine;

namespace RunRich.Runtime.Runner
{
    public sealed class RunnerActor : MonoBehaviour
    {
        [SerializeField] private Transform steeringVisual;
        [SerializeField] private Transform cameraPositionTarget;
        [SerializeField] private Transform cameraRotationTarget;
        [SerializeField] private RunnerFeedback feedback;

        private GameFlow _gameFlow;
        private RichPoorController _wealth;
        private FinishProgress _finishProgress;
        private Transform _lastCheckpoint;

        public Transform SteeringVisual
        {
            get
            {
                if (steeringVisual != null)
                    return steeringVisual;

                steeringVisual = transform.Find("ToMove/VisualCont")
                    ?? transform.Find("VisualCont")
                    ?? transform.Find("VisualRoot")
                    ?? transform.Find("CharacterVisual");
                return steeringVisual;
            }
        }

        /// <summary>
        /// Original CamController follows MovePlayer.toMove for position.
        /// Existing scenes without a ToMove anchor safely fall back to the player root.
        /// </summary>
        public Transform CameraPositionTarget
        {
            get
            {
                if (cameraPositionTarget != null)
                    return cameraPositionTarget;

                cameraPositionTarget = transform.Find("ToMove") ?? transform;
                return cameraPositionTarget;
            }
        }

        /// <summary>Original CamController takes rotation from the player/root transform.</summary>
        public Transform CameraRotationTarget => cameraRotationTarget != null ? cameraRotationTarget : transform;

        public WealthState WealthState => _wealth?.State ?? WealthState.Hobo;
        public Transform LastCheckpoint => _lastCheckpoint;

        public void Bind(GameFlow gameFlow, RichPoorController wealth, FinishProgress finishProgress)
        {
            _gameFlow = gameFlow;
            _wealth = wealth;
            _finishProgress = finishProgress;
            _lastCheckpoint = null;
            Feedback?.Bind(gameFlow, wealth);
        }

        public bool TryApplyWealth(int delta)
        {
            if (_gameFlow == null || _wealth == null || _gameFlow.State != GameFlowState.Playing)
                return false;

            _wealth.Add(delta);
            Feedback?.PlayWealthDelta(delta);
            return true;
        }

        public bool TryApplyChoiceWealth(int delta)
        {
            if (_gameFlow == null || _wealth == null || _gameFlow.State != GameFlowState.Playing)
                return false;

            _wealth.Add(delta);
            Feedback?.PlayChoice(delta);
            return true;
        }

        public bool TryReachFinishStep(int multiplier, bool completesRun)
        {
            if (_gameFlow == null || _finishProgress == null || _gameFlow.State != GameFlowState.Playing)
                return false;

            _finishProgress.Reach(multiplier);
            Feedback?.PlayFinish(multiplier, completesRun);
            if (completesRun)
                _gameFlow.Win();

            return true;
        }

        public bool TryPassStatusDoor(WealthState minimumState, int multiplier, bool millionaire)
        {
            if (_gameFlow == null || _wealth == null || _gameFlow.State != GameFlowState.Playing)
                return false;

            if (!millionaire && _wealth.State < minimumState)
            {
                // A multiplier gate is not a death condition. In the reference run, failing
                // to qualify for the NEXT status gate ends the finish ladder at the multiplier
                // already earned instead of showing the defeat/retry flow.
                var reached = _finishProgress != null ? _finishProgress.Multiplier : 1;
                Feedback?.PlayFinish(reached, true);
                _gameFlow.Win();
                return true;
            }

            _finishProgress?.Reach(multiplier);
            Feedback?.PlayFinish(multiplier, millionaire);
            if (millionaire)
                _gameFlow.Win();

            return true;
        }

        public void RegisterCheckpoint(Transform checkpoint)
        {
            if (_gameFlow != null && _gameFlow.State == GameFlowState.Playing && checkpoint != null)
                _lastCheckpoint = checkpoint;
        }

        public void PlayCheckpointFeedback()
        {
            Feedback?.PlayCheckpoint();
        }

        private RunnerFeedback Feedback
        {
            get
            {
                if (feedback == null)
                    feedback = GetComponent<RunnerFeedback>();
                return feedback;
            }
        }
    }
}
