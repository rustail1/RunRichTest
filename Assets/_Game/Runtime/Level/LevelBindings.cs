using RunRich.Runtime.Pickups;
using RunRich.Runtime.Runner;
using RunRich.Runtime.Wealth;
using UnityEngine;

namespace RunRich.Runtime.Level
{
    public sealed class LevelBindings : MonoBehaviour
    {
        [SerializeField, Min(1)] private int levelNumber = 1;
        [SerializeField] private RunnerActor runner;
        [SerializeField] private PlayerAppearance appearance;
        [SerializeField] private RunnerAnimationView animationView;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform[] pathWaypoints;
        [SerializeField] private PickupStrip[] pickupStrips;

        public int LevelNumber => levelNumber;
        public RunnerActor Runner => runner;
        public PlayerAppearance Appearance => appearance;
        public RunnerAnimationView AnimationView => animationView;
        public Transform RunnerTransform => runner != null ? runner.transform : null;

        public Vector3[] GetPathPoints()
        {
            if (pathWaypoints == null) return System.Array.Empty<Vector3>();

            var points = new Vector3[pathWaypoints.Length];
            for (var index = 0; index < pathWaypoints.Length; index++)
                points[index] = pathWaypoints[index] != null ? pathWaypoints[index].position : Vector3.zero;

            return points;
        }

        public void PrepareForRun()
        {
            if (runner != null && playerSpawnPoint != null)
            {
                runner.transform.SetPositionAndRotation(playerSpawnPoint.position, playerSpawnPoint.rotation);

                // RunnerMotor captures the current steering-local rotation as its authored baseline.
                // Reset the builder-authored VisualCont to neutral before composing a retry/next run,
                // otherwise a fast Retry while the runner is still leaning can permanently offset yaw.
                var steering = runner.SteeringVisual;
                if (steering != null)
                    steering.localRotation = Quaternion.identity;
            }

            // Individual PickupTrigger objects disable themselves after collection.
            // Re-enable inactive pickups explicitly: toggling the level root alone does not
            // change a child's activeSelf flag, which caused collected pickups to stay gone
            // after Retry/Restart.
            var pickupTriggers = GetComponentsInChildren<PickupTrigger>(true);
            for (var i = 0; i < pickupTriggers.Length; i++)
            {
                if (pickupTriggers[i] != null)
                    pickupTriggers[i].gameObject.SetActive(true);
            }

            // Stop presentation audio left over from the previous run before the new run is composed.
            // This prevents a victory/multiplier/footstep clip from leaking into Retry or Next Level.
            var audioSources = GetComponentsInChildren<AudioSource>(true);
            for (var i = 0; i < audioSources.Length; i++)
            {
                if (audioSources[i] != null)
                    audioSources[i].Stop();
            }

            if (pickupStrips == null) return;
            for (var i = 0; i < pickupStrips.Length; i++)
                pickupStrips[i]?.PrepareForRun();
        }
    }
}
