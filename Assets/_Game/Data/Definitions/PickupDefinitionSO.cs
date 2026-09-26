using UnityEngine;

namespace RunRich.Data.Definitions
{
    [CreateAssetMenu(fileName = "PickupDefinition", menuName = "Run Rich/Definitions/Pickup")]
    public sealed class PickupDefinitionSO : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int wealthDelta = 1;

        public GameObject Prefab => prefab;
        public int WealthDelta => wealthDelta;
    }
}
