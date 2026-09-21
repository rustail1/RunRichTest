using System;
using ButchersGames;

namespace RunRich.Runtime.Level
{
    public sealed class ButchersLevelService : ILevelService
    {
        private readonly LevelManager _levelManager;

        public ButchersLevelService(LevelManager levelManager)
        {
            _levelManager = levelManager != null
                ? levelManager
                : throw new ArgumentNullException(nameof(levelManager));
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
