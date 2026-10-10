using System;
using System.Collections;
using UnityEngine;
using AntGame.Food;
using AntGame.Nest;

namespace AntGame.Characters.Ant
{
    public sealed class AntCarryComponent : MonoBehaviour
    {
        [Header("Collection")]
        [SerializeField, Min(0f)]
        private float collectionDuration = 1.5f;

        [SerializeField, Min(1)]
        private int collectionAmount = 1;

        [Header("Deposit")]
        [SerializeField, Min(0f)]
        private float depositDuration = 2f;

        private AntNest nest;
        private Coroutine activeOperation;
        private CarryOperation currentOperation;
        private int carriedAmount;
        private bool initialized;
        private bool isBusy;

        public int CarriedAmount => carriedAmount;
        public bool IsBusy => isBusy;

        public event Action<int> CarriedAmountChanged;
        public event Action<bool> CollectionCompleted;
        public event Action<bool> DepositCompleted;

        public bool Initialize(AntNest assignedNest)
        {
            if (initialized)
                return nest == assignedNest;

            if (assignedNest == null || !assignedNest.IsInitialized)
            {
                Debug.LogError(
                    $"AntCarryComponent on {name} received an invalid AntNest.",
                    this
                );

                return false;
            }

            collectionDuration = Mathf.Max(0f, collectionDuration);
            collectionAmount = Mathf.Max(1, collectionAmount);
            depositDuration = Mathf.Max(0f, depositDuration);

            nest = assignedNest;
            initialized = true;

            return true;
        }

        public bool BeginCollection(FoodSource source)
        {
            Debug.Log(
    $"[Carry] BeginCollection called. " +
    $"Source={(source != null ? source.name : "NULL")}, " +
    $"Initialized={initialized}, Busy={isBusy}, " +
    $"Carried={carriedAmount}",
    this
);
            if (!initialized ||
                isBusy ||
                carriedAmount > 0 ||
                source == null ||
                !source.IsInitialized ||
                !source.HasFood)
            {
                return false;
            }
            Debug.Log(
                $"[Carry] {name}: collection started. " +
                $"Duration={collectionDuration}, " +
                $"Amount={collectionAmount}",
                this
            );
            isBusy = true;
            currentOperation = CarryOperation.Collection;
            activeOperation = StartCoroutine(CollectRoutine(source));

            return true;
        }

        public bool BeginDeposit()
        {
            if (!initialized ||
                isBusy ||
                carriedAmount <= 0 ||
                nest == null)
            {
                return false;
            }

            isBusy = true;
            currentOperation = CarryOperation.Deposit;
            activeOperation = StartCoroutine(DepositRoutine());

            return true;
        }

        public void CancelCurrentOperation()
        {
            if (!isBusy)
                return;

            CarryOperation cancelledOperation = currentOperation;

            if (activeOperation != null)
                StopCoroutine(activeOperation);

            FinishOperation();

            if (cancelledOperation == CarryOperation.Collection)
                CollectionCompleted?.Invoke(false);
            else if (cancelledOperation == CarryOperation.Deposit)
                DepositCompleted?.Invoke(false);
        }

        private IEnumerator CollectRoutine(FoodSource source)
        {
            if (collectionDuration > 0f)
                yield return new WaitForSeconds(collectionDuration);
            else
                yield return null;

            int collectedAmount =
                source != null
                    ? source.TryCollect(collectionAmount)
                    : 0;

            if (collectedAmount > 0)
                carriedAmount += collectedAmount;
            Debug.Log(
                $"[Carry] {name}: collected amount = {collectedAmount}",
                this
            );
            FinishOperation();

            if (collectedAmount > 0)
                CarriedAmountChanged?.Invoke(carriedAmount);

            CollectionCompleted?.Invoke(collectedAmount > 0);
        }

        private IEnumerator DepositRoutine()
        {
            if (depositDuration > 0f)
                yield return new WaitForSeconds(depositDuration);
            else
                yield return null;

            int amountToDeposit = carriedAmount;

            bool success =
                nest != null &&
                nest.DepositFood(amountToDeposit);

            if (success)
                carriedAmount -= amountToDeposit;

            FinishOperation();

            if (success)
                CarriedAmountChanged?.Invoke(carriedAmount);

            DepositCompleted?.Invoke(success);
        }

        private void FinishOperation()
        {
            activeOperation = null;
            currentOperation = CarryOperation.None;
            isBusy = false;
        }

        private enum CarryOperation
        {
            None,
            Collection,
            Deposit
        }
    }
}