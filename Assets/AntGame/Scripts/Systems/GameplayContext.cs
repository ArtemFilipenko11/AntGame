using UnityEngine;

namespace AntGame.Systems
{
    public class GameplayContext : MonoBehaviour
    {
        [Header("World")]
        [SerializeField]
        private Transform worldRoot;

        private Food.FoodRegistry foodRegistry;

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
            foodRegistry =
                new Food.FoodRegistry(worldRoot);

            foodRegistry.Initialize();
        }
    }
}