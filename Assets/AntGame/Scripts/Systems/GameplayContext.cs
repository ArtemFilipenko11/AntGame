using UnityEngine;

namespace AntGame.Systems
{
    public class GameplayContext : MonoBehaviour
    {
        [SerializeField]
        private GameplaySystem[] systems;

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

            foreach (GameplaySystem system in systems)
            {
                if (system == null)
                    continue;

                system.Initialize();
            }
        }
    }
}