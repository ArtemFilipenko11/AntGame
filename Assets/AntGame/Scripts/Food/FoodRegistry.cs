using System.Collections.Generic;
using UnityEngine;
using AntGame.Systems;

namespace AntGame.Food
{
    [DisallowMultipleComponent]
    public class FoodRegistry : GameplaySystem
    {
        private readonly HashSet<FoodSource> sources = new();

        private bool initialized;

        public IReadOnlyCollection<FoodSource> Sources => sources;

        public override void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            RegisterExistingSources();
        }

        public bool Register(FoodSource source)
        {
            if (!initialized)
                return false;

            if (source == null)
                return false;

            return sources.Add(source);
        }

        public void Unregister(FoodSource source)
        {
            if (source == null)
                return;

            sources.Remove(source);
        }

        private void RegisterExistingSources()
        {
            FoodSource[] existingSources = FindObjectsByType<FoodSource>();

            foreach (FoodSource source in existingSources)
            {
                Register(source);
            }
        }

        public FoodSource FindClosestAvailable(Vector2 position)
        {
            FoodSource closestSource = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (FoodSource source in sources)
            {
                if (source == null || !source.HasFood)
                    continue;

                Vector2 sourcePosition = source.transform.position;

                float distanceSqr =
                    (sourcePosition - position).sqrMagnitude;

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