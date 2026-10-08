using UnityEngine;
using AntGame.Movement;

namespace AntGame.Ant
{
    [RequireComponent(typeof(Movement2D))]
    public class AntController : MonoBehaviour
    {
        private Movement2D movement;
        private AntDependencies dependencies;

        private Food.FoodSource targetFood;

        private int carriedFood;

        private bool initialized;

        public Movement2D Movement => movement;

        public AntDependencies Dependencies => dependencies;

        public Food.FoodSource TargetFood => targetFood;

        public int CarriedFood => carriedFood;

        public bool IsCarryingFood => carriedFood > 0;

        private void Awake()
        {
            movement = GetComponent<Movement2D>();
        }

        public bool Initialize(AntDependencies newDependencies)
        {
            if (initialized)
                return true;

            if (newDependencies == null)
            {
                Debug.LogError(
                    $"AntController on {name} received null dependencies.",
                    this);

                return false;
            }

            if (newDependencies.Nest == null)
            {
                Debug.LogError(
                    $"AntController on {name} received dependencies without an AntNest.",
                    this);

                return false;
            }

            if (newDependencies.FoodRegistry == null)
            {
                Debug.LogError(
                    $"AntController on {name} received dependencies without a FoodRegistry.",
                    this);

                return false;
            }

            dependencies = newDependencies;
            initialized = true;

            return true;
        }

        public void SetTargetFood(Food.FoodSource foodSource)
        {
            targetFood = foodSource;
        }

        public void ClearTargetFood()
        {
            targetFood = null;
        }

        public bool TryCollectTargetFood()
        {
            if (!initialized)
                return false;

            if (targetFood == null)
                return false;

            if (IsCarryingFood)
                return false;

            if (!targetFood.TryCollect())
                return false;

            carriedFood = 1;

            return true;
        }

        public bool TryDepositFood()
        {
            if (!initialized)
                return false;

            if (!IsCarryingFood)
                return false;

            dependencies.Nest.DepositFood(carriedFood);

            carriedFood = 0;

            return true;
        }
    }
}