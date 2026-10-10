using UnityEngine;
using AntGame.Characters.Ant;
using System;
using System.Collections.Generic;

namespace AntGame.Systems
{
    public class AntFactory : IGameplayService
    {
        private readonly AntController antPrefab;
        private readonly Transform spawnParent;
        private readonly AntDependencies dependencies;

        private readonly HashSet<AntController> ants = new();

        public IReadOnlyCollection<AntController> Ants => ants;

        private bool initialized;

        public event Action<AntController> AntRegistered;
        public event Action<AntController> AntUnregistered;
        public event Action AntCarryChanged;
        
        public AntFactory(
            AntController antPrefab,
            Transform spawnParent,
            AntDependencies dependencies)
        {
            this.antPrefab = antPrefab;
            this.spawnParent = spawnParent;
            this.dependencies = dependencies;
        }

        public bool Initialize()
        {
            if (initialized)
                return true;

            if (antPrefab == null)
            {
                Debug.LogError(
                    "AntFactory has no Ant prefab assigned."
                );

                return false;
            }

            if (spawnParent == null)
            {
                Debug.LogError(
                    "AntFactory has no spawn parent assigned."
                );

                return false;
            }

            if (dependencies == null)
            {
                Debug.LogError(
                    "AntFactory has no dependencies."
                );

                return false;
            }
            
            initialized = true;

            InitializeExistingAnts();
    
            return true;
        }

        public bool InitializeExistingAnts()
        {
            if (!initialized)
            {
                Debug.LogError(
                    "Cannot initialize existing ants before AntFactory is initialized."
                );

                return false;
            }

            AntController[] existingAnts =
                spawnParent.GetComponentsInChildren<AntController>(true);

            foreach (AntController ant in existingAnts)
            {
                if (ant == null)
                    continue;

                if (!ant.IsInitialized && !ant.Initialize(dependencies))
                {
                    Debug.LogError(
                        $"Failed to initialize ant '{ant.name}'.",
                        ant
                    );

                    continue;
                }

                RegisterAnt(ant);
            }

            return true;
        }

        
        public AntController Spawn(Vector2 position)
        {
            if (!initialized)
            {
                Debug.LogError(
                    "Cannot spawn Ant before AntFactory is initialized.");

                return null;
            }

            AntController ant = UnityEngine.Object.Instantiate(
                antPrefab,
                position,
                Quaternion.identity,
                spawnParent
            );

            if (!ant.Initialize(dependencies))
            {
                UnityEngine.Object.Destroy(ant.gameObject);
                return null;
            }

            RegisterAnt(ant);
            return ant;
        }

        private void RegisterAnt(AntController ant)
        {
            if (ant == null || !ants.Add(ant))
                return;

            ant.Destroyed += OnAntDestroyed;
            ant.CarriedFoodChanged += OnAntCarriedFoodChanged;
            AntRegistered?.Invoke(ant);
        }

        private void UnregisterAnt(AntController ant)
        {
            if (ReferenceEquals(ant, null) || !ants.Remove(ant))
                return;

            ant.Destroyed -= OnAntDestroyed;
            ant.CarriedFoodChanged -= OnAntCarriedFoodChanged;
            AntUnregistered?.Invoke(ant);
        }

        private void OnAntDestroyed(AntController ant)
        {
            UnregisterAnt(ant);
        }

        private void OnAntCarriedFoodChanged(int _)
        {
            AntCarryChanged?.Invoke();
        }

    }
}