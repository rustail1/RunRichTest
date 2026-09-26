using System;
using RunRich.Runtime.Bootstrap;
using UnityEngine;

namespace RunRich.Runtime.Wealth
{
    public enum WealthState
    {
        Hobo = 0,
        Poor = 1,
        Decent = 2,
        Rich = 3,
        Millionaire = 4
    }

    /// <summary>Single runtime authority for the XAPK Rich/Poor value and status thresholds.</summary>
    public sealed class RichPoorController : ITickable
    {
        private readonly int _minimum;
        private readonly int _maximum;
        private readonly int _poorThreshold;
        private readonly int _decentThreshold;
        private readonly int _richThreshold;
        private readonly int _millionaireThreshold;
        private readonly float _changeSpeed;
        private readonly bool _gameOverAtZero;
        private readonly bool _waitOneMoreErrorAtZero;
        private float _currentValue;
        private bool _waitingForOneMoreError;

        public RichPoorController(int initial, int minimum, int maximum, int poorThreshold,
            int decentThreshold, int richThreshold, int millionaireThreshold, float changeSpeed = 140f)
            : this(initial, minimum, maximum, poorThreshold, decentThreshold, richThreshold,
                millionaireThreshold, changeSpeed, true, true)
        {
        }

        public RichPoorController(int initial, int minimum, int maximum, int poorThreshold,
            int decentThreshold, int richThreshold, int millionaireThreshold, float changeSpeed,
            bool gameOverAtZero, bool waitOneMoreErrorAtZero)
        {
            _minimum = minimum;
            _maximum = Mathf.Max(minimum, maximum);
            _poorThreshold = poorThreshold;
            _decentThreshold = Mathf.Max(poorThreshold, decentThreshold);
            _richThreshold = Mathf.Max(_decentThreshold, richThreshold);
            _millionaireThreshold = Mathf.Max(_richThreshold, millionaireThreshold);
            _changeSpeed = Mathf.Max(0f, changeSpeed);
            _gameOverAtZero = gameOverAtZero;
            _waitOneMoreErrorAtZero = waitOneMoreErrorAtZero;
            Value = Mathf.Clamp(initial, _minimum, _maximum);
            TargetValue = Value;
            _currentValue = Value;
            State = EvaluateState(Value);
        }

        public event Action<int, WealthState> Changed;
        public event Action Depleted;

        public int Value { get; private set; }
        public int TargetValue { get; private set; }
        public int Minimum => _minimum;
        public int Maximum => _maximum;
        public WealthState State { get; private set; }
        public float Normalized => _maximum > _minimum
            ? Mathf.InverseLerp(_minimum, _maximum, _currentValue)
            : 0f;
        public float SpeedRatio => 1f + Mathf.Max(0f, Value) / Mathf.Max(1f, _maximum);

        public void Add(int delta)
        {
            if (delta < 0 && TargetValue <= _minimum && _waitingForOneMoreError)
            {
                Depleted?.Invoke();
                return;
            }
            TargetValue = Mathf.Clamp(TargetValue + delta, _minimum, _maximum);
        }

        public void Force(int value)
        {
            var clamped = Mathf.Clamp(value, _minimum, _maximum);
            TargetValue = clamped;
            _currentValue = clamped;
            _waitingForOneMoreError = clamped <= _minimum && _waitOneMoreErrorAtZero;
            PublishValue(clamped);
        }

        public void Tick(float deltaTime)
        {
            if (Mathf.Approximately(_currentValue, TargetValue))
                return;

            _currentValue = Mathf.MoveTowards(_currentValue, TargetValue, _changeSpeed * Mathf.Max(0f, deltaTime));
            PublishValue(Mathf.RoundToInt(_currentValue));
        }

        private void PublishValue(int value)
        {
            value = Mathf.Clamp(value, _minimum, _maximum);
            var previousValue = Value;
            var previousState = State;
            Value = value;
            State = EvaluateState(Value);

            if (Value != previousValue || State != previousState)
                Changed?.Invoke(Value, State);

            if (previousValue > _minimum && Value <= _minimum && _gameOverAtZero)
            {
                if (_waitOneMoreErrorAtZero)
                    _waitingForOneMoreError = true;
                else
                    Depleted?.Invoke();
            }
        }

        public WealthState EvaluateState(int value)
        {
            if (value >= _millionaireThreshold) return WealthState.Millionaire;
            if (value >= _richThreshold) return WealthState.Rich;
            if (value >= _decentThreshold) return WealthState.Decent;
            if (value >= _poorThreshold) return WealthState.Poor;
            return WealthState.Hobo;
        }
    }
}
