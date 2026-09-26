using System.Collections;
using RunRich.Runtime.Runner;
using UnityEngine;

namespace RunRich.Runtime.Level
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Transform leftFlagVisual;
        [SerializeField] private Transform rightFlagVisual;
        [SerializeField, Min(0f)] private float flagUpDuration = 0.25f;

        private Vector3 _leftScale;
        private Vector3 _rightScale;
        private bool _passed;

        public bool IsPassed => _passed;

        private void Awake()
        {
            _leftScale = leftFlagVisual != null ? leftFlagVisual.localScale : Vector3.one;
            _rightScale = rightFlagVisual != null ? rightFlagVisual.localScale : Vector3.one;
        }

        private void OnEnable()
        {
            _passed = false;
            StopAllCoroutines();
            SetFlagAmount(0f);
            var trigger = GetComponent<BoxCollider>();
            if (trigger != null) trigger.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var runner = other.GetComponentInParent<RunnerActor>();
            if (runner != null) Pass(runner);
        }

        public bool Pass(RunnerActor runner)
        {
            if (_passed || runner == null) return false;
            _passed = true;
            runner.RegisterCheckpoint(transform);
            runner.PlayCheckpointFeedback();
            var trigger = GetComponent<BoxCollider>();
            if (trigger != null) trigger.enabled = false;
            StartCoroutine(FlagUp());
            return true;
        }

        private IEnumerator FlagUp()
        {
            if (flagUpDuration <= 0f)
            {
                SetFlagAmount(1f);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < flagUpDuration)
            {
                elapsed += Time.deltaTime;
                SetFlagAmount(Mathf.Clamp01(elapsed / flagUpDuration));
                yield return null;
            }
            SetFlagAmount(1f);
        }

        private void SetFlagAmount(float amount)
        {
            if (leftFlagVisual != null)
                leftFlagVisual.localScale = new Vector3(_leftScale.x, _leftScale.y * amount, _leftScale.z);
            if (rightFlagVisual != null)
                rightFlagVisual.localScale = new Vector3(_rightScale.x, _rightScale.y * amount, _rightScale.z);
        }
    }
}
