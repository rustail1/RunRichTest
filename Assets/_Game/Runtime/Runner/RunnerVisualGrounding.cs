using UnityEngine;

namespace RunRich.Runtime.Runner
{
    /// <summary>Aligns the animated visual to the authored road without moving the runner root.</summary>
    public sealed class RunnerVisualGrounding : MonoBehaviour
    {
        [SerializeField] private Transform road;

        private MeshRenderer[] _roadSegments;
        private SkinnedMeshRenderer[] _characterMeshes;
        private float _verticalVelocity;
        private float _initialLocalY;

        private void Awake() => _initialLocalY = transform.localPosition.y;

        public void ResetForRun()
        {
            _verticalVelocity = 0f;
            var position = transform.localPosition;
            position.y = _initialLocalY;
            transform.localPosition = position;
        }

        private void OnDisable() => _verticalVelocity = 0f;

        private void LateUpdate()
        {
            if (road == null) return;
            if (_roadSegments == null) _roadSegments = road.GetComponentsInChildren<MeshRenderer>(true);
            if (_characterMeshes == null) _characterMeshes = GetComponentsInChildren<SkinnedMeshRenderer>(true);

            SkinnedMeshRenderer character = null;
            foreach (var mesh in _characterMeshes)
            {
                if (mesh.gameObject.activeInHierarchy)
                {
                    character = mesh;
                    break;
                }
            }
            if (character == null) return;

            var position = transform.position;
            var found = false;
            var roadY = float.NegativeInfinity;
            foreach (var segment in _roadSegments)
            {
                var bounds = segment.bounds;
                if (position.x < bounds.min.x || position.x > bounds.max.x ||
                    position.z < bounds.min.z || position.z > bounds.max.z)
                    continue;

                roadY = Mathf.Max(roadY, bounds.max.y);
                found = true;
            }
            if (!found) return;

            var current = transform.localPosition;
            var targetY = current.y + roadY - character.bounds.min.y;
            current.y = Mathf.SmoothDamp(current.y, targetY, ref _verticalVelocity, 0.08f, 2.5f);
            transform.localPosition = current;
        }
    }
}
