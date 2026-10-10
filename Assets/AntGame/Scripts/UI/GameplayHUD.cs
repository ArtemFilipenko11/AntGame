using AntGame.Systems;
using TMPro;
using UnityEngine;
using AntGame.Characters.Ant;
using AntGame.Food;

namespace AntGame.UI
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField]
        private GameplayContext gameplayContext;

        [SerializeField]
        private TMP_Text hudText;

        private void OnEnable()
        {
            if (gameplayContext == null || hudText == null)
            {
                Debug.LogError(
                    "GameplayHUD requires GameplayContext and TMP_Text.",
                    this
                );

                return;
            }

            gameplayContext.Initialized += Refresh;
            gameplayContext.NestStorageChanged += OnContextChanged;

            gameplayContext.InventoryChanged += Refresh;
            gameplayContext.FoodRegistered += OnFoodChanged;
            gameplayContext.FoodUnregistered += OnFoodChanged;

            gameplayContext.AntRegistered += OnAntChanged;
            gameplayContext.AntUnregistered += OnAntChanged;
            gameplayContext.AntCarryChanged += Refresh;

            hudText.text = "Initializing...";

            if (gameplayContext.IsInitialized)
                Refresh();
        }

        private void OnDisable()
        {
            if (gameplayContext == null)
                return;

            gameplayContext.Initialized -= Refresh;
            gameplayContext.NestStorageChanged -= OnContextChanged;

            gameplayContext.InventoryChanged -= Refresh;
            gameplayContext.FoodRegistered -= OnFoodChanged;
            gameplayContext.FoodUnregistered -= OnFoodChanged;

            gameplayContext.AntRegistered -= OnAntChanged;
            gameplayContext.AntUnregistered -= OnAntChanged;
            gameplayContext.AntCarryChanged -= Refresh;
        }

        private void OnContextChanged(int storedFood, int maxFood)
        {
            Refresh();
        }

        private void OnFoodChanged(FoodSource source)
        {
            Refresh();
        }

        private void OnAntChanged(AntController ant)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (gameplayContext == null || hudText == null)
                return;

            int totalCarriedFood = 0;

            foreach (AntController ant in gameplayContext.Ants)
            {
                if (ant != null)
                    totalCarriedFood += ant.CarriedFood;
            }

            hudText.text =
                $"Anthill: {gameplayContext.StoredFood} / {gameplayContext.MaxFood}\n" +
                $"Food sources: {gameplayContext.FoodSourceCount}\n" +
                $"Available food: {gameplayContext.AvailableFoodAmount}\n" +
                $"Ants: {gameplayContext.Ants.Count}\n" +
                $"Food carried: {totalCarriedFood}";
        }

    }
}