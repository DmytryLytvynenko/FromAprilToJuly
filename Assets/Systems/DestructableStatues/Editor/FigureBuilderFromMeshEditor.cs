using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DestructableFigures
{
    [CustomEditor(typeof(FigureBuilderFromMesh))]
    public class FigureBuilderFromMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("Build From Mesh", GUILayout.Height(28)))
            {
                var builder = (FigureBuilderFromMesh)target;
                builder.BuildFromMesh();
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(builder.gameObject.scene);
            }
        }
    }
}