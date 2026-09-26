using System;
using System.Collections.Generic;
using RunRich.Data.Configs;
using RunRich.Runtime.Bootstrap;
using RunRich.Runtime.Flow;
using RunRich.Runtime.Wealth;
using UnityEngine;

namespace RunRich.Runtime.Runner
{
    /// <summary>
    /// XAPK-style runner movement: drag-from-down sets one persistent lateral target,
    /// SmoothDamp is the only lateral motion path, and live raw DeltaX drives visual yaw.
    /// </summary>
    public sealed class RunnerMotor : ITickable
    {
        private readonly Transform _runner;
        private readonly Transform _steeringVisual;
        private readonly Quaternion _steeringVisualBaseRotation;
        private readonly IRunnerInput _input;
        private readonly GameFlow _gameFlow;
        private readonly RunnerConfigSO _config;
        private readonly Vector3[] _samples;
        private readonly float[] _sampleDistances;
        private readonly float _totalLength;
        private readonly float _heightOffset;
        private readonly RichPoorController _wealth;

        private float _distance;
        private float _targetLateral;
        private float _currentLateral;
        private float _currentLateralVelocity;
        private float _lateralOnPointerDown;
        private float _visualTargetYaw;
        private float _startDelayRemaining;
        private float _currentSpeedRatio = 1f;
        private Vector3 _smoothedPathForward;
        private bool _wasInputPressed;

        public float CurrentLateral => _currentLateral;
        public float TargetLateral => _targetLateral;
        public float CurrentLateralVelocity => _currentLateralVelocity;
        public float VisualTargetYaw => _visualTargetYaw;
        public float CurrentSpeedRatio => _currentSpeedRatio;
        public float CurrentForwardSpeed => _config.ForwardSpeed * _currentSpeedRatio;
        public float Progress01 => _totalLength > 0.0001f ? Mathf.Clamp01(_distance / _totalLength) : 0f;
        public event Action<float> ProgressChanged;

        public float ActualVisualYaw
        {
            get
            {
                if (_steeringVisual == null)
                    return 0f;

                var relativeRotation = Quaternion.Inverse(_steeringVisualBaseRotation) * _steeringVisual.localRotation;
                return Mathf.DeltaAngle(0f, relativeRotation.eulerAngles.y);
            }
        }

        public RunnerMotor(
            Transform runner,
            Transform steeringVisual,
            Vector3[] pathPoints,
            IRunnerInput input,
            GameFlow gameFlow,
            RunnerConfigSO config,
            RichPoorController wealth = null)
        {
            _runner = runner != null ? runner : throw new ArgumentNullException(nameof(runner));
            _steeringVisual = steeringVisual;
            _steeringVisualBaseRotation = steeringVisual != null ? steeringVisual.localRotation : Quaternion.identity;
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _gameFlow = gameFlow ?? throw new ArgumentNullException(nameof(gameFlow));
            _config = config != null ? config : throw new ArgumentNullException(nameof(config));
            _wealth = wealth;

            if (pathPoints == null || pathPoints.Length < 2)
                throw new ArgumentException("Runner path requires at least two authored points.", nameof(pathPoints));

            BuildSmoothPath(pathPoints, config.CurveSamplesPerSegment, out _samples, out _sampleDistances, out _totalLength);
            _heightOffset = runner.position.y - pathPoints[0].y;
            _startDelayRemaining = config.StartDelay;

            SamplePath(0f, out _, out _smoothedPathForward);
            if (_smoothedPathForward.sqrMagnitude < 0.0001f)
                _smoothedPathForward = Vector3.forward;

            PlaceRunner(0f, 0f);
            ApplyVisualSteering(0f);
        }

        public void Tick(float deltaTime)
        {
            if (_gameFlow.State == GameFlowState.Ready && _input.StartRequested)
                _gameFlow.StartRun();

            if (_gameFlow.State != GameFlowState.Playing)
            {
                _currentLateralVelocity = 0f;
                _currentSpeedRatio = ResolveSpeedRatio();
                ApplyVisualSteering(deltaTime);
                _wasInputPressed = _input.IsPressed;
                return;
            }

            if (_startDelayRemaining > 0f)
            {
                _startDelayRemaining = Mathf.Max(0f, _startDelayRemaining - deltaTime);
                _wasInputPressed = _input.IsPressed;
                return;
            }

            // PointerRunnerInput is sampled on Update while movement runs on FixedUpdate.
            // Preserve the original pointer-down anchor even when render/fixed cadence differs:
            // use the explicit frame pulse when available, otherwise detect the held-state edge.
            var pointerPressedThisStep = _input.PressedThisFrame || (_input.IsPressed && !_wasInputPressed);
            if (pointerPressedThisStep)
            {
                _lateralOnPointerDown = _currentLateral;
                _targetLateral = _currentLateral;
            }

            ResolveLateralBounds(out var minLateral, out var maxLateral);

            if (_input.IsPressed)
            {
                var dragOffset = _input.DragFromPointerDown * _config.SwerveSensitivity;
                var rawTarget = _lateralOnPointerDown + dragOffset;
                _targetLateral = Mathf.Clamp(rawTarget, minLateral, maxLateral);

                // XAPK behavior: when the target hits a bound, move the world anchor too.
                // This removes the dead-zone that would otherwise appear when reversing the pointer.
                if (!Mathf.Approximately(rawTarget, _targetLateral))
                    _lateralOnPointerDown = _targetLateral - dragOffset;
            }
            else
            {
                // Release keeps TargetWorldX; only live input delta is cleared by PointerRunnerInput.
                _targetLateral = Mathf.Clamp(_targetLateral, minLateral, maxLateral);
            }

            _currentSpeedRatio = ResolveSpeedRatio();
            _currentLateral = Mathf.SmoothDamp(
                _currentLateral,
                _targetLateral,
                ref _currentLateralVelocity,
                _config.LateralSmoothTime,
                _config.LateralMaxSpeed * _currentSpeedRatio,
                deltaTime);

            _distance = Mathf.Min(_totalLength, _distance + CurrentForwardSpeed * deltaTime);
            PlaceRunner(_distance, deltaTime);
            ProgressChanged?.Invoke(Progress01);
            ApplyVisualSteering(deltaTime);
            _wasInputPressed = _input.IsPressed;
        }

        private float ResolveSpeedRatio()
        {
            if (!_config.SlowSpeedWhenPoor || _wealth == null)
                return 1f;

            return _wealth.SpeedRatio;
        }

        private void ResolveLateralBounds(out float minLateral, out float maxLateral)
        {
            minLateral = -_config.RoadHalfWidth;
            maxLateral = _config.RoadHalfWidth;

            var wallMask = _config.WallLayerMask.value;
            if (wallMask == 0 || _config.WallRaycastDistance <= 0f)
                return;

            // Use the same smoothed path frame that drives player rotation.
            // Using the raw sampled tangent here can rotate the lateral basis a frame
            // ahead of the player at 90-degree turns and makes the camera appear to snag.
            var boundsForward = _smoothedPathForward.sqrMagnitude > 0.0001f
                ? _smoothedPathForward
                : Vector3.forward;
            var right = Vector3.Cross(Vector3.up, boundsForward).normalized;
            if (right.sqrMagnitude < 0.0001f)
                return;

            var origin = _runner.position;
            var padding = Mathf.Max(0f, _config.WallRaycastPadding);
            var rayDistance = _config.WallRaycastDistance;

            if (Physics.Raycast(origin, right, out var hitRight, rayDistance, _config.WallLayerMask, QueryTriggerInteraction.Ignore))
            {
                var rightBound = _currentLateral + Mathf.Max(0f, hitRight.distance - padding);
                maxLateral = Mathf.Min(maxLateral, rightBound);
            }

            if (Physics.Raycast(origin, -right, out var hitLeft, rayDistance, _config.WallLayerMask, QueryTriggerInteraction.Ignore))
            {
                var leftBound = _currentLateral - Mathf.Max(0f, hitLeft.distance - padding);
                minLateral = Mathf.Max(minLateral, leftBound);
            }

            if (minLateral > maxLateral)
            {
                var midpoint = (minLateral + maxLateral) * 0.5f;
                minLateral = midpoint;
                maxLateral = midpoint;
            }
        }

        private void PlaceRunner(float distance, float deltaTime)
        {
            SamplePath(distance, out var center, out var pathForward);

            if (deltaTime <= 0f || _config.PathRotationSmoothTime <= 0f)
            {
                _smoothedPathForward = pathForward;
            }
            else
            {
                var t = 1f - Mathf.Exp(-deltaTime / Mathf.Max(0.0001f, _config.PathRotationSmoothTime));
                _smoothedPathForward = Vector3.Slerp(_smoothedPathForward, pathForward, t).normalized;
            }

            // IMPORTANT: position and rotation must use one coherent path frame.
            // The authored turn is already an arc, but if lateral offset uses the raw
            // tangent while rotation uses the smoothed tangent, the runner shifts
            // sideways at each turn sample. The camera follows that positional jump
            // and it looks like it is catching on the corner.
            var right = Vector3.Cross(Vector3.up, _smoothedPathForward).normalized;
            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;

            var position = center + Vector3.up * _heightOffset + right * _currentLateral;
            _runner.SetPositionAndRotation(position, Quaternion.LookRotation(_smoothedPathForward, Vector3.up));
        }

        private void ApplyVisualSteering(float deltaTime)
        {
            var inputDirection = !_input.IsPressed || Mathf.Approximately(_input.DeltaX, 0f)
                ? 0f
                : _input.DeltaX > 0f ? 1f : -1f;
            _visualTargetYaw = inputDirection * _config.MaxTurnAngle;

            if (_steeringVisual == null)
                return;

            var targetRotation = _steeringVisualBaseRotation * Quaternion.Euler(0f, _visualTargetYaw, 0f);

            if (deltaTime <= 0f || _config.VisualRotationSpeed <= 0f)
            {
                _steeringVisual.localRotation = targetRotation;
                return;
            }

            _steeringVisual.localRotation = Quaternion.Lerp(
                _steeringVisual.localRotation,
                targetRotation,
                deltaTime * _config.VisualRotationSpeed);
        }

        private void SamplePath(float distance, out Vector3 position, out Vector3 forward)
        {
            var clamped = Mathf.Clamp(distance, 0f, _totalLength);
            var upper = Array.BinarySearch(_sampleDistances, clamped);
            if (upper < 0) upper = ~upper;

            if (upper <= 0)
            {
                position = _samples[0];
                forward = (_samples[1] - _samples[0]).normalized;
                return;
            }

            if (upper >= _samples.Length)
            {
                position = _samples[^1];
                forward = (_samples[^1] - _samples[^2]).normalized;
                return;
            }

            var lower = upper - 1;
            var length = _sampleDistances[upper] - _sampleDistances[lower];
            var t = length > 0.0001f ? (clamped - _sampleDistances[lower]) / length : 0f;
            position = Vector3.LerpUnclamped(_samples[lower], _samples[upper], t);

            var previous = Mathf.Max(0, lower - 1);
            var next = Mathf.Min(_samples.Length - 1, upper + 1);
            var tangent = _samples[next] - _samples[previous];
            forward = tangent.sqrMagnitude > 0.0001f ? tangent.normalized : Vector3.forward;
        }

        private static void BuildSmoothPath(
            Vector3[] controlPoints,
            int samplesPerSegment,
            out Vector3[] samples,
            out float[] distances,
            out float totalLength)
        {
            samplesPerSegment = Mathf.Clamp(samplesPerSegment, 8, 64);
            var result = new List<Vector3>((controlPoints.Length - 1) * samplesPerSegment + 1);

            // The authored turn waypoints already describe the original ~3 m quarter-turns.
            // Do not merely subdivide their straight chords: that leaves a small velocity
            // direction discontinuity at every waypoint, which the 0.01 s camera follow
            // faithfully exposes as a tiny snag. Centripetal Catmull-Rom keeps the same
            // authored points while making the centerline C1-continuous through the turn.
            for (var segment = 0; segment < controlPoints.Length - 1; segment++)
            {
                var p1 = controlPoints[segment];
                var p2 = controlPoints[segment + 1];
                var p0 = segment > 0
                    ? controlPoints[segment - 1]
                    : p1 + (p1 - p2);
                var p3 = segment + 2 < controlPoints.Length
                    ? controlPoints[segment + 2]
                    : p2 + (p2 - p1);

                for (var step = 0; step < samplesPerSegment; step++)
                {
                    var u = step / (float)samplesPerSegment;
                    result.Add(EvaluateCentripetalCatmullRom(p0, p1, p2, p3, u));
                }
            }

            result.Add(controlPoints[^1]);
            samples = result.ToArray();
            distances = new float[samples.Length];
            totalLength = 0f;

            for (var i = 1; i < samples.Length; i++)
            {
                totalLength += Vector3.Distance(samples[i - 1], samples[i]);
                distances[i] = totalLength;
            }
        }

        private static Vector3 EvaluateCentripetalCatmullRom(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float u)
        {
            const float alpha = 0.5f;
            const float epsilon = 0.0001f;

            var t0 = 0f;
            var t1 = t0 + Mathf.Pow(Mathf.Max(epsilon, Vector3.Distance(p0, p1)), alpha);
            var t2 = t1 + Mathf.Pow(Mathf.Max(epsilon, Vector3.Distance(p1, p2)), alpha);
            var t3 = t2 + Mathf.Pow(Mathf.Max(epsilon, Vector3.Distance(p2, p3)), alpha);
            var t = Mathf.Lerp(t1, t2, Mathf.Clamp01(u));

            var a1 = InterpolateByTime(p0, p1, t0, t1, t);
            var a2 = InterpolateByTime(p1, p2, t1, t2, t);
            var a3 = InterpolateByTime(p2, p3, t2, t3, t);

            var b1 = InterpolateByTime(a1, a2, t0, t2, t);
            var b2 = InterpolateByTime(a2, a3, t1, t3, t);
            return InterpolateByTime(b1, b2, t1, t2, t);
        }

        private static Vector3 InterpolateByTime(
            Vector3 a,
            Vector3 b,
            float ta,
            float tb,
            float t)
        {
            var length = tb - ta;
            if (Mathf.Abs(length) <= 0.000001f)
                return b;

            var blend = (t - ta) / length;
            return Vector3.LerpUnclamped(a, b, blend);
        }
    }
}
