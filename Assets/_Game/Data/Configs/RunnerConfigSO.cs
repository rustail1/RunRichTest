using UnityEngine;

namespace RunRich.Data.Configs
{
    [CreateAssetMenu(fileName = "RunnerConfig", menuName = "Run Rich/Configs/Runner")]
    public sealed class RunnerConfigSO : ScriptableObject
    {
        [SerializeField, Min(0f)] private float forwardSpeed = 5f;
        [SerializeField, Min(0f)] private float swerveSensitivity = 5f;
        [SerializeField, Min(0f)] private float swerveSmoothTime = 0.08f;
        [SerializeField, Min(0f)] private float maxLateralSpeed = 8f;
        [SerializeField, Min(0f)] private float roadHalfWidth = 2.5f;
        [SerializeField, Min(0f)] private float startDelay;

        public float ForwardSpeed => forwardSpeed;
        public float SwerveSensitivity => swerveSensitivity;
        public float SwerveSmoothTime => swerveSmoothTime;
        public float MaxLateralSpeed => maxLateralSpeed;
        public float RoadHalfWidth => roadHalfWidth;
        public float StartDelay => startDelay;
    }
}
