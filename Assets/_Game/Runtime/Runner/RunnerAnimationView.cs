using RunRich.Runtime.Bootstrap;
using RunRich.Runtime.Flow;
using RunRich.Runtime.Wealth;
using UnityEngine;

namespace RunRich.Runtime.Runner
{
    /// <summary>
    /// Keeps animation behind a small presentation adapter. When a controller is supplied it owns
    /// the pose. The provided character contains a humanoid rig but no clips, so the fallback below
    /// supplies a lightweight procedural idle/run/result pose without another Update.
    /// </summary>
    public sealed class RunnerAnimationView : MonoBehaviour, ITickable
    {
        private static readonly int RunningHash = Animator.StringToHash("Running");
        private static readonly int RichHash = Animator.StringToHash("Rich");

        [SerializeField] private Animator animator;

        private GameFlow _flow;
        private RichPoorController _wealth;
        private GameFlowState _state;
        private bool _rigCached;
        private bool _useProceduralPose;
        private float _time;
        private float _richBlend;
        private WealthState? _cachedAppearanceState;

        private Transform _hips;
        private Transform _spine;
        private Transform _leftUpperLeg;
        private Transform _rightUpperLeg;
        private Transform _leftLowerLeg;
        private Transform _rightLowerLeg;
        private Transform _leftUpperArm;
        private Transform _rightUpperArm;
        private Transform _leftLowerArm;
        private Transform _rightLowerArm;

        private Vector3 _hipsBasePosition;
        private Quaternion _hipsBaseRotation;
        private Quaternion _spineBaseRotation;
        private Quaternion _leftUpperLegBaseRotation;
        private Quaternion _rightUpperLegBaseRotation;
        private Quaternion _leftLowerLegBaseRotation;
        private Quaternion _rightLowerLegBaseRotation;
        private Quaternion _leftUpperArmBaseRotation;
        private Quaternion _rightUpperArmBaseRotation;
        private Quaternion _leftLowerArmBaseRotation;
        private Quaternion _rightLowerArmBaseRotation;

        public void Bind(GameFlow flow, RichPoorController wealth)
        {
            Unbind();
            _flow = flow;
            _wealth = wealth;

            // Retry/Next may reuse an authored player object. Reset the controller to its default
            // Idle state before applying the new GameFlow so no previous run state leaks through.
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.applyRootMotion = false;
                animator.Rebind();
                animator.Update(0f);
            }
            GetComponentInChildren<RunnerVisualGrounding>(true)?.ResetForRun();

            _time = 0f;
            _cachedAppearanceState = null;
            CacheRig(true);
            if (_flow == null) return;

            _flow.StateChanged += HandleStateChanged;
            if (_wealth != null)
            {
                _wealth.Changed += HandleWealthChanged;
                HandleWealthChanged(_wealth.Value, _wealth.State);
            }
            HandleStateChanged(_flow.State);
        }

        public void Tick(float deltaTime)
        {
            if (!_useProceduralPose || !_rigCached)
                return;

            _time += Mathf.Max(0f, deltaTime);
            ApplyProceduralPose();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void HandleStateChanged(GameFlowState state)
        {
            _state = state;
            if (animator == null) return;

            animator.applyRootMotion = false;
            if (animator.runtimeAnimatorController != null && HasParameter(animator, RunningHash, AnimatorControllerParameterType.Bool))
                animator.SetBool(RunningHash, state == GameFlowState.Playing);
        }

        private void HandleWealthChanged(int value, WealthState state)
        {
            if (_cachedAppearanceState != state)
                CacheRig(true);

            _cachedAppearanceState = state;
            _richBlend = _wealth != null ? _wealth.Normalized : 0f;
            if (animator != null && animator.runtimeAnimatorController != null &&
                HasParameter(animator, RichHash, AnimatorControllerParameterType.Float))
                animator.SetFloat(RichHash, _richBlend);
        }

        private void CacheRig(bool force = false)
        {
            if (_rigCached && !force)
                return;

            _rigCached = false;
            _useProceduralPose = false;
            _hips = null;
            _spine = null;
            _leftUpperLeg = null;
            _rightUpperLeg = null;
            _leftLowerLeg = null;
            _rightLowerLeg = null;
            _leftUpperArm = null;
            _rightUpperArm = null;
            _leftLowerArm = null;
            _rightLowerArm = null;

            var searchRoot = animator != null ? animator.transform : transform;
            if (animator != null && animator.isHuman)
            {
                _hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                _spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                _leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                _rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                _leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                _rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                _leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                _rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                _leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                _rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            }
            else
            {
                _hips = FindDeep(searchRoot, "mixamorig:Hips");
                _spine = FindDeep(searchRoot, "mixamorig:Spine");
                _leftUpperLeg = FindDeep(searchRoot, "mixamorig:LeftUpLeg");
                _rightUpperLeg = FindDeep(searchRoot, "mixamorig:RightUpLeg");
                _leftLowerLeg = FindDeep(searchRoot, "mixamorig:LeftLeg");
                _rightLowerLeg = FindDeep(searchRoot, "mixamorig:RightLeg");
                _leftUpperArm = FindDeep(searchRoot, "mixamorig:LeftArm");
                _rightUpperArm = FindDeep(searchRoot, "mixamorig:RightArm");
                _leftLowerArm = FindDeep(searchRoot, "mixamorig:LeftForeArm");
                _rightLowerArm = FindDeep(searchRoot, "mixamorig:RightForeArm");
            }

            if (_hips == null || _spine == null || _leftUpperLeg == null || _rightUpperLeg == null ||
                _leftUpperArm == null || _rightUpperArm == null)
                return;

            _hipsBasePosition = _hips.localPosition;
            _hipsBaseRotation = _hips.localRotation;
            _spineBaseRotation = _spine.localRotation;
            _leftUpperLegBaseRotation = _leftUpperLeg.localRotation;
            _rightUpperLegBaseRotation = _rightUpperLeg.localRotation;
            _leftLowerLegBaseRotation = LocalRotationOrIdentity(_leftLowerLeg);
            _rightLowerLegBaseRotation = LocalRotationOrIdentity(_rightLowerLeg);
            _leftUpperArmBaseRotation = _leftUpperArm.localRotation;
            _rightUpperArmBaseRotation = _rightUpperArm.localRotation;
            _leftLowerArmBaseRotation = LocalRotationOrIdentity(_leftLowerArm);
            _rightLowerArmBaseRotation = LocalRotationOrIdentity(_rightLowerArm);

            _rigCached = true;
            _useProceduralPose = animator == null || animator.runtimeAnimatorController == null;
        }

        private void ApplyProceduralPose()
        {
            var idleBreath = Mathf.Sin(_time * 2.2f);
            var cadence = Mathf.Lerp(7.8f, 10.6f, _richBlend);
            var stride = _state == GameFlowState.Playing ? Mathf.Sin(_time * cadence) : 0f;
            var bob = _state == GameFlowState.Playing
                ? Mathf.Abs(Mathf.Sin(_time * cadence)) * Mathf.Lerp(0.025f, 0.045f, _richBlend)
                : idleBreath * 0.008f;

            var leftArmDown = 58f;
            var rightArmDown = -58f;
            var spinePitch = Mathf.Lerp(10f, -3f, _richBlend);
            var hipsYaw = 0f;

            if (_state == GameFlowState.Won)
            {
                leftArmDown = -38f;
                rightArmDown = 38f;
                hipsYaw = Mathf.Sin(_time * 4f) * 8f;
                bob = Mathf.Abs(Mathf.Sin(_time * 5f)) * 0.05f;
            }
            else if (_state == GameFlowState.Lost)
            {
                spinePitch = 18f;
                leftArmDown = 76f;
                rightArmDown = -76f;
                bob = 0f;
            }

            _hips.localPosition = _hipsBasePosition + Vector3.up * bob;
            _hips.localRotation = _hipsBaseRotation * Quaternion.Euler(0f, hipsYaw, 0f);
            _spine.localRotation = _spineBaseRotation * Quaternion.Euler(spinePitch, 0f, idleBreath * 1.5f);

            var legSwing = stride * Mathf.Lerp(18f, 30f, _richBlend);
            _leftUpperLeg.localRotation = _leftUpperLegBaseRotation * Quaternion.Euler(legSwing, 0f, 0f);
            _rightUpperLeg.localRotation = _rightUpperLegBaseRotation * Quaternion.Euler(-legSwing, 0f, 0f);
            SetLocalRotation(_leftLowerLeg, _leftLowerLegBaseRotation, Mathf.Max(0f, -stride) * 24f, 0f);
            SetLocalRotation(_rightLowerLeg, _rightLowerLegBaseRotation, Mathf.Max(0f, stride) * 24f, 0f);

            var armSwing = _state == GameFlowState.Playing ? -stride * 21f : 0f;
            _leftUpperArm.localRotation = _leftUpperArmBaseRotation * Quaternion.Euler(armSwing, 0f, leftArmDown);
            _rightUpperArm.localRotation = _rightUpperArmBaseRotation * Quaternion.Euler(-armSwing, 0f, rightArmDown);
            SetLocalRotation(_leftLowerArm, _leftLowerArmBaseRotation, 0f, -8f);
            SetLocalRotation(_rightLowerArm, _rightLowerArmBaseRotation, 0f, 8f);
        }

        private static bool HasParameter(Animator target, int nameHash, AnimatorControllerParameterType type)
        {
            if (target == null) return false;
            var parameters = target.parameters;
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].nameHash == nameHash && parameters[i].type == type)
                    return true;
            }
            return false;
        }

        private static Quaternion LocalRotationOrIdentity(Transform bone)
        {
            return bone != null ? bone.localRotation : Quaternion.identity;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null || !root.gameObject.activeInHierarchy) return null;
            if (root.name == name) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindDeep(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static void SetLocalRotation(Transform bone, Quaternion baseRotation, float x, float z)
        {
            if (bone != null)
                bone.localRotation = baseRotation * Quaternion.Euler(x, 0f, z);
        }

        private void Unbind()
        {
            if (_flow != null)
            {
                _flow.StateChanged -= HandleStateChanged;
                _flow = null;
            }
            if (_wealth != null)
            {
                _wealth.Changed -= HandleWealthChanged;
                _wealth = null;
            }
        }
    }
}
