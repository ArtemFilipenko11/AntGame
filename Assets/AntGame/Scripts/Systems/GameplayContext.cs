using AntGame.Ant;
using AntGame.Food;
using AntGame.Nest;
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

        private FoodRegistry foodRegistry;
        private FoodSpawner foodSpawner;
        private AntFactory antFactory;

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

            antNest.Initialize();

            AntDependencies antDependencies =
                new AntDependencies(
                    antNest,
                    foodRegistry
                );

            antFactory = new AntFactory(
                antPrefab,
                antRoot,
                antDependencies
            );

            antFactory.Initialize();
        }

        public FoodSource SpawnFood(
            FoodDefinition definition,
            Vector2 position)
        {
            if (!initialized)
                return null;

            return foodSpawner.Spawn(
                definition,
                position
            );
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