using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AntGame.Systems;
using AntGame.Food;

namespace AntGame.Editor
{
    [CustomEditor(typeof(GameplayContext))]
    public class GameplayContextEditor : UnityEditor.Editor
    {
        private Vector2 antSpawnPosition;
        private Vector2 foodSpawnPosition;
        private FoodDefinition foodDefinition;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GameplayContext context = (GameplayContext)target;

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(
                "Debug Controls",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "Ant",
                EditorStyles.boldLabel
            );

            antSpawnPosition = EditorGUILayout.Vector2Field(
                "Spawn Position",
                antSpawnPosition
            );

            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("Spawn Ant"))
            {
                AntGame.Characters.Ant.AntController ant =
                    context.SpawnAnt(antSpawnPosition);

                if (ant != null)
                {
                    Selection.activeGameObject = ant.gameObject;

                    Debug.Log(
                        $"Ant spawned at {antSpawnPosition}.",
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

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "Mushroom",
                EditorStyles.boldLabel
            );

            foodSpawnPosition = EditorGUILayout.Vector2Field(
                "Spawn Position",
                foodSpawnPosition
            );

            foodDefinition =
                (FoodDefinition)EditorGUILayout.ObjectField(
                    "Food Definition (empty = random)",
                    foodDefinition,
                    typeof(FoodDefinition),
                    false
                );

            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("Spawn Mushroom"))
            {
                FoodDefinition definitionToSpawn = foodDefinition;

                if (definitionToSpawn == null)
                    definitionToSpawn = GetRandomFoodDefinition();

                if (definitionToSpawn == null)
                {
                    Debug.LogError(
                        "No FoodDefinition assets found in the project."
                    );
                }
                else
                {
                    FoodSource source = context.SpawnFood(
                        definitionToSpawn,
                        foodSpawnPosition
                    );

                    if (source != null)
                    {
                        Selection.activeGameObject = source.gameObject;

                        Debug.Log(
                            $"Mushroom '{definitionToSpawn.DisplayName}' spawned at {foodSpawnPosition}.",
                            source
                        );
                    }
                    else
                    {
                        Debug.LogError(
                            "Failed to spawn mushroom. Check GameplayContext initialization and FoodSource prefab."
                        );
                    }
                }
            }

            GUI.enabled = true;
        }

        private FoodDefinition GetRandomFoodDefinition()
        {
            string[] assetGuids =
                AssetDatabase.FindAssets("t:FoodDefinition");

            List<FoodDefinition> definitions = new();

            foreach (string assetGuid in assetGuids)
            {
                string assetPath =
                    AssetDatabase.GUIDToAssetPath(assetGuid);

                FoodDefinition definition =
                    AssetDatabase.LoadAssetAtPath<FoodDefinition>(
                        assetPath
                    );

                if (definition != null)
                    definitions.Add(definition);
            }

            if (definitions.Count == 0)
                return null;

            int randomIndex = Random.Range(
                0,
                definitions.Count
            );

            return definitions[randomIndex];
        }
    }
}