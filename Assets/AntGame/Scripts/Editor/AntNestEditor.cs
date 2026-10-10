using UnityEditor;
using UnityEngine;
using AntGame.Nest;

namespace AntGame.Nest.Editor
{
    [CustomEditor(typeof(AntNest))]
    public sealed class AntNestEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            AntNest nest = (AntNest)target;

            EditorGUILayout.LabelField(
                "Storage",
                EditorStyles.boldLabel
            );

            int oldMaxFood = nest.MaxFood;
            int oldStoredFood = nest.StoredFood;

            EditorGUI.BeginDisabledGroup(
                !Application.isPlaying || !nest.IsInitialized
            );

            EditorGUI.BeginChangeCheck();

            int newMaxFood = EditorGUILayout.IntField(
                "Max Food",
                oldMaxFood
            );

            int newStoredFood = EditorGUILayout.IntField(
                "Stored Food",
                oldStoredFood
            );

            bool stockChanged = EditorGUI.EndChangeCheck();

            EditorGUI.EndDisabledGroup();

            if (stockChanged &&
                Application.isPlaying &&
                nest.IsInitialized)
            {
                Undo.RecordObject(nest, "Изменить запас муравейника");

                if (newMaxFood != oldMaxFood)
                    nest.SetMaxFood(newMaxFood);

                if (newStoredFood != oldStoredFood)
                    nest.SetStoredFood(newStoredFood);

                EditorUtility.SetDirty(nest);
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }
    }
}