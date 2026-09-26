using UnityEngine;

namespace RunRich.Runtime.UI
{
    public sealed class TutorialSwipeAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform finger;
        [SerializeField, Min(10f)] private float travel = 150f;
        [SerializeField, Min(0.2f)] private float cycleSeconds = 1.05f;

        private Vector2 _basePosition;
        private float _elapsed;
        private bool _initialized;

        private void Awake() => CacheBasePosition();

        private void OnEnable()
        {
            CacheBasePosition();
            _elapsed = 0f;
            if (finger != null)
            {
                finger.anchoredPosition = _basePosition;
                finger.localScale = Vector3.one;
            }
        }

        private void OnDisable()
        {
            if (finger != null)
            {
                finger.anchoredPosition = _basePosition;
                finger.localScale = Vector3.one;
            }
        }

        private void Update()
        {
            if (finger == null) return;
            _elapsed += Time.unscaledDeltaTime;
            var phase = Mathf.Repeat(_elapsed / cycleSeconds, 1f);
            var eased = phase * phase * (3f - 2f * phase);
            finger.anchoredPosition = _basePosition + new Vector2(Mathf.Lerp(-travel, travel, eased), 0f);
            finger.localScale = Vector3.one * (1f + Mathf.Sin(phase * Mathf.PI) * 0.06f);
        }

        private void CacheBasePosition()
        {
            if (_initialized || finger == null) return;
            _basePosition = finger.anchoredPosition;
            _initialized = true;
        }
    }
}
