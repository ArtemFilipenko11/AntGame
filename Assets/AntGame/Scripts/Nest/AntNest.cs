using System;
using UnityEngine;

namespace AntGame.Nest
{
    public class AntNest : MonoBehaviour
    {
        [Header("Storage")]
        [SerializeField, Min(0)]
        private int storedFood = 0;

        public int StoredFood => storedFood;

        public event Action<int> FoodChanged;

        public void DepositFood(int amount)
        {
            if (amount <= 0)
                return;

            storedFood += amount;
            FoodChanged?.Invoke(storedFood);
        }
    }
}