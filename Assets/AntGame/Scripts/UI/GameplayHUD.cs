using AntGame.Systems;
using TMPro;
using UnityEngine;
using AntGame.Characters.Ant;
using AntGame.Food;
using UnityEngine.UI;

namespace AntGame.UI
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField]
        private GameplayContext gameplayContext;

        [SerializeField]
        private TMP_Text nestLabel;

        [SerializeField]
        private TMP_Text foodSourcesText;

        [SerializeField]
        private TMP_Text availableFoodText;

        [SerializeField]
        private TMP_Text antsText;

        [SerializeField]
        private TMP_Text foodCarriedText;
        
        [SerializeField]
        private Image nestProgressFill;

        private void OnEnable()
        {
            if (gameplayContext == null ||
                nestLabel == null ||
                foodSourcesText == null ||
                availableFoodText == null ||
                antsText == null ||
                foodCarriedText == null)
            {
                Debug.LogError(
                    "GameplayHUD: assign GameplayContext and all text fields.",
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

            nestLabel.text = "Муравейник: ...";
            foodSourcesText.text = "Грибы: ...";
            availableFoodText.text = "Еда в мире: ...";
            antsText.text = "Муравьи: ...";
            foodCarriedText.text = "Еда у муравьёв: ...";

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
            if (gameplayContext == null)
                return;

            if (nestProgressFill != null)
            {
                nestProgressFill.fillAmount =
                    gameplayContext.MaxFood > 0
                        ? Mathf.Clamp01(
                            (float)gameplayContext.StoredFood /
                            gameplayContext.MaxFood
                        )
                        : 0f;
            }
            int totalCarriedFood = 0;

            foreach (AntController ant in gameplayContext.Ants)
            {
                if (ant != null)
                    totalCarriedFood += ant.CarriedFood;
            }

            nestLabel.text =
                $"Муравейник: {gameplayContext.StoredFood} / {gameplayContext.MaxFood}";

            foodSourcesText.text =
                $"Грибы: {gameplayContext.FoodSourceCount}";

            availableFoodText.text =
                $"Еда в мире: {gameplayContext.AvailableFoodAmount}";

            antsText.text =
                $"Муравьи: {gameplayContext.Ants.Count}";

            foodCarriedText.text =
                $"Еда у муравьёв: {totalCarriedFood}";
        }

    }
}