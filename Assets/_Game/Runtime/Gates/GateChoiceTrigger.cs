using RunRich.Runtime.Runner;
using UnityEngine;

namespace RunRich.Runtime.Gates
{
    [RequireComponent(typeof(Collider))]
    public sealed class GateChoiceTrigger : MonoBehaviour
    {
        [SerializeField] private GateChoiceGroup group;
        [SerializeField] private int wealthDelta;

        private void OnTriggerEnter(Collider other)
        {
            var runner = other.GetComponentInParent<RunnerActor>();
            if (runner != null)
            {
                group?.TryChoose(runner, wealthDelta);
            }
        }
    }
}
