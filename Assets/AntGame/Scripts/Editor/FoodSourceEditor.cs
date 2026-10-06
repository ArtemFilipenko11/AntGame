#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AntGame.Food.Editor
{
    [CustomEditor(typeof(FoodSource))]
    public class FoodSourceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FoodSource foodSource =
                (FoodSource)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(
                "Visual Variant",
                EditorStyles.boldLabel
            );

            EditorGUILayout.LabelField(
                "Current Variant",
                GetCurrentVariantName(foodSource)
            );

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select Previous"))
            {
                SelectPrevious(foodSource);
            }

            if (GUILayout.Button("Select Next"))
            {
                SelectNext(foodSource);
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Select Random"))
            {
                SelectRandom(foodSource);
            }
        }

        private string GetCurrentVariantName(
            FoodSource foodSource)
        {
            FoodDefinition definition =
                foodSource.Definition;

            if (definition == null ||
                definition.VisualVariants == null ||
                definition.VisualVariants.Length == 0)
            {
                return "Не выбран";
            }

            int index = foodSource.SelectedVariantIndex;

            if (index < 0 ||
                index >= definition.VisualVariants.Length)
            {
                return "Некорректный индекс";
            }

            return definition.VisualVariants[index]
                .VariantName;
        }

        private void SelectPrevious(FoodSource foodSource)
        {
            FoodDefinition definition =
                foodSource.Definition;

            if (!HasVariants(definition))
                return;

            int count =
                definition.VisualVariants.Length;

            int newIndex =
                (foodSource.SelectedVariantIndex - 1 + count) % count;

            ApplyVariant(foodSource, newIndex);
        }

        private void SelectNext(FoodSource foodSource)
        {
            FoodDefinition definition =
                foodSource.Definition;

            if (!HasVariants(definition))
                return;

            int count =
                definition.VisualVariants.Length;

            int newIndex =
                (foodSource.SelectedVariantIndex + 1) % count;

            ApplyVariant(foodSource, newIndex);
        }

        private void SelectRandom(FoodSource foodSource)
        {
            ChangeVariant(foodSource,() => foodSource.TrySelectRandomVariant());
        }

        private void ApplyVariant(FoodSource foodSource, int index)
        {
            ChangeVariant(foodSource,() => foodSource.TrySelectVariant(index));
        }

        private void ChangeVariant(FoodSource foodSource, System.Func<bool> changeAction)
        {
            SpriteRenderer spriteRenderer = foodSource.GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                Undo.RecordObject(spriteRenderer, "Изменить визуальный вариант источника еды");
            }

            Undo.RecordObject(foodSource, "Изменить визуальный вариант источника еды");

            if (!changeAction())
                return;

            EditorUtility.SetDirty(foodSource);

            if (spriteRenderer != null)
            {
                EditorUtility.SetDirty(spriteRenderer);
            }

            if (PrefabUtility.IsPartOfPrefabInstance(foodSource))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(foodSource);
            }

            if (spriteRenderer != null &&
                PrefabUtility.IsPartOfPrefabInstance(spriteRenderer))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(spriteRenderer);
            }
        }

        private bool HasVariants(
            FoodDefinition definition)
        {
            return definition != null &&
                   definition.VisualVariants != null &&
                   definition.VisualVariants.Length > 0;
        }
    }
}

#endif