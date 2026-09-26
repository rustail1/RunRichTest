using RunRich.Data.Configs;
using RunRich.Runtime.Camera;
using RunRich.Runtime.Finish;
using RunRich.Runtime.Flow;
using RunRich.Runtime.Level;
using RunRich.Runtime.Runner;
using RunRich.Runtime.UI;
using RunRich.Runtime.Wealth;
using UnityEngine;

namespace RunRich.Runtime.Bootstrap
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private LevelBindings[] authoredLevels;
        [SerializeField] private UnityEngine.Camera gameplayCamera;
        [SerializeField] private RunnerConfigSO runnerConfig;
        [SerializeField] private CameraConfigSO cameraConfig;
        [SerializeField] private WealthConfigSO wealthConfig;
        [SerializeField] private GameplayHud hud;

        private GameLoop _frameLoop;
        private GameLoop _fixedLoop;
        private ILevelService _levelService;
        private PointerRunnerInput _debugInput;
        private RunnerMotor _debugMotor;
        private GameFlow _debugGameFlow;
        private RichPoorController _debugWealth;
        private RunnerActor _debugRunner;

        public GameFlow CurrentFlow => _debugGameFlow;
        public RichPoorController CurrentWealth => _debugWealth;
        public RunnerActor CurrentRunner => _debugRunner;

        private void Awake()
        {
            if (authoredLevels == null || authoredLevels.Length == 0 || gameplayCamera == null ||
                runnerConfig == null || cameraConfig == null || wealthConfig == null || hud == null)
            {
                Debug.LogError("GameBootstrapper requires authored levels, camera, configs and HUD.", this);
                enabled = false;
                return;
            }

            _levelService = new AuthoredLevelService(authoredLevels);
            _levelService.Initialize();
            ComposeCurrentLevel();
        }

        private void Update()
        {
            // Pointer sampling, wealth interpolation and presentation stay on the render frame.
            // The original XAPK runs MovePlayer and CamController from FixedUpdate.
            _frameLoop?.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            // Keep movement first and camera second so CamController observes the runner pose
            // produced by the same fixed step, matching the original MovePlayer/CamController cadence.
            _fixedLoop?.Tick(Time.fixedDeltaTime);
        }

        private void OnGUI()
        {
            if (runnerConfig == null || !runnerConfig.ShowSwerveDebug ||
                _debugInput == null || _debugMotor == null || _debugGameFlow == null)
                return;

            var text =
                "SWERVE DEBUG\n" +
                $"Pointer pressed: {_debugInput.IsPressed}\n" +
                $"Pointer X: {_debugInput.PointerX:F2}\n" +
                $"Drag from down: {_debugInput.DragFromPointerDown:F4}\n" +
                $"Delta X: {_debugInput.DeltaX:F4}\n" +
                $"Current lateral X: {_debugMotor.CurrentLateral:F4}\n" +
                $"Target lateral X: {_debugMotor.TargetLateral:F4}\n" +
                $"Current lateral velocity: {_debugMotor.CurrentLateralVelocity:F4}\n" +
                $"Speed ratio: {_debugMotor.CurrentSpeedRatio:F3}\n" +
                $"Forward speed: {_debugMotor.CurrentForwardSpeed:F3}\n" +
                $"Visual target yaw: {_debugMotor.VisualTargetYaw:F2}\n" +
                $"Actual visual yaw: {_debugMotor.ActualVisualYaw:F2}\n" +
                $"Screen.width: {Screen.width}\n" +
                $"Screen.height: {Screen.height}\n" +
                $"Game state: {_debugGameFlow.State}";

            GUI.Box(new Rect(12f, 12f, 360f, 295f), text);
        }

        public void RestartLevel()
        {
            _levelService.Restart();
            ComposeCurrentLevel();
        }

        public void LoadNextLevel()
        {
            _levelService.LoadNext();
            ComposeCurrentLevel();
        }

        private void ComposeCurrentLevel()
        {
            var bindings = _levelService.CurrentLevel;
            if (bindings == null || bindings.Runner == null || bindings.Appearance == null)
            {
                Debug.LogError("Authored level requires complete LevelBindings.", this);
                enabled = false;
                return;
            }

            var pathPoints = bindings.GetPathPoints();
            if (pathPoints.Length < 2)
            {
                Debug.LogError("Authored level requires at least two path waypoints.", bindings);
                enabled = false;
                return;
            }

            var gameFlow = new GameFlow();
            var wealth = new RichPoorController(
                wealthConfig.InitialWealth,
                wealthConfig.MinimumWealth,
                wealthConfig.MaximumWealth,
                wealthConfig.PoorThreshold,
                wealthConfig.DecentThreshold,
                wealthConfig.RichThreshold,
                wealthConfig.MillionaireThreshold,
                wealthConfig.ChangeSpeed,
                wealthConfig.GameOverAtZero,
                wealthConfig.WaitOneMoreErrorAtZero);
            var finishProgress = new FinishProgress();
            var input = new PointerRunnerInput();

            wealth.Depleted += gameFlow.Lose;
            bindings.Runner.Bind(gameFlow, wealth, finishProgress);
            bindings.Appearance.Bind(wealth);
            bindings.AnimationView?.Bind(gameFlow, wealth);

            _frameLoop = new GameLoop();
            _fixedLoop = new GameLoop();

            // Dynamic input must be sampled on Update. RunnerMotor reads the latest sampled
            // state from the fixed loop; its press-edge fallback prevents a missed anchor
            // even if render and fixed rates do not line up one-to-one.
            _frameLoop.Register(input);
            _frameLoop.Register(wealth);

            var motor = new RunnerMotor(
                bindings.RunnerTransform,
                bindings.Runner.SteeringVisual,
                pathPoints,
                input,
                gameFlow,
                runnerConfig,
                wealth);

            // XAPK: MovePlayer.FixedUpdate first, then CamController.FixedUpdate.
            _fixedLoop.Register(motor);
            _fixedLoop.Register(new RunnerCamera(
                gameplayCamera,
                bindings.Runner.CameraPositionTarget,
                bindings.Runner.CameraRotationTarget,
                cameraConfig,
                gameFlow));

            _debugInput = input;
            _debugMotor = motor;
            _debugGameFlow = gameFlow;
            _debugWealth = wealth;
            _debugRunner = bindings.Runner;

            if (bindings.AnimationView != null)
                _frameLoop.Register(bindings.AnimationView);

            hud.Bind(bindings.LevelNumber, gameFlow, wealth, finishProgress, motor, RestartLevel, LoadNextLevel);

            if (hud.CanvasRect != null && hud.RunnerStatus?.RectTransform != null)
            {
                _frameLoop.Register(new RunnerStatusFollower(
                    gameplayCamera,
                    bindings.RunnerTransform,
                    hud.CanvasRect,
                    hud.RunnerStatus.RectTransform,
                    new Vector3(0f, 2.45f, 0f),
                    gameFlow));
            }
        }

        public void StartRunForValidation()
        {
            _debugGameFlow?.StartRun();
        }
    }
}
