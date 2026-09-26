using RunRich.Runtime.Runner;
using RunRich.Runtime.Wealth;
using UnityEngine;

namespace RunRich.Runtime.Finish
{
    [RequireComponent(typeof(Collider))]
    public sealed class StatusDoorTrigger : MonoBehaviour
    {
        [SerializeField] private WealthState minimumState;
        [SerializeField, Min(1)] private int multiplier = 1;
        [SerializeField] private bool millionaire;
        [SerializeField] private bool moveActive = true;
        [SerializeField] private GateOpenVisual gateVisual;

        private bool _consumed;

        public bool MoveActive => moveActive;

        private void OnEnable()
        {
            _consumed = false;
            gateVisual?.ResetClosed();
            var trigger = GetComponent<Collider>();
            if (trigger != null) trigger.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_consumed) return;

            var runner = other.GetComponentInParent<RunnerActor>();
            if (runner == null) return;

            var canPass = millionaire || runner.WealthState >= minimumState;
            _consumed = runner.TryPassStatusDoor(minimumState, multiplier, millionaire);
            if (_consumed)
            {
                if (canPass)
                    gateVisual?.Open();

                var trigger = GetComponent<Collider>();
                if (trigger != null) trigger.enabled = false;
            }
        }

        public bool TryActivate(RunnerActor runner)
        {
            if (_consumed || runner == null) return false;
            var canPass = millionaire || runner.WealthState >= minimumState;
            _consumed = runner.TryPassStatusDoor(minimumState, multiplier, millionaire);
            if (_consumed)
            {
                if (canPass)
                    gateVisual?.Open();

                var trigger = GetComponent<Collider>();
                if (trigger != null) trigger.enabled = false;
            }
            return _consumed;
        }
    }
}
