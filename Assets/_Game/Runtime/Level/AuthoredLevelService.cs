using System;

namespace RunRich.Runtime.Level
{
    /// <summary>
    /// Switches between level roots that already exist in Gameplay_Clean.unity.
    /// No level geometry is instantiated at runtime.
    /// </summary>
    public sealed class AuthoredLevelService : ILevelService
    {
        private readonly LevelBindings[] _levels;
        private int _currentIndex;

        public AuthoredLevelService(LevelBindings[] levels)
        {
            if (levels == null || levels.Length == 0)
                throw new ArgumentException("At least one authored level is required.", nameof(levels));

            for (var i = 0; i < levels.Length; i++)
            {
                if (levels[i] == null)
                    throw new ArgumentException($"Authored level at index {i} is null.", nameof(levels));
            }

            _levels = levels;
        }

        public LevelBindings CurrentLevel => _levels[_currentIndex];

        public void Initialize()
        {
            _currentIndex = 0;
            for (var i = 0; i < _levels.Length; i++)
                _levels[i].gameObject.SetActive(i == _currentIndex);

            CurrentLevel.PrepareForRun();
        }

        public void Restart()
        {
            var current = CurrentLevel.gameObject;
            current.SetActive(false);
            current.SetActive(true);
            CurrentLevel.PrepareForRun();
        }

        public void LoadNext()
        {
            CurrentLevel.gameObject.SetActive(false);
            _currentIndex = (_currentIndex + 1) % _levels.Length;
            CurrentLevel.gameObject.SetActive(true);
            CurrentLevel.PrepareForRun();
        }
    }
}
