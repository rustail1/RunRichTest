using UnityEngine;

namespace RunRich.Runtime.Wealth
{
    /// <summary>
    /// XAPK wealth-state visual mapping. The shared skeleton stays alive while exactly one of
    /// poor/casual/middle/bling/cocktail renderers is active.
    /// </summary>
    public sealed class PlayerAppearance : MonoBehaviour
    {
        [SerializeField] private GameObject hoboVisual;
        [SerializeField] private GameObject poorVisual;
        [SerializeField] private GameObject decentVisual;
        [SerializeField] private GameObject richVisual;
        [SerializeField] private GameObject millionaireVisual;
        [SerializeField] private Animator animator;

        private static readonly int RichHash = Animator.StringToHash("Rich");

        private RichPoorController _wealth;

        public void Bind(RichPoorController wealth)
        {
            Unbind();
            _wealth = wealth;
            _wealth.Changed += HandleChanged;
            Apply(_wealth.State);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void HandleChanged(int value, WealthState state)
        {
            Apply(state);
        }

        private void Apply(WealthState state)
        {
            SetActive(hoboVisual, state == WealthState.Hobo);
            SetActive(poorVisual, state == WealthState.Poor);
            SetActive(decentVisual, state == WealthState.Decent);
            SetActive(richVisual, state == WealthState.Rich);
            SetActive(millionaireVisual, state == WealthState.Millionaire);

            if (animator != null && animator.runtimeAnimatorController != null &&
                HasParameter(animator, RichHash, AnimatorControllerParameterType.Float))
                animator.SetFloat(RichHash, _wealth != null ? _wealth.Normalized : 0f);
        }

        private static bool HasParameter(Animator target, int nameHash, AnimatorControllerParameterType type)
        {
            if (target == null) return false;
            var parameters = target.parameters;
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].nameHash == nameHash && parameters[i].type == type)
                    return true;
            }
            return false;
        }

        private static void SetActive(GameObject target, bool value)
        {
            if (target != null) target.SetActive(value);
        }

        private void Unbind()
        {
            if (_wealth != null)
            {
                _wealth.Changed -= HandleChanged;
                _wealth = null;
            }
        }
    }
}
