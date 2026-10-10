using UnityEngine;
using System;

namespace AntGame.Nest
{
    public class AntNest : MonoBehaviour
    {
        [SerializeField, HideInInspector, Min(0)]
        private int storedFood;

        [SerializeField, HideInInspector, Min(1)]
        private int maxFood = 100;

        private bool initialized;

        public int StoredFood => storedFood;
        public int MaxFood => maxFood;
        public bool IsFull => storedFood >= maxFood;
        public bool IsInitialized => initialized;

        public event Action<int, int> FoodStorageChanged;

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
            if (!initialized || amount <= 0)
                return false;

            if (amount > maxFood - storedFood)
                return false;

            storedFood += amount;

            FoodStorageChanged?.Invoke(storedFood, maxFood);

            return true;
        }

        public bool SetMaxFood(int value)
        {
            if (!initialized)
                return false;

            maxFood = Mathf.Max(1, value);
            storedFood = Mathf.Min(storedFood, maxFood);

            FoodStorageChanged?.Invoke(storedFood, maxFood);

            return true;
        }

        public bool SetStoredFood(int value)
        {
            if (!initialized)
                return false;

            storedFood = Mathf.Clamp(value, 0, maxFood);

            FoodStorageChanged?.Invoke(storedFood, maxFood);

            return true;
        }
    }
}