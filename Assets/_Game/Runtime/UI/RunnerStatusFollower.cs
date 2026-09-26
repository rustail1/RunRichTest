using System;
using RunRich.Runtime.Bootstrap;
using RunRich.Runtime.Flow;
using UnityEngine;

namespace RunRich.Runtime.UI
{
    /// <summary>
    /// Moves an already-authored screen-space status widget above the runner. It creates nothing.
    /// </summary>
    public sealed class RunnerStatusFollower : ITickable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly Transform _runner;
        private readonly RectTransform _canvasRect;
        private readonly RectTransform _statusRect;
        private readonly Vector3 _worldOffset;
        private readonly GameFlow _flow;

        public RunnerStatusFollower(
            UnityEngine.Camera camera,
            Transform runner,
            RectTransform canvasRect,
            RectTransform statusRect,
            Vector3 worldOffset,
            GameFlow flow)
        {
            _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
            _runner = runner != null ? runner : throw new ArgumentNullException(nameof(runner));
            _canvasRect = canvasRect != null ? canvasRect : throw new ArgumentNullException(nameof(canvasRect));
            _statusRect = statusRect != null ? statusRect : throw new ArgumentNullException(nameof(statusRect));
            _worldOffset = worldOffset;
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        }

        public void Tick(float deltaTime)
        {
            // The HUD intentionally hides the runner status on Ready/Win/Lose. The follower must
            // not fight that decision by re-activating the RectTransform on the next frame.
            if (_flow.State != GameFlowState.Playing)
            {
                if (_statusRect.gameObject.activeSelf)
                    _statusRect.gameObject.SetActive(false);
                return;
            }

            var screen = _camera.WorldToScreenPoint(_runner.position + _worldOffset);
            if (screen.z <= 0f)
            {
                _statusRect.gameObject.SetActive(false);
                return;
            }

            if (!_statusRect.gameObject.activeSelf)
            {
                _statusRect.gameObject.SetActive(true);
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, null, out var local))
            {
                _statusRect.anchoredPosition = local;
            }
        }
    }
}
