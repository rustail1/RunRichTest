using UnityEngine;

namespace RunRich.Data.Configs
{
    [CreateAssetMenu(fileName = "RunnerConfig", menuName = "Run Rich/Configs/Runner")]
    public sealed class RunnerConfigSO : ScriptableObject
    {
        [Header("Forward")]
        [SerializeField, Min(0f)] private float forwardSpeed = 8f;
        [SerializeField] private bool slowSpeedWhenPoor = false;
        [SerializeField, Min(0f)] private float startDelay;

        [Header("Swerve")]
        [SerializeField, Min(0f)] private float swerveSensitivity = 20f;
        [SerializeField, Min(0f)] private float roadHalfWidth = 2.5f;
        [SerializeField, Min(0f)] private float lateralMaxSpeed = 80f;
        [SerializeField, Min(0.001f)] private float lateralSmoothTime = 0.1f;

        [Header("Swerve Wall Bounds")]
        [Tooltip("Optional wall-only layer mask for the original CheckCharacterRaycast-style bounds. Leave Nothing when the authored track has no wall colliders on a dedicated layer.")]
        [SerializeField] private LayerMask wallLayerMask;
        [SerializeField, Min(0f)] private float wallRaycastDistance = 100f;
        [SerializeField, Min(0f)] private float wallRaycastPadding = 0.3f;

        [Header("Character Turn")]
        [SerializeField, Range(0f, 60f)] private float maxTurnAngle = 35f;
        [SerializeField, Min(0f)] private float visualRotationSpeed = 10f;

        [Header("Debug")]
        [SerializeField] private bool showSwerveDebug;

        [Header("Path - Advanced")]
        [SerializeField, Min(0f)] private float pathRotationSmoothTime = 0.07f;
        [SerializeField, Range(4, 64)] private int curveSamplesPerSegment = 40;

        // Phase 1 migration lets the scripts-only drop-in upgrade an older RunnerConfig.asset
        // that still contains the temporary 6.2 forward speed. It runs once per asset.
        [SerializeField, HideInInspector] private int configDataVersion;
        private const int CurrentConfigDataVersion = 2;

        public float ForwardSpeed => forwardSpeed;
        public bool SlowSpeedWhenPoor => slowSpeedWhenPoor;
        public float StartDelay => startDelay;
        public float SwerveSensitivity => swerveSensitivity;
        public float RoadHalfWidth => roadHalfWidth;
        public float LateralMaxSpeed => lateralMaxSpeed;
        public float LateralSmoothTime => lateralSmoothTime;
        public LayerMask WallLayerMask => wallLayerMask;
        public float WallRaycastDistance => wallRaycastDistance;
        public float WallRaycastPadding => wallRaycastPadding;
        public float MaxTurnAngle => maxTurnAngle;
        public float VisualRotationSpeed => visualRotationSpeed;
        public bool ShowSwerveDebug => showSwerveDebug;
        public float PathRotationSmoothTime => pathRotationSmoothTime;
        public int CurveSamplesPerSegment => curveSamplesPerSegment;

        private void OnEnable()
        {
            if (configDataVersion >= CurrentConfigDataVersion)
                return;

            // XAPK-confirmed Phase 1 defaults. The old clean-scene build used 6.2 as a
            // compensation value; after this migration the base running speed is 8 again.
            forwardSpeed = 8f;
            // The XAPK branch exists, but the serialized Level01 flag was not confirmed.
            // Keep the confirmed base RunningSpeed=8 without stacking an unverified wealth multiplier.
            slowSpeedWhenPoor = false;
            wallRaycastDistance = 100f;
            wallRaycastPadding = 0.3f;
            configDataVersion = CurrentConfigDataVersion;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += PersistMigratedAsset;
#endif
        }

#if UNITY_EDITOR
        private void PersistMigratedAsset()
        {
            if (this == null)
                return;

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
#endif
    }
}
