using UnityEditor;
using UnityEngine;
using AntGame.Systems;

namespace AntGame.Editor
{
    [CustomEditor(typeof(GameplayContext))]
    public class GameplayContextEditor : UnityEditor.Editor
    {
        private Vector2 spawnPosition;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(
                "Debug Controls",
                EditorStyles.boldLabel
            );

            spawnPosition = EditorGUILayout.Vector2Field(
                "Ant Spawn Position",
                spawnPosition
            );

            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("Spawn Ant"))
            {
                GameplayContext context =
                    (GameplayContext)target;

                AntGame.Characters.Ant.AntController ant =
                    context.SpawnAnt(spawnPosition);

                if (ant != null)
                {
                    Selection.activeGameObject = ant.gameObject;

                    Debug.Log(
                        $"Ant spawned at {spawnPosition}.",
                        ant
                    );
                }
                else
                {
                    Debug.LogError(
                        "Failed to spawn ant. Check GameplayContext initialization."
                    );
                }
            }

            GUI.enabled = true;
        }
    }
}