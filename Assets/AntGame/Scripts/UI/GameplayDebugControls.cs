using AntGame.Systems;
using UnityEngine;

namespace AntGame.UI
{
    public sealed class GameplayDebugControls : MonoBehaviour
    {
        [SerializeField]
        private GameplayContext gameplayContext;

        public void SpawnAntAtOrigin()
        {
            gameplayContext?.SpawnAnt(Vector2.zero);
        }

        public void SpawnRandomFood()
        {
            gameplayContext?.SpawnRandomFood();
        }

        public void DespawnRandomAnt()
        {
            gameplayContext?.DespawnRandomAnt();
        }

        public void DespawnRandomFood()
        {
            gameplayContext?.DespawnRandomFood();
        }

        public void SpawnScoutAtOrigin()
        {
            gameplayContext?.SpawnScout(Vector2.zero);
        }

        public void DespawnRandomScout()
        {
            gameplayContext?.DespawnRandomScout();
        }
    }
}
