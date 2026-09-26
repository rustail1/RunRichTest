using System;
using ButchersGames;

namespace RunRich.Runtime.Level
{
    /// <summary>
    /// Legacy adapter kept for compatibility with the provided BG_LevelManager package.
    /// Gameplay_Clean uses AuthoredLevelService instead so scene-authored levels are not instantiated at runtime.
    /// </summary>
    public sealed class ButchersLevelService : ILevelService
    {
        private readonly LevelManager _levelManager;

        public ButchersLevelService(LevelManager levelManager)
        {
            _levelManager = levelManager != null
                ? levelManager
                : throw new ArgumentNullException(nameof(levelManager));
        }

        public LevelBindings CurrentLevel => _levelManager.GetComponentInChildren<LevelBindings>(true);

        public void Initialize()
        {
            if (_levelManager.Levels == null || _levelManager.Levels.Count == 0)
                throw new InvalidOperationException("LevelManager requires at least one configured level.");

            _levelManager.SelectLevel(0, false);
            _levelManager.StartLevel();
        }

        public void Restart()
        {
            _levelManager.RestartLevel();
        }

        public void LoadNext()
        {
            _levelManager.NextLevel();
        }
    }
}
