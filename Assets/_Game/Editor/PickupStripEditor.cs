#if UNITY_EDITOR
using RunRich.Runtime.Pickups;
using UnityEditor;
using UnityEngine;

namespace RunRich.EditorTools
{
    [CustomEditor(typeof(PickupStrip))]
    public sealed class PickupStripEditor : UnityEditor.Editor
    {
        private SerializedProperty _rowSpacing;
        private SerializedProperty _slotSpacing;
        private SerializedProperty _verticalOffset;
        private SerializedProperty _rows;

        private void OnEnable()
        {
            _rowSpacing = serializedObject.FindProperty("rowSpacing");
            _slotSpacing = serializedObject.FindProperty("slotSpacing");
            _verticalOffset = serializedObject.FindProperty("verticalOffset");
            _rows = serializedObject.FindProperty("rows");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Pickup Strip", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Each row has exactly 4 road slots. Leave a slot empty for no pickup, or assign Money/Bottle definitions.",
                MessageType.Info);

            EditorGUILayout.PropertyField(_rowSpacing);
            EditorGUILayout.PropertyField(_slotSpacing);
            EditorGUILayout.PropertyField(_verticalOffset);
            EditorGUILayout.Space(8);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Rows", EditorStyles.boldLabel);
                if (GUILayout.Button("+ Row", GUILayout.Width(70)))
                    _rows.arraySize++;
                if (GUILayout.Button("- Row", GUILayout.Width(70)) && _rows.arraySize > 0)
                    _rows.arraySize--;
            }

            EditorGUILayout.Space(4);
            for (var i = 0; i < _rows.arraySize; i++)
            {
                var row = _rows.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"Row {i:00}", EditorStyles.miniBoldLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        for (var slot = 0; slot < 4; slot++)
                        {
                            var property = row.FindPropertyRelative($"slot{slot}");
                            EditorGUILayout.PropertyField(property, new GUIContent($"S{slot}"), GUILayout.MinWidth(90));
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
