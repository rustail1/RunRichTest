using UnityEngine;

namespace RunRich.Runtime.UI
{
    /// <summary>Fits one RectTransform to Screen.safeArea without touching child authored layout.</summary>
    [ExecuteAlways]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake()
        {
            if (target == null) target = transform as RectTransform;
            Apply();
        }

        private void OnEnable() => Apply();

        private void Update()
        {
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (_lastSafeArea != Screen.safeArea || _lastScreen != screen)
                Apply();
        }

        private void Apply()
        {
            if (target == null || Screen.width <= 0 || Screen.height <= 0)
                return;

            var safe = Screen.safeArea;
            var min = safe.position;
            var max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            target.anchorMin = min;
            target.anchorMax = max;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;

            _lastSafeArea = safe;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
