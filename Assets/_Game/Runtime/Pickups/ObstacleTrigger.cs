using RunRich.Runtime.Runner;
using UnityEngine;

namespace RunRich.Runtime.Pickups
{
    [RequireComponent(typeof(Collider))]
    public sealed class ObstacleTrigger : MonoBehaviour
    {
        [SerializeField] private int wealthDelta = -20;

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
            if (runner == null || !runner.TryApplyWealth(wealthDelta)) return;

            _consumed = true;
            GetComponent<Collider>().enabled = false;
        }
    }
}
