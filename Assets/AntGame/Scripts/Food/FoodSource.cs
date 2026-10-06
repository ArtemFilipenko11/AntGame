using UnityEngine;

namespace AntGame.Food
{
    public class FoodSource : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private FoodDefinition definition;

        [Header("References")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private int currentFood;
        private FoodDefinition.FoodVisualVariant selectedVisualVariant;

        public FoodDefinition Definition => definition;

        public int CurrentFood => currentFood;

        public bool HasFood => currentFood > 0;

        private void Awake()
        {
            if (!Initialize())
                return;
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
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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

            currentFood = definition.MaxFood;

            SelectRandomVisualVariant();
            UpdateVisual();

            return true;
        }


        

        private void SelectRandomVisualVariant()
        {
            FoodDefinition.FoodVisualVariant[] variants =
                definition.VisualVariants;

            int index = Random.Range(0, variants.Length);

            selectedVisualVariant = variants[index];
        }

        private void UpdateVisual()
        {
            if (selectedVisualVariant == null)
                return;

            Sprite[] states = selectedVisualVariant.States;

            if (states == null || states.Length == 0)
                return;

            int stateIndex = GetVisualStateIndex();

            spriteRenderer.sprite = states[stateIndex];
        }

        private int GetVisualStateIndex()
        {
            int stateCount = selectedVisualVariant.States.Length;

            if (stateCount <= 1 || definition.MaxFood <= 1)
                return 0;

            float depletionRatio =
                (float)(definition.MaxFood - currentFood)
                / (definition.MaxFood - 1);

            int stateIndex = Mathf.FloorToInt(
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
            if (!HasFood)
                return false;

            currentFood--;

            if (currentFood <= 0)
            {
                Destroy(gameObject);
                return true;
            }

            UpdateVisual();

            return true;
        }
    }
}