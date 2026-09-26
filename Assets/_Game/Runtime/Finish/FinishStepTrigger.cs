using RunRich.Runtime.Runner;
using UnityEngine;

namespace RunRich.Runtime.Finish
{
    [RequireComponent(typeof(Collider))]
    public sealed class FinishStepTrigger : MonoBehaviour
    {
        [SerializeField, Min(1)] private int multiplier = 2;
        [SerializeField] private bool completesRun;

        private bool _consumed;

        private void OnEnable()
        {
            _consumed = false;
            var trigger = GetComponent<Collider>();
            if (trigger != null) trigger.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_consumed) return;

            var runner = other.GetComponentInParent<RunnerActor>();
            if (runner == null || !runner.TryReachFinishStep(multiplier, completesRun)) return;

            _consumed = true;
            GetComponent<Collider>().enabled = false;
        }
    }
}
