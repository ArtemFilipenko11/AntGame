using UnityEngine;
using AntGame.Characters.Ant;

namespace AntGame.Systems
{
    public class AntFactory : IGameplayService
    {
        private readonly AntController antPrefab;
        private readonly Transform spawnParent;
        private readonly AntDependencies dependencies;

        private bool initialized;

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

            AntController ant = Object.Instantiate(
                antPrefab,
                position,
                Quaternion.identity,
                spawnParent
            );

            if (!ant.Initialize(dependencies))
            {
                Object.Destroy(ant.gameObject);
                return null;
            }
            ant.StartForaging();
            return ant;
        }
    }
}