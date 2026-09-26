using System;
using RunRich.Data.Configs;
using RunRich.Runtime.Bootstrap;
using RunRich.Runtime.Flow;
using UnityEngine;

namespace RunRich.Runtime.Camera
{
    /// <summary>
    /// Custom XAPK-style CamCont/DistCam camera.
    /// Position follows the MovePlayer/ToMove equivalent; rotation follows the player root.
    /// </summary>
    public sealed class RunnerCamera : ITickable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly Transform _cameraTransform;
        private readonly Transform _distCam;
        private readonly Transform _camCont;
        private readonly Transform _positionTarget;
        private readonly Transform _rotationTarget;
        private readonly CameraConfigSO _config;
        private readonly GameFlow _gameFlow;
        private Vector3 _followVelocity;
        private GameFlowState _state;
        private float _winElapsed;
        private float _transitionElapsed;
        private float _transitionDuration;
        private Vector3 _poseFromPosition;
        private Quaternion _poseFromRotation;
        private Vector3 _poseToPosition;
        private Quaternion _poseToRotation;

        public RunnerCamera(
            UnityEngine.Camera camera,
            Transform positionTarget,
            Transform rotationTarget,
            CameraConfigSO config,
            GameFlow gameFlow)
        {
            _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
            _positionTarget = positionTarget != null ? positionTarget : throw new ArgumentNullException(nameof(positionTarget));
            _rotationTarget = rotationTarget != null ? rotationTarget : throw new ArgumentNullException(nameof(rotationTarget));
            _config = config != null ? config : throw new ArgumentNullException(nameof(config));
            _gameFlow = gameFlow ?? throw new ArgumentNullException(nameof(gameFlow));
            _cameraTransform = _camera.transform;
            _distCam = _cameraTransform.parent;
            _camCont = _distCam != null ? _distCam.parent : null;

            if (_distCam == null || _camCont == null || _distCam.name != "DistCam" || _camCont.name != "CamCont")
                throw new InvalidOperationException("Gameplay camera requires CameraRig/CamCont/DistCam/MainCamera hierarchy.");

            _camera.fieldOfView = _config.FieldOfView;
            _camera.nearClipPlane = _config.NearClipPlane;
            _camera.farClipPlane = _config.FarClipPlane;

            // A previous victory run rotates DistCam for the orbit shot. The camera rig is shared
            // between authored levels, so every new composition must restore its neutral child pose
            // before following the next/restarted runner. Otherwise Next Level can inherit the old yaw.
            _distCam.localPosition = Vector3.zero;
            _distCam.localRotation = Quaternion.identity;
            _followVelocity = Vector3.zero;

            _state = _gameFlow.State;
            SnapFollow();
            ResolvePose(_state, out _poseToPosition, out _poseToRotation);
            _cameraTransform.localPosition = _poseToPosition;
            _cameraTransform.localRotation = _poseToRotation;
        }

        public void Tick(float deltaTime)
        {
            if (_state != _gameFlow.State)
            {
                var previous = _state;
                _state = _gameFlow.State;
                _winElapsed = 0f;
                BeginPoseTransition(previous, _state);
            }

            FollowTarget(deltaTime);
            UpdatePoseTransition(deltaTime);

            if (_state == GameFlowState.Won)
            {
                _winElapsed = Mathf.Min(_config.WinOrbitDuration, _winElapsed + Mathf.Max(0f, deltaTime));
                var yaw = _config.WinOrbitDuration <= 0f
                    ? 360f
                    : 360f * (_winElapsed / _config.WinOrbitDuration);
                _distCam.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }
        }

        private void FollowTarget(float deltaTime)
        {
            var desired = _positionTarget.position;
            if (!_config.FollowTargetOnX)
                desired.x = _camCont.position.x;

            _camCont.position = _config.FollowSmoothTime <= 0f
                ? desired
                : Vector3.SmoothDamp(
                    _camCont.position,
                    desired,
                    ref _followVelocity,
                    _config.FollowSmoothTime,
                    Mathf.Infinity,
                    deltaTime);

            // Native CamController copies player/root rotation rather than steering-visual rotation.
            _camCont.rotation = _rotationTarget.rotation;
        }

        private void SnapFollow()
        {
            _camCont.position = _positionTarget.position;
            _camCont.rotation = _rotationTarget.rotation;
        }

        private void BeginPoseTransition(GameFlowState previous, GameFlowState next)
        {
            _poseFromPosition = _cameraTransform.localPosition;
            _poseFromRotation = _cameraTransform.localRotation;
            ResolvePose(next, out _poseToPosition, out _poseToRotation);
            _transitionElapsed = 0f;
            _transitionDuration = next switch
            {
                GameFlowState.Playing when previous == GameFlowState.Ready => 0.75f,
                GameFlowState.Playing when previous == GameFlowState.Lost => 0.25f,
                GameFlowState.Won => 0.5f,
                GameFlowState.Lost => 0.5f,
                _ => 0.25f
            };

            _distCam.localPosition = Vector3.zero;
            if (next != GameFlowState.Won)
                _distCam.localRotation = Quaternion.identity;
        }

        private void UpdatePoseTransition(float deltaTime)
        {
            if (_transitionDuration <= 0f)
            {
                _cameraTransform.localPosition = _poseToPosition;
                _cameraTransform.localRotation = _poseToRotation;
                return;
            }

            _transitionElapsed = Mathf.Min(_transitionDuration, _transitionElapsed + Mathf.Max(0f, deltaTime));
            var t = Mathf.Clamp01(_transitionElapsed / _transitionDuration);
            _cameraTransform.localPosition = Vector3.Lerp(_poseFromPosition, _poseToPosition, t);
            _cameraTransform.localRotation = Quaternion.Slerp(_poseFromRotation, _poseToRotation, t);
        }

        private void ResolvePose(GameFlowState state, out Vector3 position, out Quaternion rotation)
        {
            switch (state)
            {
                case GameFlowState.Ready:
                    position = _config.StartLocalPosition;
                    rotation = Quaternion.Euler(_config.StartLocalEuler);
                    return;
                case GameFlowState.Lost:
                    position = _config.GameOverLocalPosition;
                    rotation = Quaternion.Euler(_config.GameOverLocalEuler);
                    return;
                default:
                    position = _config.GameLocalPosition;
                    rotation = Quaternion.Euler(_config.GameLocalEuler);
                    return;
            }
        }
    }
}
