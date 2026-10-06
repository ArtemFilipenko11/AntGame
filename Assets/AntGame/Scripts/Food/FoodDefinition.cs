using System;
using UnityEngine;

namespace AntGame.Food
{
    [CreateAssetMenu(
        fileName = "FoodDefinition",
        menuName = "AntGame/Food/Food Definition")]
    public class FoodDefinition : ScriptableObject
    {
        [Serializable]
        public class FoodVisualVariant
        {
            [SerializeField]
            private string variantName = "Variant";

            [SerializeField]
            private Sprite[] states = Array.Empty<Sprite>();

            public string VariantName => variantName;
            public Sprite[] States => states;
        }

        [Header("Identity")]
        [SerializeField]
        private string displayName = "Food";

        [Header("Gameplay")]
        [SerializeField, Min(1)]
        private int maxFood = 10;

        [SerializeField, Min(0f)]
        private float gatherDuration = 1f;

        [Header("Visuals")]
        [SerializeField]
        private FoodVisualVariant[] visualVariants =
            Array.Empty<FoodVisualVariant>();

        public string DisplayName => displayName;
        public int MaxFood => maxFood;
        public float GatherDuration => gatherDuration;
        public FoodVisualVariant[] VisualVariants => visualVariants;

#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                Debug.LogWarning(
                    $"FoodDefinition '{name}' has an empty display name.",
                    this);
            }

            if (maxFood < 1)
            {
                Debug.LogError(
                    $"FoodDefinition '{name}' must have Max Food greater than 0.",
                    this);
            }

            if (visualVariants == null || visualVariants.Length == 0)
            {
                Debug.LogError(
                    $"FoodDefinition '{name}' has no visual variants.",
                    this);

                return;
            }

            for (int variantIndex = 0;
                 variantIndex < visualVariants.Length;
                 variantIndex++)
            {
                FoodVisualVariant variant = visualVariants[variantIndex];

                if (variant == null)
                {
                    Debug.LogError(
                        $"FoodDefinition '{name}' contains a null visual variant at index {variantIndex}.",
                        this);

                    continue;
                }

                if (string.IsNullOrWhiteSpace(variant.VariantName))
                {
                    Debug.LogWarning(
                        $"FoodDefinition '{name}' has a visual variant with an empty name at index {variantIndex}.",
                        this);
                }

                Sprite[] states = variant.States;

                if (states == null || states.Length == 0)
                {
                    Debug.LogError(
                        $"FoodDefinition '{name}', variant '{variant.VariantName}' has no states.",
                        this);

                    continue;
                }

                for (int stateIndex = 0;
                     stateIndex < states.Length;
                     stateIndex++)
                {
                    if (states[stateIndex] == null)
                    {
                        Debug.LogError(
                            $"FoodDefinition '{name}', variant '{variant.VariantName}' has a missing sprite at state index {stateIndex}.",
                            this);
                    }
                }
            }
        }
#endif
    }
}