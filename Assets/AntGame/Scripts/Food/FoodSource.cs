using UnityEngine;

namespace AntGame.Food
{
    public class FoodSource : MonoBehaviour
    {
        [Header("Food")]
        [SerializeField, Min(1)]
        private int maxFood = 10;

        private int currentFood;

        public int CurrentFood => currentFood;
        public int MaxFood => maxFood;
        public bool HasFood => currentFood > 0;

        private void Awake()
        {
            currentFood = maxFood;
        }

        public bool TryCollect()
        {
            
            if (!HasFood)
                return false;

            currentFood--;

            return true;
        }
    }
}