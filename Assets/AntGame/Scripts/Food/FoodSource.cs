using System;
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

        [Header("Discovery Visual")]
        [SerializeField]
        private Color undiscoveredTint = new Color(0.55f, 0.55f, 0.55f, 1f);

        private bool isDiscovered;
        private Color discoveredColor = Color.white;
        private bool hasCachedDiscoveredColor;

        public bool IsDiscovered => isDiscovered;

        private int currentFood;
        private int maxFood;
        private bool initialized;

        
        public FoodDefinition Definition => definition;

        public int SelectedVariantIndex => selectedVariantIndex;

        public bool IsInitialized => initialized;

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

        public event Action<FoodSource> Destroyed;
        public event Action<FoodSource, int, int> FoodAmountChanged;

        public int MaxFood =>
            initialized
                ? maxFood
                : definition != null ? definition.MaxFood : 0;

        public int CurrentFood =>
            initialized ? currentFood : MaxFood;

        public bool HasFood => CurrentFood > 0;

        private void Awake()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null && !hasCachedDiscoveredColor)
            {
                discoveredColor = spriteRenderer.color;
                hasCachedDiscoveredColor = true;
            }
        }

        public bool Initialize()
        {
            return Initialize(definition);
        }

        public bool Initialize(FoodDefinition newDefinition)
        {
            if (initialized)
                return true;

            if (newDefinition == null)
            {
                Debug.LogError(
                    $"FoodSource on {name} received no FoodDefinition.",
                    this);

                return false;
            }

            definition = newDefinition;

            CacheReferences();

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

            maxFood = Mathf.Max(1, definition.MaxFood);
            currentFood = maxFood;

            initialized = true;

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

            if (index < 0 ||
                index >= definition.VisualVariants.Length)
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

            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    definition.VisualVariants.Length
                );

            return TrySelectVariant(randomIndex);
        }

        public void MarkDiscovered()
        {
            if (isDiscovered)
                return;

            isDiscovered = true;
            ApplyVisual();
        }

        public void ApplyVisual()
        {
            if (spriteRenderer == null)
                return;
            
            spriteRenderer.color = isDiscovered
                ? discoveredColor
                : discoveredColor * undiscoveredTint;

            FoodDefinition.FoodVisualVariant variant =
                SelectedVariant;

            if (variant == null)
                return;

            Sprite[] states = variant.States;

            if (states == null || states.Length == 0)
                return;

            int foodForVisual = CurrentFood;

            int maxFoodForVisual = MaxFood;

            if (foodForVisual <= 0)
                return;

            int stateIndex =
                GetVisualStateIndex(
                    foodForVisual,
                    maxFoodForVisual,
                    states.Length
                );

            spriteRenderer.sprite = states[stateIndex];
        }

        private int GetVisualStateIndex(
            int foodAmount,
            int maxFood,
            int stateCount)
        {
            if (stateCount <= 1 ||
                maxFood <= 1)
            {
                return 0;
            }

            float depletionRatio =
                (float)(maxFood - foodAmount)
                / (maxFood - 1);

            int stateIndex =
                Mathf.FloorToInt(
                    depletionRatio * (stateCount - 1)
                );

            return Mathf.Clamp(
                stateIndex,
                0,
                stateCount - 1
            );
        }

        public bool TryCollect()
        {
            return TryCollect(1) > 0;
        }

        public int TryCollect(int requestedAmount)
        {
            if (!initialized ||
                requestedAmount <= 0 ||
                !HasFood)
            {
                return 0;
            }

            int collectedAmount = Mathf.Min(
                requestedAmount,
                currentFood
            );

            currentFood -= collectedAmount;

            FoodAmountChanged?.Invoke(this, currentFood, maxFood);

            if (currentFood <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                ApplyVisual();
            }

            return collectedAmount;
        }

        public bool SetMaxFood(int value)
        {
            if (!initialized)
                return false;

            maxFood = Mathf.Max(1, value);
            currentFood = Mathf.Min(currentFood, maxFood);

            ApplyVisual();
            FoodAmountChanged?.Invoke(this, currentFood, maxFood);

            return true;
        }

        public bool SetCurrentFood(int value)
        {
            if (!initialized)
                return false;

            currentFood = Mathf.Clamp(value, 0, maxFood);

            FoodAmountChanged?.Invoke(this, currentFood, maxFood);

            if (currentFood <= 0)
            {
                Destroy(gameObject);
                return true;
            }

            ApplyVisual();

            return true;
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }
    }
}