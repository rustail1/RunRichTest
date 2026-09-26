#if UNITY_EDITOR
using System;
using RunRich.Data.Configs;
using RunRich.Runtime.Flow;
using RunRich.Runtime.Runner;
using UnityEditor;
using UnityEngine;

namespace RunRich.EditorTools
{
    public static class SwerveDeterministicVerifier
    {
        private const float DeltaTime = 1f / 60f;
        private const float ScreenHeight = 1000f;
        private const string ConfigPath = "Assets/_Game/Data/Configs/RunnerConfig.asset";

        [MenuItem("Run Rich/Verification/Run Swerve Deterministic Checks")]
        public static void Run()
        {
            var passed = 0;
            RunCase("stationary hold", StationaryHold, ref passed);
            RunCase("small right movement", SmallRightMovement, ref passed);
            RunCase("drag from center", DragFromCenter, ref passed);
            RunCase("edge to edge", EdgeToEdge, ref passed);
            RunCase("stationary finger while moving", StationaryFingerWhileMoving, ref passed);
            RunCase("release preserves target", ReleasePreservesTarget, ref passed);
            RunCase("visual root only", VisualRootOnly, ref passed);
            RunCase("runtime config values", RuntimeConfigValues, ref passed);

            Debug.Log($"SWERVE_DETERMINISTIC: PASS ({passed}/8)");
        }

        private static void StationaryHold()
        {
            using var fixture = new Fixture();
            fixture.Sample(true, 500f);

            for (var frame = 0; frame < 10; frame++)
                fixture.Sample(true, 500f);

            AssertClose(0f, fixture.Input.DeltaX, "DeltaX");
            AssertClose(0f, fixture.Motor.TargetLateral, "TargetX");
            AssertClose(0f, fixture.Motor.VisualTargetYaw, "Visual target yaw");
        }

        private static void SmallRightMovement()
        {
            using var fixture = new Fixture();
            fixture.Sample(true, 500f);
            fixture.Sample(true, 510f);

            AssertClose(10f, fixture.Input.DeltaX, "DeltaX");
            AssertClose(0.01f, fixture.Input.DragFromPointerDown, "DragFromPointerDown");
            AssertClose(0.2f, fixture.Motor.TargetLateral, "TargetX");
        }

        private static void DragFromCenter()
        {
            using var fixture = new Fixture();
            fixture.Sample(true, 500f);
            fixture.Sample(true, 625f);

            AssertClose(0.125f, fixture.Input.DragFromPointerDown, "DragFromPointerDown");
            AssertClose(2.5f, fixture.Motor.TargetLateral, "TargetX");
        }

        private static void EdgeToEdge()
        {
            using var fixture = new Fixture();
            fixture.Sample(true, 500f);
            fixture.Sample(true, 375f);

            for (var frame = 0; frame < 240; frame++)
            {
                fixture.Sample(true, 375f);
                AssertWithinBounds(fixture.Motor.CurrentLateral);
            }

            AssertClose(-2.5f, fixture.Motor.CurrentLateral, "Left-edge CurrentX", 0.001f);

            fixture.Sample(false, 375f);
            fixture.Sample(true, 500f);
            fixture.Sample(true, 1000f);

            AssertClose(2.5f, fixture.Motor.TargetLateral, "Right-edge TargetX");
            for (var frame = 0; frame < 240; frame++)
            {
                fixture.Sample(true, 1000f);
                AssertWithinBounds(fixture.Motor.CurrentLateral);
                AssertWithinBounds(fixture.Motor.TargetLateral);
            }
        }

        private static void StationaryFingerWhileMoving()
        {
            using var fixture = new Fixture();
            fixture.Sample(true, 500f);
            fixture.Sample(true, 625f);
            var previousCurrent = fixture.Motor.CurrentLateral;

            fixture.Sample(true, 625f);

            AssertClose(0f, fixture.Input.DeltaX, "DeltaX");
            AssertClose(2.5f, fixture.Motor.TargetLateral, "TargetX");
            AssertTrue(fixture.Motor.TargetLateral > fixture.Motor.CurrentLateral, "TargetX must remain ahead of CurrentX.");
            AssertTrue(fixture.Motor.CurrentLateral > previousCurrent, "CurrentX must continue approaching TargetX.");
            AssertClose(0f, fixture.Motor.VisualTargetYaw, "Visual target yaw");
        }

        private static void ReleasePreservesTarget()
        {
            using var fixture = new Fixture();
            fixture.Sample(true, 500f);
            fixture.Sample(true, 625f);

            var previousCurrent = fixture.Motor.CurrentLateral;
            var expectedVelocity = fixture.Motor.CurrentLateralVelocity;
            var target = fixture.Motor.TargetLateral;
            var expectedCurrent = Mathf.SmoothDamp(
                previousCurrent,
                target,
                ref expectedVelocity,
                fixture.Config.LateralSmoothTime,
                fixture.Config.LateralMaxSpeed,
                DeltaTime);

            fixture.Sample(false, 625f);

            AssertTrue(!fixture.Input.IsPressed, "IsPressed must be false after release.");
            AssertClose(0f, fixture.Input.DeltaX, "DeltaX");
            AssertClose(target, fixture.Motor.TargetLateral, "TargetX");
            AssertClose(expectedCurrent, fixture.Motor.CurrentLateral, "SmoothDamp CurrentX", 0.00001f);
            AssertClose(expectedVelocity, fixture.Motor.CurrentLateralVelocity, "SmoothDamp velocity", 0.00001f);
            AssertTrue(fixture.Motor.CurrentLateral > previousCurrent, "CurrentX must continue after release.");
        }

        private static void VisualRootOnly()
        {
            using var fixture = new Fixture();
            fixture.Sample(true, 500f);
            fixture.Sample(true, 510f);

            AssertClose(35f, fixture.Motor.VisualTargetYaw, "Visual target yaw");
            AssertTrue(fixture.Motor.ActualVisualYaw > 0f, "VisualRoot must receive positive steering yaw.");
            AssertClose(0f, Mathf.DeltaAngle(0f, fixture.Runner.transform.eulerAngles.y), "Player root input yaw", 0.001f);

            fixture.Sample(true, 510f);
            AssertClose(0f, fixture.Motor.VisualTargetYaw, "Stationary visual target yaw");
            AssertClose(0f, Mathf.DeltaAngle(0f, fixture.Runner.transform.eulerAngles.y), "Player root input yaw", 0.001f);
        }

        private static void RuntimeConfigValues()
        {
            var config = LoadConfig();
            AssertClose(20f, config.SwerveSensitivity, "Sensitivity");
            AssertClose(2.5f, config.RoadHalfWidth, "RoadHalfWidth");
            AssertClose(8f, config.ForwardSpeed, "ForwardSpeed");
            AssertClose(80f, config.LateralMaxSpeed, "LateralMaxSpeed");
            AssertClose(0.1f, config.LateralSmoothTime, "LateralSmoothTime");
            AssertClose(35f, config.MaxTurnAngle, "MaxTurnAngle");
            AssertClose(10f, config.VisualRotationSpeed, "VisualRotationSpeed");
        }

        private static void RunCase(string name, Action test, ref int passed)
        {
            try
            {
                test();
                passed++;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Swerve deterministic check failed: {name}.", exception);
            }
        }

        private static RunnerConfigSO LoadConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<RunnerConfigSO>(ConfigPath);
            if (config == null)
                throw new InvalidOperationException($"Missing RunnerConfigSO at {ConfigPath}.");

            return config;
        }

        private static void AssertWithinBounds(float value)
        {
            AssertTrue(value >= -2.5001f && value <= 2.5001f, $"Lateral value {value} escaped [-2.5, 2.5].");
        }

        private static void AssertClose(float expected, float actual, string label, float tolerance = 0.0001f)
        {
            if (Mathf.Abs(expected - actual) > tolerance)
                throw new InvalidOperationException($"{label}: expected {expected}, actual {actual}.");
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private sealed class Fixture : IDisposable
        {
            public Fixture()
            {
                Config = LoadConfig();
                Input = new PointerRunnerInput();
                Flow = new GameFlow();
                Runner = new GameObject("SwerveTestPlayer") { hideFlags = HideFlags.HideAndDontSave };
                Visual = new GameObject("VisualRoot") { hideFlags = HideFlags.HideAndDontSave };
                Visual.transform.SetParent(Runner.transform, false);
                Motor = new RunnerMotor(
                    Runner.transform,
                    Visual.transform,
                    new[] { Vector3.zero, new Vector3(0f, 0f, 1000f) },
                    Input,
                    Flow,
                    Config);
            }

            public RunnerConfigSO Config { get; }
            public PointerRunnerInput Input { get; }
            public GameFlow Flow { get; }
            public GameObject Runner { get; }
            public GameObject Visual { get; }
            public RunnerMotor Motor { get; }

            public void Sample(bool pressed, float pointerX)
            {
                Input.ProcessSample(pressed, pointerX, ScreenHeight);
                Motor.Tick(DeltaTime);
            }

            public void Dispose()
            {
                if (Runner != null)
                    UnityEngine.Object.DestroyImmediate(Runner);
            }
        }
    }
}
#endif
