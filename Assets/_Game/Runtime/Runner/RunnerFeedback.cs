using RunRich.Runtime.Flow;
using RunRich.Runtime.Wealth;
using TMPro;
using UnityEngine;

namespace RunRich.Runtime.Runner
{
    /// <summary>
    /// Presentation-only feedback for the runner. Gameplay values are owned elsewhere.
    /// The audio mapping in this class was matched against the supplied reference video,
    /// while the visual sprites come from the extracted ReferenceAssets package.
    /// </summary>
    public sealed class RunnerFeedback : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioClip positiveClip;
        [SerializeField] private AudioClip negativeClip;
        [SerializeField] private AudioClip choicePositiveClip;
        [SerializeField] private AudioClip checkpointClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip multiplierLowClip;
        [SerializeField] private AudioClip multiplierMidClip;
        [SerializeField] private AudioClip multiplierHighClip;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private AudioClip[] heelClips;

        [Header("World feedback")]
        [SerializeField] private Transform billboardRoot;
        [SerializeField] private SpriteRenderer positiveAura;
        [SerializeField] private SpriteRenderer negativeStain;
        [SerializeField] private SpriteRenderer sparkle;
        [SerializeField] private SpriteRenderer[] moneyBursts;
        [SerializeField] private SpriteRenderer[] negativeShards;
        [SerializeField] private SpriteRenderer[] negativeStars;
        [SerializeField] private TMP_Text deltaText;
        [SerializeField] private TMP_Text stateText;

        [Header("Tuning")]
        [SerializeField, Min(0.1f)] private float normalFxDuration = 0.56f;
        [SerializeField, Min(0.1f)] private float strongFxDuration = 0.95f;
        [SerializeField, Min(0.1f)] private float stateFxDuration = 1.05f;
        [SerializeField, Min(0.15f)] private float footstepInterval = 0.43f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.13f;

        private GameFlow _flow;
        private RichPoorController _wealth;
        private WealthState _lastState;
        private bool _hasLastState;
        private float _footstepTimer;
        private int _footstepIndex;

        private VisualFxKind _fxKind;
        private float _fxTime;
        private float _fxDuration;
        private bool _strongFx;
        private float _stateFxTime = 99f;
        private int _displayedDelta;
        private float _lastDeltaFxAt = -10f;
        private float _nextPositiveSfxAt;

        private enum VisualFxKind
        {
            None,
            Positive,
            Negative,
            Checkpoint
        }

        public void Bind(GameFlow flow, RichPoorController wealth)
        {
            Unbind();
            _flow = flow;
            _wealth = wealth;

            if (_flow != null)
                _flow.StateChanged += HandleFlowChanged;

            if (_wealth != null)
            {
                _wealth.Changed += HandleWealthChanged;
                _lastState = _wealth.State;
                _hasLastState = true;
            }

            _footstepTimer = 0.15f;
            ResetVisuals();
        }

        public void PlayWealthDelta(int delta)
        {
            if (delta >= 0)
            {
                if (Time.unscaledTime >= _nextPositiveSfxAt)
                {
                    PlayClip(audioSource, positiveClip, sfxVolume);
                    _nextPositiveSfxAt = Time.unscaledTime + 0.085f;
                }
                // Reference presentation displays +1 $ for each normal money pickup even though
                // the confirmed gameplay wealth delta is +2. Consecutive pickups are still merged
                // by StartWealthFx, so the on-screen burst naturally becomes +2/+3/.../+6 $.
                // Choice gates keep their actual +/-20 presentation through PlayChoice().
                StartWealthFx(delta > 0 ? 1 : 0, VisualFxKind.Positive, false);
            }
            else
            {
                PlayClip(audioSource, negativeClip, sfxVolume);
                StartWealthFx(delta, VisualFxKind.Negative, false);
            }
        }

        public void PlayChoice(int delta)
        {
            if (delta >= 0)
            {
                // Exact reference-video match at the School choice/status transition.
                PlayClip(audioSource, choicePositiveClip != null ? choicePositiveClip : positiveClip, sfxVolume);
                StartWealthFx(delta, VisualFxKind.Positive, true);
            }
            else
            {
                PlayClip(audioSource, negativeClip, sfxVolume);
                StartWealthFx(delta, VisualFxKind.Negative, true);
            }
        }

        public void PlayCheckpoint()
        {
            PlayClip(audioSource, checkpointClip, 1f);
        }

        public void PlayFinish(int multiplier, bool completed)
        {
            if (completed)
            {
                PlayClip(audioSource, winClip, 1f);
                return;
            }

            // The three sequential clips below were waveform-matched in the reference video.
            AudioClip clip = null;
            if (multiplier == 2) clip = multiplierLowClip;
            else if (multiplier == 3) clip = multiplierMidClip;
            else if (multiplier >= 4) clip = multiplierHighClip;

            PlayClip(audioSource, clip, sfxVolume);
        }

        private void Update()
        {
            UpdateBillboard();
            UpdateFootsteps(Time.deltaTime);
            UpdateWorldFx(Time.deltaTime);
            UpdateStateFx(Time.deltaTime);
        }

        private void HandleFlowChanged(GameFlowState state)
        {
            if (state == GameFlowState.Playing)
                _footstepTimer = 0.34f;
            else
                _footstepTimer = footstepInterval;
        }

        private void HandleWealthChanged(int value, WealthState state)
        {
            if (!_hasLastState)
            {
                _lastState = state;
                _hasLastState = true;
                return;
            }

            if (state == _lastState)
                return;

            _lastState = state;
            StartStateFx(state);
        }

        private void UpdateFootsteps(float deltaTime)
        {
            if (_flow == null || _flow.State != GameFlowState.Playing)
                return;

            _footstepTimer -= Mathf.Max(0f, deltaTime);
            if (_footstepTimer > 0f)
                return;

            _footstepTimer += footstepInterval;

            var useHeels = _wealth != null && _wealth.State >= WealthState.Decent;
            var clips = useHeels ? heelClips : footstepClips;
            if (clips == null || clips.Length == 0)
                return;

            var clip = clips[_footstepIndex % clips.Length];
            _footstepIndex++;
            PlayClip(footstepSource != null ? footstepSource : audioSource, clip, footstepVolume);
        }

        private void StartWealthFx(int delta, VisualFxKind kind, bool strong)
        {
            var now = Time.unscaledTime;
            var sameBurst = _fxKind == kind && now - _lastDeltaFxAt <= 0.18f;
            _displayedDelta = sameBurst ? _displayedDelta + delta : delta;
            _lastDeltaFxAt = now;

            _fxKind = kind;
            _strongFx = strong || _strongFx && sameBurst;
            _fxTime = 0f;
            _fxDuration = _strongFx ? strongFxDuration : normalFxDuration;

            if (deltaText != null)
            {
                deltaText.gameObject.SetActive(true);
                deltaText.text = $"{(_displayedDelta >= 0 ? "+" : string.Empty)}{_displayedDelta} $";
                deltaText.color = delta >= 0
                    ? new Color(0.16f, 1f, 0.22f, 1f)
                    : new Color(1f, 0.12f, 0.12f, 1f);
                deltaText.transform.localPosition = new Vector3(0f, 0.28f, -0.02f);
                deltaText.transform.localScale = Vector3.one * (strong ? 0.12f : 0.095f);
            }
        }

        private void StartStateFx(WealthState state)
        {
            if (stateText == null)
                return;

            stateText.gameObject.SetActive(true);
            stateText.text = StateLabel(state);
            stateText.color = StateColor(state);
            stateText.transform.localPosition = new Vector3(0f, 0.92f, -0.03f);
            stateText.transform.localScale = Vector3.one * 0.13f;
            _stateFxTime = 0f;
        }

        private void UpdateWorldFx(float deltaTime)
        {
            if (_fxKind == VisualFxKind.None)
                return;

            _fxTime += Mathf.Max(0f, deltaTime);
            var t = Mathf.Clamp01(_fxTime / Mathf.Max(0.001f, _fxDuration));
            var bump = Mathf.Sin(t * Mathf.PI);
            var fade = 1f - t;

            var positive = _fxKind == VisualFxKind.Positive || _fxKind == VisualFxKind.Checkpoint;
            SetSpriteVisible(positiveAura, positive, positive
                ? (_fxKind == VisualFxKind.Checkpoint
                    ? new Color(1f, 0.86f, 0.18f, fade * 0.78f)
                    : new Color(0.18f, 1f, 0.28f, fade * 0.82f))
                : Color.clear);

            if (positiveAura != null && positive)
            {
                var maxScale = _strongFx ? 2.2f : 1.55f;
                positiveAura.transform.localScale = Vector3.one * Mathf.Lerp(0.38f, maxScale, bump);
                positiveAura.transform.localRotation = Quaternion.Euler(0f, 0f, t * 95f);
            }

            var negative = _fxKind == VisualFxKind.Negative;
            SetSpriteVisible(negativeStain, negative, new Color(1f, 0.12f, 0.08f, fade * 0.95f));
            if (negativeStain != null && negative)
            {
                negativeStain.transform.localScale = Vector3.one * Mathf.Lerp(0.45f, _strongFx ? 1.6f : 1.2f, bump);
                negativeStain.transform.localRotation = Quaternion.Euler(0f, 0f, -15f + t * 28f);
            }

            SetSpriteVisible(sparkle, positive, new Color(1f, 1f, 1f, bump * fade));
            if (sparkle != null && positive)
            {
                sparkle.transform.localScale = Vector3.one * Mathf.Lerp(0.18f, _strongFx ? 1.05f : 0.72f, bump);
                sparkle.transform.localRotation = Quaternion.Euler(0f, 0f, -t * 160f);
            }

            if (moneyBursts != null)
            {
                for (var i = 0; i < moneyBursts.Length; i++)
                {
                    var sprite = moneyBursts[i];
                    if (sprite == null) continue;

                    var show = _fxKind == VisualFxKind.Positive;
                    sprite.gameObject.SetActive(show);
                    if (!show) continue;

                    var angle = (i / Mathf.Max(1f, moneyBursts.Length)) * Mathf.PI * 2f + 0.4f;
                    var radius = bump * (_strongFx ? 1.12f : 0.72f);
                    sprite.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        0.12f + Mathf.Sin(angle) * radius * 0.55f + t * 0.32f,
                        -0.015f);
                    sprite.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + t * 210f);
                    sprite.transform.localScale = Vector3.one * Mathf.Lerp(0.18f, 0.11f, t);
                    sprite.color = new Color(1f, 1f, 1f, fade);
                }
            }

            if (negativeShards != null)
            {
                for (var i = 0; i < negativeShards.Length; i++)
                {
                    var sprite = negativeShards[i];
                    if (sprite == null) continue;
                    var show = _fxKind == VisualFxKind.Negative;
                    sprite.gameObject.SetActive(show);
                    if (!show) continue;

                    var angle = (i / Mathf.Max(1f, negativeShards.Length)) * Mathf.PI * 2f + 0.23f;
                    var radius = Mathf.Lerp(0.08f, _strongFx ? 1.08f : 0.72f, t);
                    sprite.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        0.15f + Mathf.Sin(angle) * radius * 0.52f - t * t * 0.18f,
                        -0.016f);
                    sprite.transform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + t * 210f);
                    sprite.transform.localScale = Vector3.one * Mathf.Lerp(0.075f, 0.035f, t);
                    sprite.color = new Color(1f, 1f, 1f, fade);
                }
            }

            if (negativeStars != null)
            {
                for (var i = 0; i < negativeStars.Length; i++)
                {
                    var sprite = negativeStars[i];
                    if (sprite == null) continue;
                    var show = _fxKind == VisualFxKind.Negative;
                    sprite.gameObject.SetActive(show);
                    if (!show) continue;

                    var angle = (i / Mathf.Max(1f, negativeStars.Length)) * Mathf.PI * 2f - 0.35f;
                    var radius = bump * (_strongFx ? 0.92f : 0.62f);
                    sprite.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        0.22f + Mathf.Sin(angle) * radius * 0.55f + t * 0.12f,
                        -0.019f);
                    sprite.transform.localRotation = Quaternion.Euler(0f, 0f, -t * 130f + i * 22f);
                    sprite.transform.localScale = Vector3.one * Mathf.Lerp(0.11f, 0.055f, t);
                    sprite.color = new Color(1f, 0.06f, 0.04f, fade * 0.9f);
                }
            }

            if (deltaText != null && deltaText.gameObject.activeSelf)
            {
                var c = deltaText.color;
                c.a = fade;
                deltaText.color = c;
                deltaText.transform.localPosition = new Vector3(0f, 0.28f + t * 0.72f, -0.02f);
                var pop = 0.85f + bump * 0.34f;
                var baseScale = _strongFx ? 0.12f : 0.095f;
                deltaText.transform.localScale = Vector3.one * baseScale * pop;
            }

            if (t >= 1f)
            {
                _fxKind = VisualFxKind.None;
                HideFxSprites();
                if (deltaText != null) deltaText.gameObject.SetActive(false);
            }
        }

        private void UpdateStateFx(float deltaTime)
        {
            if (stateText == null || !stateText.gameObject.activeSelf)
                return;

            _stateFxTime += Mathf.Max(0f, deltaTime);
            var t = Mathf.Clamp01(_stateFxTime / Mathf.Max(0.001f, stateFxDuration));
            var bump = Mathf.Sin(t * Mathf.PI);
            var c = stateText.color;
            c.a = Mathf.Clamp01((1f - t) * 1.25f);
            stateText.color = c;
            stateText.transform.localPosition = new Vector3(0f, 0.92f + t * 0.42f, -0.03f);
            stateText.transform.localScale = Vector3.one * (0.13f * (0.9f + bump * 0.34f));

            if (t >= 1f)
                stateText.gameObject.SetActive(false);
        }

        private void UpdateBillboard()
        {
            if (billboardRoot == null)
                return;

            var camera = UnityEngine.Camera.main;
            if (camera != null)
                billboardRoot.rotation = camera.transform.rotation;
        }

        private void ResetVisuals()
        {
            _fxKind = VisualFxKind.None;
            _fxTime = 0f;
            _stateFxTime = 99f;
            _displayedDelta = 0;
            _lastDeltaFxAt = -10f;
            _nextPositiveSfxAt = 0f;
            HideFxSprites();
            if (deltaText != null) deltaText.gameObject.SetActive(false);
            if (stateText != null) stateText.gameObject.SetActive(false);
        }

        private void HideFxSprites()
        {
            if (positiveAura != null) positiveAura.gameObject.SetActive(false);
            if (negativeStain != null) negativeStain.gameObject.SetActive(false);
            if (sparkle != null) sparkle.gameObject.SetActive(false);
            if (negativeShards != null)
                foreach (var sprite in negativeShards)
                    if (sprite != null) sprite.gameObject.SetActive(false);

            if (negativeStars != null)
                foreach (var sprite in negativeStars)
                    if (sprite != null) sprite.gameObject.SetActive(false);

            if (moneyBursts == null) return;
            foreach (var sprite in moneyBursts)
                if (sprite != null) sprite.gameObject.SetActive(false);
        }

        private static void SetSpriteVisible(SpriteRenderer sprite, bool visible, Color color)
        {
            if (sprite == null) return;
            sprite.gameObject.SetActive(visible);
            if (visible) sprite.color = color;
        }

        private static string StateLabel(WealthState state)
        {
            return state switch
            {
                WealthState.Hobo => "БОМЖ",
                WealthState.Poor => "БЕДНЫЙ",
                WealthState.Decent => "СОСТОЯТЕЛЬНЫЙ",
                WealthState.Rich => "БОГАТЫЙ",
                WealthState.Millionaire => "МИЛЛИОНЕР",
                _ => string.Empty
            };
        }

        private static Color StateColor(WealthState state)
        {
            return state switch
            {
                WealthState.Hobo => new Color(1f, 0f, 0f, 1f),
                WealthState.Poor => new Color(1f, 0.423569858f, 0f, 1f),
                WealthState.Decent => new Color(0.94339621f, 0.827859402f, 0.15574935f, 1f),
                WealthState.Rich => new Color(0.39854759f, 0.745098054f, 0.05882353f, 1f),
                WealthState.Millionaire => new Color(0.235294104f, 0.839215696f, 0.723115802f, 1f),
                _ => Color.white
            };
        }

        private static void PlayClip(AudioSource source, AudioClip clip, float volume)
        {
            if (source != null && clip != null)
                source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private void Unbind()
        {
            if (_flow != null)
            {
                _flow.StateChanged -= HandleFlowChanged;
                _flow = null;
            }

            if (_wealth != null)
            {
                _wealth.Changed -= HandleWealthChanged;
                _wealth = null;
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
