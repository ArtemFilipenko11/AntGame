using UnityEngine;

using AntGame.Food;

namespace AntGame.Systems
{
    public class GameplayContext : MonoBehaviour
    {
        [Header("World")]
        [SerializeField]
        private Transform worldRoot;

        [Header("Food")]
        [SerializeField]
        private Transform foodRoot;

        [SerializeField]
        private FoodSource foodPrefab;

        private FoodRegistry foodRegistry;
        private FoodSpawner foodSpawner;

        private bool initialized;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            InitializeServices();

            Debug.Log("Gameplay initialization completed.");
        }

        private void InitializeServices()
        {
            foodRegistry = new FoodRegistry(worldRoot);
            foodRegistry.Initialize();

            foodSpawner = new FoodSpawner(
                foodPrefab,
                foodRoot,
                foodRegistry
            );

            foodSpawner.Initialize();
        }

        public FoodSource SpawnFood(
            FoodDefinition definition,
            Vector2 position)
        {
            if (!initialized)
            {
                Debug.LogError(
                    "GameplayContext is not initialized."
                );

                return null;
            }

            return foodSpawner.Spawn(
                definition,
                position
            );
        }
    }
}