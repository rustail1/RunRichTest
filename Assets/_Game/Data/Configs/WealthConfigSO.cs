using UnityEngine;

namespace RunRich.Data.Configs
{
    [CreateAssetMenu(fileName = "WealthConfig", menuName = "Run Rich/Configs/Wealth")]
    public sealed class WealthConfigSO : ScriptableObject
    {
        [SerializeField] private int initialWealth = 40;
        [SerializeField] private int minimumWealth;
        [SerializeField] private int maximumWealth = 120;
        [SerializeField] private int wellOffThreshold = 70;
        [SerializeField] private int richThreshold = 100;

        public int InitialWealth => initialWealth;
        public int MinimumWealth => minimumWealth;
        public int MaximumWealth => maximumWealth;
        public int WellOffThreshold => wellOffThreshold;
        public int RichThreshold => richThreshold;
    }
}
