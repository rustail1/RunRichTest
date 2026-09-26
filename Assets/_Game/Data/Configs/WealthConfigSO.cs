using UnityEngine;

namespace RunRich.Data.Configs
{
    [CreateAssetMenu(fileName = "WealthConfig", menuName = "Run Rich/Configs/Wealth")]
    public sealed class WealthConfigSO : ScriptableObject
    {
        [SerializeField] private int initialWealth = 40;
        [SerializeField] private int minimumWealth;
        [SerializeField] private int maximumWealth = 140;
        [SerializeField, Min(0f)] private float changeSpeed = 140f;
        [SerializeField] private bool gameOverAtZero = true;
        [SerializeField] private bool waitOneMoreErrorAtZero = true;
        [SerializeField] private int hoboThreshold;
        [SerializeField] private int poorThreshold = 31;
        [SerializeField] private int decentThreshold = 61;
        [SerializeField] private int richThreshold = 101;
        [SerializeField] private int millionaireThreshold = 139;

        public int InitialWealth => initialWealth;
        public int MinimumWealth => minimumWealth;
        public int MaximumWealth => maximumWealth;
        public int HoboThreshold => hoboThreshold;
        public int PoorThreshold => poorThreshold;
        public int DecentThreshold => decentThreshold;
        public int RichThreshold => richThreshold;
        public int MillionaireThreshold => millionaireThreshold;
        public float ChangeSpeed => changeSpeed;
        public bool GameOverAtZero => gameOverAtZero;
        public bool WaitOneMoreErrorAtZero => waitOneMoreErrorAtZero;
    }
}
