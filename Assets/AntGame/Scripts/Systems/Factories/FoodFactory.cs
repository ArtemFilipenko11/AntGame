using UnityEngine;
using AntGame.Food;
using AntGame.Systems;
using System.Collections.Generic;

namespace AntGame.Systems
{
    public class FoodFactory : IGameplayService
    {
        private readonly FoodSource foodPrefab;
        private readonly Transform foodRoot;
        private readonly FoodRegistry foodRegistry;

        private bool initialized;

        public FoodFactory(
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

            source.TrySelectRandomVariant();

            if (!foodRegistry.Register(source))
            {
                Object.Destroy(source.gameObject);
                return null;
            }

            return source;
        }

        public bool Despawn(FoodSource source)
        {
            if (!initialized || source == null)
                return false;

            UnityEngine.Object.Destroy(source.gameObject);
            return true;
        }

        public bool DespawnRandomFood()
        {
            if (!initialized || foodRegistry == null)
                return false;

            List<FoodSource> candidates =
                new List<FoodSource>();

            foreach (FoodSource source in foodRegistry.Sources)
            {
                if (source != null && source.HasFood)
                    candidates.Add(source);
            }

            if (candidates.Count == 0)
                return false;

            int index = UnityEngine.Random.Range(
                0,
                candidates.Count
            );

            return Despawn(candidates[index]);
        }
    }
}