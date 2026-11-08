using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DestructableFigures
{
    [CustomEditor(typeof(FigureBuilder))]
    public class FigureBuilderEditor : Editor
    {
        private SerializedProperty _type;
        private SerializedProperty _samplePart;
        private SerializedProperty _center;
        private SerializedProperty _cubeSize;
        private SerializedProperty _cubeStep;

        private void OnEnable()
        {
            _type = serializedObject.FindProperty("_type");
            _samplePart = serializedObject.FindProperty("_samplePart");
            _center = serializedObject.FindProperty("_center");
            _cubeSize = serializedObject.FindProperty("_cubeSize");
            _cubeStep = serializedObject.FindProperty("_cubeStep");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_type);
            EditorGUILayout.PropertyField(_samplePart);
            EditorGUILayout.PropertyField(_center);

            EditorGUILayout.Space();

            switch ((FigureType)_type.enumValueIndex)
            {
                case FigureType.Cube:
                    EditorGUILayout.LabelField("Cube", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(_cubeSize);
                    EditorGUILayout.PropertyField(_cubeStep);
                    break;
                case FigureType.Cone:
                    EditorGUILayout.LabelField("Cone", EditorStyles.boldLabel);
                    break;
                case FigureType.Pyramid:
                    EditorGUILayout.LabelField("Pyramid", EditorStyles.boldLabel);
                    break;
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Build Figure", GUILayout.Height(28)))
            {
                var builder = (FigureBuilder)target;
                builder.BuildFigure();

                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
            }
        }
    }
}