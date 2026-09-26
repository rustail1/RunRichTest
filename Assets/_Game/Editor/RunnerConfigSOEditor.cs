#if UNITY_EDITOR
using RunRich.Data.Configs;
using UnityEditor;
using UnityEngine;

namespace RunRich.EditorTools
{
    [CustomEditor(typeof(RunnerConfigSO))]
    public sealed class RunnerConfigSOEditor : UnityEditor.Editor
    {
        private SerializedProperty _forwardSpeed;
        private SerializedProperty _slowSpeedWhenPoor;
        private SerializedProperty _swerveSensitivity;
        private SerializedProperty _roadHalfWidth;
        private SerializedProperty _lateralMaxSpeed;
        private SerializedProperty _lateralSmoothTime;
        private SerializedProperty _wallLayerMask;
        private SerializedProperty _wallRaycastDistance;
        private SerializedProperty _wallRaycastPadding;
        private SerializedProperty _maxTurnAngle;
        private SerializedProperty _visualRotationSpeed;
        private SerializedProperty _showSwerveDebug;

        private SerializedProperty _startDelay;
        private SerializedProperty _pathRotationSmoothTime;
        private SerializedProperty _curveSamplesPerSegment;

        private bool _showAdvanced;

        private void OnEnable()
        {
            _forwardSpeed = serializedObject.FindProperty("forwardSpeed");
            _slowSpeedWhenPoor = serializedObject.FindProperty("slowSpeedWhenPoor");
            _startDelay = serializedObject.FindProperty("startDelay");
            _swerveSensitivity = serializedObject.FindProperty("swerveSensitivity");
            _roadHalfWidth = serializedObject.FindProperty("roadHalfWidth");
            _lateralMaxSpeed = serializedObject.FindProperty("lateralMaxSpeed");
            _lateralSmoothTime = serializedObject.FindProperty("lateralSmoothTime");
            _wallLayerMask = serializedObject.FindProperty("wallLayerMask");
            _wallRaycastDistance = serializedObject.FindProperty("wallRaycastDistance");
            _wallRaycastPadding = serializedObject.FindProperty("wallRaycastPadding");
            _maxTurnAngle = serializedObject.FindProperty("maxTurnAngle");
            _visualRotationSpeed = serializedObject.FindProperty("visualRotationSpeed");
            _showSwerveDebug = serializedObject.FindProperty("showSwerveDebug");

            _pathRotationSmoothTime = serializedObject.FindProperty("pathRotationSmoothTime");
            _curveSamplesPerSegment = serializedObject.FindProperty("curveSamplesPerSegment");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "LIVE TUNING: open RunnerConfig.asset and change these values while Play Mode is running. " +
                "The movement controller reads them live. ScriptableObject asset changes can persist after Play Mode.",
                MessageType.Info);

            Section("1. SWERVE", "One drag path updates the target; SmoothDamp is the only lateral motion.");
            Field(_swerveSensitivity, "Sensitivity", "World-space target offset produced by a full-screen drag.");
            Field(_roadHalfWidth, "Road Half Width");
            Field(_lateralMaxSpeed, "Lateral Max Speed");
            Field(_lateralSmoothTime, "Lateral Smooth Time");

            Section("1B. WALL BOUNDS", "Optional original-style left/right raycasts. Keep Wall Layer Mask empty if this level has no dedicated wall layer.");
            Field(_wallLayerMask, "Wall Layer Mask");
            Field(_wallRaycastDistance, "Raycast Distance");
            Field(_wallRaycastPadding, "Wall Padding");

            Section("2. CHARACTER TURN", "Current pointer DeltaX drives yaw; a stationary held pointer returns the visual to forward.");
            Field(_maxTurnAngle, "Max Turn Angle");
            Field(_visualRotationSpeed, "Rotation Speed");

            Section("3. BASIC", null);
            Field(_forwardSpeed, "Forward Speed");
            Field(_slowSpeedWhenPoor, "Slow Speed When Poor", "Optional XAPK wealth-dependent speed branch. Keep disabled for Level01 unless the serialized flag is explicitly verified.");

            Section("4. DEBUG", "Optional Game View overlay. Does not write per-frame Console logs.");
            Field(_showSwerveDebug, "Show Swerve Debug");

            EditorGUILayout.Space(8);
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "Advanced thresholds / path", true);
            if (_showAdvanced)
            {
                EditorGUI.indentLevel++;
                Field(_startDelay, "Start Delay");
                Field(_pathRotationSmoothTime, "Path Rotation Smooth Time");
                Field(_curveSamplesPerSegment, "Curve Samples / Segment");
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void Section(string title, string description)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(description))
                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
        }

        private static void Field(SerializedProperty property, string label, string tooltip = null)
        {
            if (property == null)
                return;

            EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip ?? string.Empty));
        }
    }
}
#endif
