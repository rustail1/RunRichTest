using UnityEngine;

namespace RunRich.Data.Configs
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Run Rich/Configs/Camera")]
    public sealed class CameraConfigSO : ScriptableObject
    {
        [SerializeField, Range(1f, 179f)] private float fieldOfView = 60f;
        [SerializeField, Min(0.01f)] private float nearClipPlane = 0.3f;
        [SerializeField, Min(1f)] private float farClipPlane = 500f;
        [SerializeField, Min(0f)] private float followSmoothTime = 0.01f;
        [SerializeField] private bool followTargetOnX = true;

        [Header("CamStart")]
        [SerializeField] private Vector3 startLocalPosition = new(0f, 2.65f, -4.82f);
        [SerializeField] private Vector3 startLocalEuler = new(17f, 0f, 0f);

        [Header("CamGame")]
        [SerializeField] private Vector3 gameLocalPosition = new(0f, 3.38f, -5.39f);
        [SerializeField] private Vector3 gameLocalEuler = new(21.33f, 0f, 0f);

        [Header("CamGameNew (debug alternative only)")]
        [SerializeField] private Vector3 gameNewLocalPosition = new(0f, 5.56f, -4.72f);
        [SerializeField] private Vector3 gameNewLocalEuler = new(31.655f, 0f, 0f);

        [Header("CamGameOver")]
        [SerializeField] private Vector3 gameOverLocalPosition = new(0f, 3.14f, -8f);
        [SerializeField] private Vector3 gameOverLocalEuler = new(25.548f, 0f, 0f);

        [Header("CamWin")]
        [SerializeField, Min(0.01f)] private float winOrbitDuration = 6f;

        public float FieldOfView => fieldOfView;
        public float NearClipPlane => nearClipPlane;
        public float FarClipPlane => farClipPlane;
        public float FollowSmoothTime => followSmoothTime;
        public bool FollowTargetOnX => followTargetOnX;
        public Vector3 StartLocalPosition => startLocalPosition;
        public Vector3 StartLocalEuler => startLocalEuler;
        public Vector3 GameLocalPosition => gameLocalPosition;
        public Vector3 GameLocalEuler => gameLocalEuler;
        public Vector3 GameNewLocalPosition => gameNewLocalPosition;
        public Vector3 GameNewLocalEuler => gameNewLocalEuler;
        public Vector3 GameOverLocalPosition => gameOverLocalPosition;
        public Vector3 GameOverLocalEuler => gameOverLocalEuler;
        public float WinOrbitDuration => winOrbitDuration;
    }
}
