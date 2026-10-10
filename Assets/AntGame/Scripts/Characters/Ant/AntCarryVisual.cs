using UnityEngine;

namespace AntGame.Characters.Ant
{
    public sealed class AntCarryVisual : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField]
        private GameObject carriedFoodVisual;

        private AntController controller;

        public bool IsConfigured => carriedFoodVisual != null;

        private void Awake()
        {
            if (carriedFoodVisual != null)
                carriedFoodVisual.SetActive(false);
        }

        public bool Initialize(AntController newController)
        {
            if (newController == null || !IsConfigured)
                return false;

            if (controller != newController)
            {
                if (controller != null)
                    controller.CarriedFoodChanged -= OnCarriedFoodChanged;

                controller = newController;
                controller.CarriedFoodChanged += OnCarriedFoodChanged;
            }

            OnCarriedFoodChanged(controller.CarriedFood);

            return true;
        }

        private void OnCarriedFoodChanged(int amount)
        {
            if (carriedFoodVisual != null)
                carriedFoodVisual.SetActive(amount > 0);
        }

        private void OnDestroy()
        {
            if (controller != null)
                controller.CarriedFoodChanged -= OnCarriedFoodChanged;
        }
    }
}