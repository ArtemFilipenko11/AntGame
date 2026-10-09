using UnityEditor;
using UnityEngine;
using AntGame.Characters.Ant;

namespace AntGame.Editor
{
    [CustomEditor(typeof(AntController))]
    public class AntControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(
                "Debug Controls",
                EditorStyles.boldLabel
            );

            AntController ant = (AntController)target;

            EditorGUILayout.LabelField(
                "Initialized",
                ant.IsInitialized.ToString()
            );

            EditorGUILayout.LabelField(
                "Carried Food",
                ant.CarriedFood.ToString()
            );

            GUI.enabled = Application.isPlaying &&
                          ant.IsInitialized;

            if (GUILayout.Button("Start Foraging"))
            {
                if (!ant.StartForaging())
                {
                    Debug.LogWarning(
                        "Could not start foraging. Check food availability and AI state.",
                        ant
                    );
                }
            }

            GUI.enabled = true;
        }
    }
}