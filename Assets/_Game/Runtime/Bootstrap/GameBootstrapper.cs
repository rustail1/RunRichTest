using ButchersGames;
using RunRich.Runtime.Flow;
using RunRich.Runtime.Level;
using UnityEngine;

namespace RunRich.Runtime.Bootstrap
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;

        private GameLoop _gameLoop;
        private GameFlow _gameFlow;

        private void Awake()
        {
            if (levelManager == null)
            {
                Debug.LogError("GameBootstrapper requires a LevelManager reference.", this);
                enabled = false;
                return;
            }

            _gameLoop = new GameLoop();

            ILevelService levelService = new ButchersLevelService(levelManager);
            _gameFlow = new GameFlow(levelService);
        }

        private void Update()
        {
            _gameLoop?.Tick(Time.deltaTime);
        }
    }
}
