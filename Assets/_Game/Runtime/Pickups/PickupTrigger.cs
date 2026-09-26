using RunRich.Data.Definitions;
using RunRich.Runtime.Runner;
using UnityEngine;

namespace RunRich.Runtime.Pickups
{
    [RequireComponent(typeof(Collider))]
    public sealed class PickupTrigger : MonoBehaviour
    {
        [SerializeField] private PickupDefinitionSO definition;

        private bool _consumed;

        private void OnEnable()
        {
            _consumed = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_consumed || definition == null)
            {
                return;
            }

            var runner = other.GetComponentInParent<RunnerActor>();
            if (runner == null || !runner.TryApplyWealth(definition.WealthDelta))
            {
                return;
            }

            _consumed = true;
            gameObject.SetActive(false);
        }
    }
}
