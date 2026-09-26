using RunRich.Runtime.Wealth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunRich.Runtime.UI
{
    /// <summary>
    /// Existing screen-space status UI. No GameObjects are created at runtime.
    /// </summary>
    public sealed class RunnerStatusView : MonoBehaviour
    {
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private Image gaugeFill;

        private RichPoorController _wealth;

        public RectTransform RectTransform => transform as RectTransform;

        public void Bind(RichPoorController wealth)
        {
            Unbind();
            _wealth = wealth;
            if (_wealth == null) return;

            _wealth.Changed += HandleChanged;
            HandleChanged(_wealth.Value, _wealth.State);
        }

        private void OnDestroy() => Unbind();

        private void HandleChanged(int value, WealthState state)
        {
            var color = state switch
            {
                WealthState.Hobo => new Color(1f, 0f, 0f, 1f),
                WealthState.Poor => new Color(1f, 0.423569858f, 0f, 1f),
                WealthState.Decent => new Color(0.94339621f, 0.827859402f, 0.15574935f, 1f),
                WealthState.Rich => new Color(0.39854759f, 0.745098054f, 0.05882353f, 1f),
                WealthState.Millionaire => new Color(0.235294104f, 0.839215696f, 0.723115802f, 1f),
                _ => Color.white
            };

            if (stateText != null)
            {
                stateText.text = state switch
                {
                    WealthState.Hobo => "БОМЖ",
                    WealthState.Poor => "БЕДНЫЙ",
                    WealthState.Decent => "СОСТОЯТЕЛЬНЫЙ",
                    WealthState.Rich => "БОГАТЫЙ",
                    WealthState.Millionaire => "МИЛЛИОНЕР",
                    _ => state.ToString().ToUpperInvariant()
                };
                stateText.color = color;
            }

            if (gaugeFill != null)
            {
                gaugeFill.color = color;
                gaugeFill.fillAmount = _wealth != null
                    ? Mathf.InverseLerp(_wealth.Minimum, _wealth.Maximum, value)
                    : 0f;
            }
        }

        private void Unbind()
        {
            if (_wealth == null) return;
            _wealth.Changed -= HandleChanged;
            _wealth = null;
        }
    }
}
