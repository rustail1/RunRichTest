using System.Collections;
using UnityEngine;

namespace RunRich.Runtime.Finish
{
    /// <summary>
    /// Small deterministic presentation component for the authored finish gates.
    /// Gameplay remains in StatusDoorTrigger; this only slides the two visible leaves aside.
    /// </summary>
    public sealed class GateOpenVisual : MonoBehaviour
    {
        [SerializeField] private Transform leftLeaf;
        [SerializeField] private Transform rightLeaf;
        [SerializeField, Min(0f)] private float openDistance = 0.46f;
        [SerializeField, Min(0.01f)] private float duration = 0.32f;

        private Vector3 _leftClosed;
        private Vector3 _rightClosed;
        private bool _cached;
        private Coroutine _routine;

        private void Awake()
        {
            CacheClosedPose();
        }

        private void OnEnable()
        {
            ResetClosed();
        }

        public void ResetClosed()
        {
            CacheClosedPose();
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            if (leftLeaf != null) leftLeaf.localPosition = _leftClosed;
            if (rightLeaf != null) rightLeaf.localPosition = _rightClosed;
        }

        public void Open()
        {
            CacheClosedPose();
            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(OpenRoutine());
        }

        private void CacheClosedPose()
        {
            if (_cached) return;
            if (leftLeaf != null) _leftClosed = leftLeaf.localPosition;
            if (rightLeaf != null) _rightClosed = rightLeaf.localPosition;
            _cached = true;
        }

        private Vector3 ResolveLeafLocalOffset(Transform leaf, float visualLocalDistance)
        {
            if (leaf == null)
                return Vector3.zero;

            // Door_Million has nested imported transforms whose local X is not guaranteed to be
            // the gate's visual X. Move in the authored Visual frame and convert that displacement
            // back into the actual leaf-parent space. For the simple x2/x3/x4 leaves this resolves
            // to the same displacement; for x5 it guarantees outward motion.
            var motionFrame = transform.Find("Visual") ?? transform;
            var worldOffset = motionFrame.TransformVector(Vector3.right * visualLocalDistance);
            return leaf.parent != null
                ? leaf.parent.InverseTransformVector(worldOffset)
                : worldOffset;
        }

        private IEnumerator OpenRoutine()
        {
            var leftStart = leftLeaf != null ? leftLeaf.localPosition : Vector3.zero;
            var rightStart = rightLeaf != null ? rightLeaf.localPosition : Vector3.zero;
            var leftTarget = _leftClosed + ResolveLeafLocalOffset(leftLeaf, -openDistance);
            var rightTarget = _rightClosed + ResolveLeafLocalOffset(rightLeaf, openDistance);

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                t = t * t * (3f - 2f * t);
                if (leftLeaf != null) leftLeaf.localPosition = Vector3.LerpUnclamped(leftStart, leftTarget, t);
                if (rightLeaf != null) rightLeaf.localPosition = Vector3.LerpUnclamped(rightStart, rightTarget, t);
                yield return null;
            }

            if (leftLeaf != null) leftLeaf.localPosition = leftTarget;
            if (rightLeaf != null) rightLeaf.localPosition = rightTarget;
            _routine = null;
        }
    }
}
