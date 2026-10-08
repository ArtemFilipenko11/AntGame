using UnityEngine;

using AntGame.Systems;

namespace AntGame.Food
{
    public class FoodSpawner : IGameplayService
    {
        private readonly FoodSource foodPrefab;
        private readonly Transform foodRoot;
        private readonly FoodRegistry foodRegistry;

        private bool initialized;

        public FoodSpawner(
            FoodSource foodPrefab,
            Transform foodRoot,
            FoodRegistry foodRegistry)
        {
            this.foodPrefab = foodPrefab;
            this.foodRoot = foodRoot;
            this.foodRegistry = foodRegistry;
        }

        public bool Initialize()
        {
            if (initialized)
                return true;

            if (foodPrefab == null)
            {
                Debug.LogError("FoodSpawner has no FoodSource prefab.");
                return false;
            }

            if (foodRoot == null)
            {
                Debug.LogError("FoodSpawner has no Food root.");
                return false;
            }

            if (foodRegistry == null)
            {
                Debug.LogError("FoodSpawner has no FoodRegistry.");
                return false;
            }

            initialized = true;

            return true;
        }

        public FoodSource Spawn(
            FoodDefinition definition,
            Vector2 position)
        {
            if (!initialized)
            {
                Debug.LogError(
                    "Cannot spawn FoodSource before FoodSpawner is initialized.");

                return null;
            }

            if (definition == null)
            {
                Debug.LogError(
                    "Cannot spawn FoodSource without FoodDefinition.");

                return null;
            }

            FoodSource source = Object.Instantiate(
                foodPrefab,
                position,
                Quaternion.identity,
                foodRoot
            );

            if (!source.Initialize(definition))
            {
                Object.Destroy(source.gameObject);
                return null;
            }

            if (!foodRegistry.Register(source))
            {
                Object.Destroy(source.gameObject);
                return null;
            }

            return source;
        }
    }
}