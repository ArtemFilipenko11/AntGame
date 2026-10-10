using System;
using System.Collections.Generic;
using AntGame.Characters.Ant;
using AntGame.Food;
using AntGame.Nest;
using AntGame.Core.Navigation;

using UnityEngine;

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

        [Header("Ant")]
        [SerializeField]
        private AntNest antNest;

        [SerializeField]
        private AntController antPrefab;

        [SerializeField]
        private Transform antRoot;

        [Header("Navigation")]
        [SerializeField]
        private Vector2 navigationCenter;

        [SerializeField]
        private Vector2 navigationSize = new Vector2(20f, 20f);

        [SerializeField, Min(0.1f)]
        private float navigationCellSize = 0.5f;

        [SerializeField]
        private LayerMask obstacleLayers;

        [SerializeField]
        private bool detectObstacles;

        public IReadOnlyCollection<AntController> Ants =>
            antFactory != null
                ? antFactory.Ants
                : Array.Empty<AntController>();

        private FoodRegistry foodRegistry;
        private FoodFactory foodFactory;
        private AntFactory antFactory;
        private GridNavigationSystem navigationSystem;

        private bool initialized;

        private static readonly FoodSource[] EmptyFoodSources =
            Array.Empty<FoodSource>();

        public bool IsInitialized => initialized;

        public event Action Initialized;

        public event Action<int, int> NestStorageChanged;

        public event Action<FoodSource> FoodRegistered;
        public event Action<FoodSource> FoodUnregistered;
        public event Action InventoryChanged;

        public event Action<AntController> AntRegistered;
        public event Action<AntController> AntUnregistered;
        public event Action AntCarryChanged;

        public int StoredFood =>
            antNest != null ? antNest.StoredFood : 0;

        public int MaxFood =>
            antNest != null ? antNest.MaxFood : 0;

        public IReadOnlyCollection<FoodSource> FoodSources =>
            foodRegistry != null
                ? foodRegistry.Sources
                : EmptyFoodSources;

        public int FoodSourceCount
        {
            get
            {
                int count = 0;

                foreach (FoodSource source in FoodSources)
                {
                    if (source != null && source.HasFood)
                        count++;
                }

                return count;
            }
        }

        public int AvailableFoodAmount
        {
            get
            {
                int total = 0;

                foreach (FoodSource source in FoodSources)
                {
                    if (source != null && source.HasFood)
                        total += source.CurrentFood;
                }

                return total;
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (initialized)
                return;

            if (!InitializeServices())
            {
                Debug.LogError("Gameplay initialization failed.");
                return;
            }

            SubscribeToServices();

            initialized = true;
            Initialized?.Invoke();
            return;
        }

        private bool InitializeServices()
        {
            navigationSystem = new GridNavigationSystem();

            navigationSystem.Initialize(
                navigationCenter,
                navigationSize,
                navigationCellSize,
                obstacleLayers,
                detectObstacles
            );

            foodRegistry = new FoodRegistry(worldRoot);

            if (!foodRegistry.Initialize())
                return false;

            foodFactory = new FoodFactory(
                foodPrefab,
                foodRoot,
                foodRegistry
            );

            if (!foodFactory.Initialize())
                return false;

            if (antNest == null)
            {
                Debug.LogError(
                    "GameplayContext has no AntNest assigned."
                );

                return false;
            }

            if (!antNest.Initialize())
                return false;

            AntDependencies antDependencies = new AntDependencies(
                antNest,
                foodRegistry,
                navigationSystem
            );

            antFactory = new AntFactory(
                antPrefab,
                antRoot,
                antDependencies
            );

            if (!antFactory.Initialize())
                return false;

            return true;
        }

        private void SubscribeToServices()
        {
            antNest.FoodStorageChanged += OnNestStorageChanged;

            foodRegistry.FoodRegistered += OnFoodRegistered;
            foodRegistry.FoodUnregistered += OnFoodUnregistered;
            foodRegistry.InventoryChanged += OnFoodInventoryChanged;

            antFactory.AntRegistered += OnAntRegistered;
            antFactory.AntUnregistered += OnAntUnregistered;
            antFactory.AntCarryChanged += OnAntCarryChanged;
        }

        private void UnsubscribeFromServices()
        {
            if (antNest != null)
                antNest.FoodStorageChanged -= OnNestStorageChanged;

            if (foodRegistry != null)
            {
                foodRegistry.FoodRegistered -= OnFoodRegistered;
                foodRegistry.FoodUnregistered -= OnFoodUnregistered;
                foodRegistry.InventoryChanged -= OnFoodInventoryChanged;
            }

            if (antFactory != null)
            {
                antFactory.AntRegistered -= OnAntRegistered;
                antFactory.AntUnregistered -= OnAntUnregistered;
                antFactory.AntCarryChanged -= OnAntCarryChanged;
            }
        }

        private void OnNestStorageChanged(int storedFood, int maxFood)
        {
            NestStorageChanged?.Invoke(storedFood, maxFood);
        }

        private void OnFoodRegistered(FoodSource source)
        {
            FoodRegistered?.Invoke(source);
        }

        private void OnFoodUnregistered(FoodSource source)
        {
            FoodUnregistered?.Invoke(source);
        }

        private void OnFoodInventoryChanged()
        {
            InventoryChanged?.Invoke();
        }

        private void OnAntRegistered(AntController ant)
        {
            AntRegistered?.Invoke(ant);
        }

        private void OnAntUnregistered(AntController ant)
        {
            AntUnregistered?.Invoke(ant);
        }

        private void OnAntCarryChanged()
        {
            AntCarryChanged?.Invoke();
        }

        private void OnDestroy()
        {
            UnsubscribeFromServices();
        }

        public FoodSource SpawnFood(
            FoodDefinition definition,
            Vector2 position)
        {
            if (!initialized)
                return null;

            return foodFactory.Spawn(definition, position);
        }

        public AntController SpawnAnt(Vector2 position)
        {
            if (!initialized)
            {
                Debug.LogError(
                    "GameplayContext is not initialized."
                );

                return null;
            }

            return antFactory.Spawn(position);
        }
    }
}