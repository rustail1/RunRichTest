using UnityEngine;

namespace RunRich.Runtime.Pickups
{
    public sealed class PickupPresentation : MonoBehaviour
    {
        [SerializeField] private Transform billboard;
        [SerializeField] private SpriteRenderer aura;
        [SerializeField] private SpriteRenderer marker;
        [SerializeField, Min(0f)] private float bobAmplitude = 0.035f;
        [SerializeField, Min(0.1f)] private float bobSpeed = 2.15f;
        [SerializeField, Range(0f, 0.25f)] private float pulseAmount = 0.055f;
        [SerializeField, Min(0.1f)] private float pulseSpeed = 2.75f;

        private Vector3 _baseLocalPosition;
        private Vector3 _baseMarkerScale;
        private Vector3 _baseAuraScale;
        private Color _baseAuraColor;
        private float _phase;
        private UnityEngine.Camera _camera;
        private bool _initialized;

        private void Awake() => CacheBaseState();

        private void OnEnable()
        {
            CacheBaseState();
            if (billboard != null) billboard.localPosition = _baseLocalPosition;
            if (marker != null) marker.transform.localScale = _baseMarkerScale;
            if (aura != null) aura.transform.localScale = _baseAuraScale;
            if (aura != null) aura.color = _baseAuraColor;
            _phase = Mathf.Repeat(Mathf.Abs(GetInstanceID()) * 0.017f, Mathf.PI * 2f);
        }

        private void LateUpdate()
        {
            if (billboard == null) return;
            if (_camera == null) _camera = UnityEngine.Camera.main;
            if (_camera != null) billboard.rotation = _camera.transform.rotation;

            var t = Time.time;
            var bob = Mathf.Sin(t * bobSpeed + _phase);
            billboard.localPosition = _baseLocalPosition + Vector3.up * (bob * bobAmplitude);

            var pulse = 1f + Mathf.Sin(t * pulseSpeed + _phase) * pulseAmount;
            if (marker != null) marker.transform.localScale = _baseMarkerScale * pulse;
            if (aura != null)
            {
                aura.transform.localScale = _baseAuraScale * (1f + (pulse - 1f) * 0.65f);
                var color = aura.color;
                color.a = _baseAuraColor.a * (0.9f + 0.2f * (0.5f + 0.5f * bob));
                aura.color = color;
            }
        }

        private void CacheBaseState()
        {
            if (_initialized) return;
            if (billboard != null) _baseLocalPosition = billboard.localPosition;
            if (marker != null) _baseMarkerScale = marker.transform.localScale;
            if (aura != null)
            {
                _baseAuraScale = aura.transform.localScale;
                _baseAuraColor = aura.color;
            }
            _initialized = true;
        }
    }
}
