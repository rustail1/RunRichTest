using System;
using RunRich.Runtime.Finish;
using RunRich.Runtime.Flow;
using RunRich.Runtime.Runner;
using RunRich.Runtime.Wealth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunRich.Runtime.UI
{
    /// <summary>Updates the authored HUD and lightweight result-screen presentation.</summary>
    public sealed class GameplayHud : MonoBehaviour
    {
        [SerializeField] private GameObject readyPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text wealthText;
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private TMP_Text winScoreText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private RunnerStatusView runnerStatus;
        [SerializeField] private GameObject readySettingsIcon;
        [SerializeField] private GameObject runExitIcon;

        [Header("UI audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip clickClip;

        [Header("Victory presentation")]
        [SerializeField] private RectTransform winRays;
        [SerializeField] private RectTransform[] winMoneyFx;
        [SerializeField] private RectTransform winPointer;
        [SerializeField] private TMP_Text adRewardLabel;

        private GameFlow _gameFlow;
        private RichPoorController _wealth;
        private FinishProgress _finishProgress;
        private RunnerMotor _motor;
        private int _levelNumber;
        private Action _restartAction;
        private Action _nextAction;
        private Vector2[] _winMoneyBasePositions;
        private float _winFxTime;
        private int _displayedBonusMultiplier = -1;

        public RectTransform CanvasRect
        {
            get
            {
                if (runnerStatus != null && runnerStatus.RectTransform != null)
                    return runnerStatus.RectTransform.parent as RectTransform;
                return transform as RectTransform;
            }
        }
        public RunnerStatusView RunnerStatus => runnerStatus;

        public void Bind(
            int levelNumber,
            GameFlow gameFlow,
            RichPoorController wealth,
            FinishProgress finishProgress,
            RunnerMotor motor,
            Action restartAction,
            Action nextAction)
        {
            Unbind();

            _gameFlow = gameFlow;
            _wealth = wealth;
            _finishProgress = finishProgress;
            _motor = motor;
            _levelNumber = levelNumber;
            _restartAction = restartAction;
            _nextAction = nextAction;

            _gameFlow.StateChanged += HandleFlowChanged;
            _wealth.Changed += HandleWealthChanged;
            _finishProgress.Changed += HandleMultiplierChanged;
            if (_motor != null) _motor.ProgressChanged += HandleProgressChanged;
            restartButton?.onClick.AddListener(HandleRestartClicked);
            nextButton?.onClick.AddListener(HandleNextClicked);
            runnerStatus?.Bind(_wealth);

            CacheWinFx();

            if (levelText != null) levelText.text = $"Уровень {levelNumber}";
            HandleWealthChanged(_wealth.Value, _wealth.State);
            HandleMultiplierChanged(_finishProgress.Multiplier);
            HandleProgressChanged(_motor != null ? _motor.Progress01 : 0f);
            HandleFlowChanged(_gameFlow.State);
        }

        private void Update()
        {
            if (winPanel == null || !winPanel.activeInHierarchy)
                return;

            _winFxTime += Time.unscaledDeltaTime;

            if (winRays != null)
                winRays.localRotation = Quaternion.Euler(0f, 0f, _winFxTime * 8f);

            if (winPointer != null)
            {
                var gaugePhase = Mathf.PingPong(_winFxTime * 1.35f, 3f);
                var gaugeIndex = Mathf.Clamp(Mathf.RoundToInt(gaugePhase), 0, 3);
                var bonusMultiplier = gaugeIndex + 2;
                SetBonusGaugeMultiplier(bonusMultiplier);

                var pulse = 1f + Mathf.Sin(_winFxTime * 4.5f) * 0.07f;
                winPointer.localScale = Vector3.one * pulse;
            }

            if (winMoneyFx == null || _winMoneyBasePositions == null)
                return;

            for (var i = 0; i < winMoneyFx.Length; i++)
            {
                var item = winMoneyFx[i];
                if (item == null || i >= _winMoneyBasePositions.Length) continue;

                var phase = i * 0.83f;
                var offset = new Vector2(
                    Mathf.Sin(_winFxTime * 1.15f + phase) * 8f,
                    Mathf.Sin(_winFxTime * 1.9f + phase) * 14f);
                item.anchoredPosition = _winMoneyBasePositions[i] + offset;
                var scale = 0.94f + Mathf.Sin(_winFxTime * 2.1f + phase) * 0.06f;
                item.localScale = Vector3.one * scale;
                item.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_winFxTime * 1.35f + phase) * 8f);
            }
        }

        private void OnDestroy() => Unbind();

        private void HandleFlowChanged(GameFlowState state)
        {
            SetActive(readyPanel, state == GameFlowState.Ready);
            SetActive(gameplayPanel, state == GameFlowState.Playing);
            SetActive(winPanel, state == GameFlowState.Won);
            SetActive(losePanel, state == GameFlowState.Lost);

            if (runnerStatus != null)
                runnerStatus.gameObject.SetActive(state == GameFlowState.Playing);

            SetActive(readySettingsIcon, state == GameFlowState.Ready);
            SetActive(runExitIcon, state != GameFlowState.Ready);

            if (levelText != null)
            {
                levelText.text = state == GameFlowState.Won
                    ? $"Уровень {_levelNumber}\nЗАВЕРШЕНО"
                    : $"Уровень {_levelNumber}";
                // Reference tutorial screen shows the 1..5 level strip, not the center level title.
                levelText.gameObject.SetActive(state == GameFlowState.Playing || state == GameFlowState.Won);
            }

            if (wealthText != null)
            {
                // Reference tutorial screen has no center wealth number; it appears once the run starts.
                wealthText.gameObject.SetActive(state == GameFlowState.Playing);
            }

            if (state == GameFlowState.Won)
            {
                _winFxTime = 0f;
                _displayedBonusMultiplier = -1;
                ResetWinFx();
                SetBonusGaugeMultiplier(4);
                if (winScoreText != null && _finishProgress != null)
                    winScoreText.text = $"x{_finishProgress.Multiplier}";
            }
        }

        private void HandleWealthChanged(int value, WealthState state)
        {
            if (wealthText != null) wealthText.text = value.ToString();
        }

        private void HandleMultiplierChanged(int multiplier)
        {
            if (multiplierText != null)
                multiplierText.text = multiplier > 1 ? $"x{multiplier}" : string.Empty;
        }

        private void SetBonusGaugeMultiplier(int multiplier)
        {
            multiplier = Mathf.Clamp(multiplier, 2, 5);
            if (_displayedBonusMultiplier == multiplier)
                return;

            _displayedBonusMultiplier = multiplier;

            if (winPointer != null)
            {
                winPointer.anchoredPosition = multiplier switch
                {
                    2 => new Vector2(-135f, 548f),
                    3 => new Vector2(-45f, 592f),
                    4 => new Vector2(48f, 592f),
                    _ => new Vector2(137f, 548f)
                };
            }

            if (adRewardLabel != null)
                adRewardLabel.text = $"ПОЛУЧИТЬ X{multiplier}\n{440 * multiplier}";
        }

        private void HandleProgressChanged(float progress)
        {
            if (progressFill != null)
                progressFill.fillAmount = Mathf.Clamp01(progress);
        }

        private void HandleRestartClicked()
        {
            PlayClick();
            _restartAction?.Invoke();
        }

        private void HandleNextClicked()
        {
            PlayClick();
            _nextAction?.Invoke();
        }

        private void PlayClick()
        {
            if (uiAudioSource != null && clickClip != null)
                uiAudioSource.PlayOneShot(clickClip, 1f);
        }

        private void CacheWinFx()
        {
            if (winMoneyFx == null)
            {
                _winMoneyBasePositions = null;
                return;
            }

            _winMoneyBasePositions = new Vector2[winMoneyFx.Length];
            for (var i = 0; i < winMoneyFx.Length; i++)
                if (winMoneyFx[i] != null)
                    _winMoneyBasePositions[i] = winMoneyFx[i].anchoredPosition;
        }

        private void ResetWinFx()
        {
            if (winRays != null)
                winRays.localRotation = Quaternion.identity;
            if (winPointer != null)
                winPointer.localScale = Vector3.one;

            if (winMoneyFx == null || _winMoneyBasePositions == null)
                return;

            for (var i = 0; i < winMoneyFx.Length; i++)
            {
                if (winMoneyFx[i] == null || i >= _winMoneyBasePositions.Length) continue;
                winMoneyFx[i].anchoredPosition = _winMoneyBasePositions[i];
                winMoneyFx[i].localScale = Vector3.one;
                winMoneyFx[i].localRotation = Quaternion.identity;
            }
        }

        private void Unbind()
        {
            if (_gameFlow != null) _gameFlow.StateChanged -= HandleFlowChanged;
            if (_wealth != null) _wealth.Changed -= HandleWealthChanged;
            if (_finishProgress != null) _finishProgress.Changed -= HandleMultiplierChanged;
            if (_motor != null) _motor.ProgressChanged -= HandleProgressChanged;

            restartButton?.onClick.RemoveListener(HandleRestartClicked);
            nextButton?.onClick.RemoveListener(HandleNextClicked);

            _gameFlow = null;
            _wealth = null;
            _finishProgress = null;
            _motor = null;
            _levelNumber = 0;
            _restartAction = null;
            _nextAction = null;
        }

        private static void SetActive(GameObject target, bool value)
        {
            if (target != null) target.SetActive(value);
        }
    }
}
