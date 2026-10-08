using UnityEngine;

namespace AntGame.Nest
{
    public class AntNest : MonoBehaviour
    {
        [Header("Storage")]
        [SerializeField, Min(0)]
        private int storedFood;

        [SerializeField, Min(1)]
        private int maxFood = 100;

        private bool initialized;

        public int StoredFood => storedFood;
        public int MaxFood => maxFood;
        public bool IsFull => storedFood >= maxFood;
        public bool IsInitialized => initialized;

        public bool Initialize()
        {
            if (initialized)
                return true;

            storedFood = Mathf.Clamp(
                storedFood,
                0,
                maxFood
            );

            initialized = true;

            return true;
        }

        public bool DepositFood(int amount)
        {
            if (!initialized)
                return false;

            if (amount <= 0)
                return false;

            if (IsFull)
                return false;

            storedFood = Mathf.Min(
                storedFood + amount,
                maxFood
            );

            return true;
        }
    }
}