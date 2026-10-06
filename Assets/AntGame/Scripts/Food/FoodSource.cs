using UnityEngine;

namespace AntGame.Food
{
    public class FoodSource : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private FoodDefinition definition;

        [SerializeField, Min(0)]
        private int selectedVariantIndex;

        [Header("References")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        private int currentFood;

        public FoodDefinition Definition => definition;

        public int CurrentFood => currentFood;

        public bool HasFood => currentFood > 0;

        public int SelectedVariantIndex => selectedVariantIndex;

        private FoodDefinition.FoodVisualVariant SelectedVariant
        {
            get
            {
                if (definition == null ||
                    definition.VisualVariants == null ||
                    definition.VisualVariants.Length == 0)
                {
                    return null;
                }

                if (selectedVariantIndex < 0 ||
                    selectedVariantIndex >= definition.VisualVariants.Length)
                {
                    return null;
                }

                return definition.VisualVariants[selectedVariantIndex];
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private bool Initialize()
        {
            if (definition == null)
            {
                Debug.LogError(
                    $"FoodSource on {name} has no FoodDefinition.",
                    this);

                return false;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer =
                    GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                Debug.LogError(
                    $"FoodSource on {name} has no SpriteRenderer.",
                    this);

                return false;
            }

            if (definition.VisualVariants == null ||
                definition.VisualVariants.Length == 0)
            {
                Debug.LogError(
                    $"FoodDefinition '{definition.name}' has no visual variants.",
                    definition);

                return false;
            }

            selectedVariantIndex = Mathf.Clamp(
                selectedVariantIndex,
                0,
                definition.VisualVariants.Length - 1
            );

            currentFood = definition.MaxFood;

            ApplyVisual();

            return true;
        }

        public bool TrySelectVariant(int index)
        {
            if (definition == null ||
                definition.VisualVariants == null ||
                definition.VisualVariants.Length == 0)
            {
                return false;
            }

            if (index < 0 || index >= definition.VisualVariants.Length)
            {
                return false;
            }

            selectedVariantIndex = index;

            ApplyVisual();

            return true;
        }

        public bool TrySelectRandomVariant()
        {
            if (definition == null ||
                definition.VisualVariants == null ||
                definition.VisualVariants.Length == 0)
            {
                return false;
            }

            int randomIndex = Random.Range(
                0,
                definition.VisualVariants.Length
            );

            return TrySelectVariant(randomIndex);
        }

        public void ApplyVisual()
        {
            FoodDefinition.FoodVisualVariant variant =
                SelectedVariant;

            if (variant == null)
                return;

            Sprite[] states = variant.States;

            if (states == null || states.Length == 0)
                return;

            int foodForVisual = Application.isPlaying
                ? currentFood
                : definition.MaxFood;

            int stateIndex = GetVisualStateIndex(
                foodForVisual,
                definition.MaxFood,
                states.Length
            );

            spriteRenderer.sprite = states[stateIndex];
        }

        private int GetVisualStateIndex(
            int foodAmount,
            int maxFood,
            int stateCount)
        {
            if (stateCount <= 1 || maxFood <= 1)
                return 0;

            float depletionRatio =
                (float)(maxFood - foodAmount)
                / (maxFood - 1);

            return Mathf.Clamp(
                Mathf.FloorToInt(
                    depletionRatio * (stateCount - 1)
                ),
                0,
                stateCount - 1
            );
        }

        public bool TryCollect()
        {
            if (!HasFood)
                return false;

            currentFood--;

            if (currentFood <= 0)
            {
                Destroy(gameObject);
                return true;
            }

            ApplyVisual();

            return true;
        }
    }
}