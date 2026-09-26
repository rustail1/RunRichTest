using System.Collections.Generic;
using RunRich.Runtime.Bootstrap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RunRich.Runtime.Runner
{
    /// <summary>
    /// One mouse/touch input path. Every sample reports only the current horizontal pointer delta;
    /// movement owns the persistent target and continues approaching it after release.
    /// </summary>
    public sealed class PointerRunnerInput : IRunnerInput, ITickable
    {
        private readonly List<RaycastResult> _uiHits = new(8);

        private bool _tracking;
        private bool _blockedUntilRelease;
        private float _downPointerX;
        private float _lastPointerX;

        public bool StartRequested { get; private set; }
        public bool IsPressed { get; private set; }
        public bool PressedThisFrame { get; private set; }
        public float PointerX { get; private set; }
        public float DragFromPointerDown { get; private set; }
        public float DeltaX { get; private set; }

        public void Tick(float deltaTime)
        {
            ReadPointer(out var pressed, out var position);
            var pointerOverUi = pressed && !_tracking && IsPointerOverUi(position);
            ProcessSample(pressed, position.x, Screen.height, pointerOverUi);
        }

        /// <summary>
        /// Deterministic sample entry point shared by runtime input and controller tests.
        /// No OS mouse/touch simulation is required.
        /// </summary>
        public void ProcessSample(bool pressed, float pointerX, float screenHeight, bool pointerOverUi = false)
        {
            StartRequested = false;
            PressedThisFrame = false;
            DeltaX = 0f;
            PointerX = pointerX;

            if (!pressed)
            {
                _tracking = false;
                _blockedUntilRelease = false;
                IsPressed = false;
                DragFromPointerDown = 0f;
                return;
            }

            if (!_tracking)
            {
                _tracking = true;
                _downPointerX = pointerX;
                _lastPointerX = pointerX;
                DragFromPointerDown = 0f;
                _blockedUntilRelease = pointerOverUi;
                IsPressed = !_blockedUntilRelease;

                if (!_blockedUntilRelease)
                {
                    PressedThisFrame = true;
                    StartRequested = true;
                }

                return;
            }

            if (_blockedUntilRelease)
            {
                IsPressed = false;
                return;
            }

            IsPressed = true;
            var height = Mathf.Max(1f, screenHeight);
            DragFromPointerDown = (pointerX - _downPointerX) / height;
            DeltaX = pointerX - _lastPointerX;
            _lastPointerX = pointerX;
        }

        private bool IsPointerOverUi(Vector2 screenPosition)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            _uiHits.Clear();
            var eventData = new PointerEventData(eventSystem) { position = screenPosition };
            eventSystem.RaycastAll(eventData, _uiHits);
            return _uiHits.Count > 0;
        }

        private static void ReadPointer(out bool pressed, out Vector2 position)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                position = touch.position.ReadValue();
                pressed = touch.press.isPressed;

                if (pressed || touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.None)
                {
                    return;
                }
            }

            if (Mouse.current != null)
            {
                pressed = Mouse.current.leftButton.isPressed;
                position = Mouse.current.position.ReadValue();
                return;
            }

            pressed = false;
            position = Vector2.zero;
        }
    }
}
