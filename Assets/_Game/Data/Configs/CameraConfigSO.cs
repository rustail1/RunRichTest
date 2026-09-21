using UnityEngine;

namespace RunRich.Data.Configs
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Run Rich/Configs/Camera")]
    public sealed class CameraConfigSO : ScriptableObject
    {
        [SerializeField, Range(1f, 179f)] private float fieldOfView = 60f;
        [SerializeField] private Vector3 offset = new(0f, 2.9f, -6.5f);
        [SerializeField] private float pitch = 16.5f;
        [SerializeField, Min(0f)] private float followSmoothTime = 0.08f;
        [SerializeField, Min(0f)] private float lookAhead;
        [SerializeField, Range(0f, 1f)] private float targetScreenX = 0.5f;
        [SerializeField, Range(0f, 1f)] private float targetScreenY = 0.5f;

        public float FieldOfView => fieldOfView;
        public Vector3 Offset => offset;
        public float Pitch => pitch;
        public float FollowSmoothTime => followSmoothTime;
        public float LookAhead => lookAhead;
        public float TargetScreenX => targetScreenX;
        public float TargetScreenY => targetScreenY;
    }
}
