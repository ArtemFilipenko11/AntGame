using System.Collections.Generic;
using UnityEngine;

using AntGame.Systems;

namespace AntGame.Food
{
  
    public class FoodRegistry : IGameplayService
    {
        private readonly HashSet<FoodSource> sources = new();

        private readonly Transform worldRoot;

        private bool initialized;

        public bool IsInitialized => initialized;
        
        public IReadOnlyCollection<FoodSource> Sources => sources;

        public FoodRegistry(Transform worldRoot)
        {
            this.worldRoot = worldRoot;
        }

        public bool Initialize()
        {
            if (initialized)
            {
                return true;
            }

            

            if (worldRoot == null)
            {
                Debug.LogError(
                    "FoodRegistry: WorldRoot is not assigned."
                );

                return false;
            }

            initialized = true;

            FoodSource[] existingSources =
                worldRoot.GetComponentsInChildren<FoodSource>(true);

            foreach (FoodSource source in existingSources)
            {
                if (!source.Initialize())
                {
                    continue;
                }

                Register(source);
            }
            return true;
        }

        public bool Register(FoodSource source)
        {
            if (!initialized)
            {
                Debug.LogError(
                    "FoodRegistry: попытка зарегистрировать FoodSource " +
                    "до инициализации реестра."
                );

                return false;
            }

            if (source == null)
            {
                return false;
            }

            if (!sources.Add(source))
            {
                return false;
            }

            source.Destroyed += OnFoodSourceDestroyed;

            return true;
        }

        public bool Unregister(FoodSource source)
        {
            if (source == null)
            {
                return false;
            }

            if (!sources.Remove(source))
            {
                return false;
            }

            source.Destroyed -= OnFoodSourceDestroyed;

            return true;
        }

        private void OnFoodSourceDestroyed(FoodSource source)
        {
            Unregister(source);
        }

        public FoodSource FindClosestAvailable(Vector2 position)
        {
            FoodSource closestSource = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (FoodSource source in sources)
            {
                if (source == null || !source.HasFood)
                {
                    continue;
                }

                float distanceSqr =
                    ((Vector2)source.transform.position - position).sqrMagnitude;

                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestSource = source;
                }
            }

            return closestSource;
        }
    }
}