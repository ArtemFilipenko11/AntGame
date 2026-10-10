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

        [Header("Navigation Debug")]
        [SerializeField]
        private bool drawNavigationGrid = true;

        [Header("FoodSpawn Debug")]
        [SerializeField, Min(0f)]
        private float foodNestExclusionRadius = 2f;

        [SerializeField, Min(1)]
        private int randomFoodSpawnAttempts = 30;

        [SerializeField]
        private FoodDefinition[] foodDefinitions;

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

        public FoodSource SpawnRandomFood()
        {
            if (!initialized)
                return null;

            if (foodPrefab == null)
            {
                Debug.LogError(
                    "GameplayContext has no FoodSource prefab.",
                    this
                );

                return null;
            }

            if (foodDefinitions == null || foodDefinitions.Length == 0)
            {
                Debug.LogError(
                    "GameplayContext has no FoodDefinitions assigned.",
                    this
                );

                return null;
            }

            FoodDefinition definition = null;

            int startIndex = UnityEngine.Random.Range(
                0,
                foodDefinitions.Length
            );

            for (int i = 0; i < foodDefinitions.Length; i++)
            {
                int index = (startIndex + i) % foodDefinitions.Length;

                if (foodDefinitions[index] != null)
                {
                    definition = foodDefinitions[index];
                    break;
                }
            }

            if (definition == null)
            {
                Debug.LogError(
                    "GameplayContext has no valid FoodDefinition.",
                    this
                );

                return null;
            }

            float halfWidth = Mathf.Abs(navigationSize.x) * 0.5f;
            float halfHeight = Mathf.Abs(navigationSize.y) * 0.5f;

            Vector2 nestPosition = antNest.transform.position;

            for (int i = 0; i < randomFoodSpawnAttempts; i++)
            {
                Vector2 position = new Vector2(
                    UnityEngine.Random.Range(
                        navigationCenter.x - halfWidth,
                        navigationCenter.x + halfWidth
                    ),
                    UnityEngine.Random.Range(
                        navigationCenter.y - halfHeight,
                        navigationCenter.y + halfHeight
                    )
                );

                if (Vector2.Distance(position, nestPosition)
                    < foodNestExclusionRadius)
                {
                    continue;
                }

                return foodFactory.Spawn(
                    definition,
                    position
                );
            }

            Debug.LogWarning(
                "Could not find a valid random food spawn position.",
                this
            );

            return null;
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

        public bool DespawnAnt(AntController ant)
        {
            return initialized && antFactory.Despawn(ant);
        }

        public bool DespawnFood(FoodSource source)
        {
            return initialized && foodFactory.Despawn(source);
        }

        public bool DespawnRandomAnt()
        {
            return initialized && antFactory.DespawnRandomAnt();
        }

        public bool DespawnRandomFood()
        {
            return initialized && foodFactory.DespawnRandomFood();
        }

        private void OnDrawGizmos()
        {
            if (!drawNavigationGrid)
                return;

            if (navigationCellSize <= 0f ||
                navigationSize.x <= 0f ||
                navigationSize.y <= 0f)
            {
                return;
            }

            Vector2 origin = navigationCenter - navigationSize * 0.5f;

            int width = Mathf.CeilToInt(
                navigationSize.x / navigationCellSize
            );

            int height = Mathf.CeilToInt(
                navigationSize.y / navigationCellSize
            );

            float gridWidth = width * navigationCellSize;
            float gridHeight = height * navigationCellSize;

            float z = transform.position.z;

            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.4f);

            for (int x = 0; x <= width; x++)
            {
                float xPosition = origin.x + x * navigationCellSize;

                Gizmos.DrawLine(
                    new Vector3(xPosition, origin.y, z),
                    new Vector3(xPosition, origin.y + gridHeight, z)
                );
            }

            for (int y = 0; y <= height; y++)
            {
                float yPosition = origin.y + y * navigationCellSize;

                Gizmos.DrawLine(
                    new Vector3(origin.x, yPosition, z),
                    new Vector3(origin.x + gridWidth, yPosition, z)
                );
            }

            Gizmos.color = Color.yellow;

            Vector3 bottomLeft = new Vector3(origin.x, origin.y, z);
            Vector3 bottomRight = new Vector3(origin.x + gridWidth, origin.y, z);
            Vector3 topLeft = new Vector3(origin.x, origin.y + gridHeight, z);
            Vector3 topRight = new Vector3(
                origin.x + gridWidth,
                origin.y + gridHeight,
                z
            );

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
    }
}