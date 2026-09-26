using RunRich.Runtime.Runner;
using UnityEngine;

namespace RunRich.Runtime.Gates
{
    public sealed class GateChoiceGroup : MonoBehaviour
    {
        [SerializeField] private Collider[] choiceColliders;

        private bool _resolved;

        private void OnEnable()
        {
            _resolved = false;
            SetCollidersEnabled(true);
        }

        public bool TryChoose(RunnerActor runner, int wealthDelta)
        {
            if (_resolved || runner == null || !runner.TryApplyChoiceWealth(wealthDelta))
            {
                return false;
            }

            _resolved = true;
            SetCollidersEnabled(false);
            return true;
        }

        private void SetCollidersEnabled(bool value)
        {
            if (choiceColliders == null)
            {
                return;
            }

            foreach (var choiceCollider in choiceColliders)
            {
                if (choiceCollider != null)
                {
                    choiceCollider.enabled = value;
                }
            }
        }
    }
}
