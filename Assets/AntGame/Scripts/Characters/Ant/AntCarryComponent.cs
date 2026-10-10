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
        private int carriedAmount;
        private bool initialized;
        private bool isBusy;
        private CarryOperationType currentOperation;

        public int CarriedAmount => carriedAmount;
        public bool IsBusy => isBusy;

        public event Action<int> CarriedAmountChanged;
        public event Action<CarryOperationType> OperationStarted;

        public event Action<CarryOperationType, bool> OperationFinished;

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
            if (!initialized ||
                isBusy ||
                carriedAmount > 0 ||
                source == null ||
                !source.IsInitialized ||
                !source.HasFood)
            {
                return false;
            }
           
            isBusy = true;
            currentOperation = CarryOperationType.Collection;
            activeOperation = StartCoroutine(CollectRoutine(source));
            OperationStarted?.Invoke(currentOperation);
            
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
            currentOperation = CarryOperationType.Deposit;
            activeOperation = StartCoroutine(DepositRoutine());
            OperationStarted?.Invoke(currentOperation);
            
            return true;
        }

        public void CancelCurrentOperation()
        {
            if (!isBusy)
                return;

            CarryOperationType cancelledOperation = currentOperation;

            if (activeOperation != null)
                StopCoroutine(activeOperation);

            FinishOperation(cancelledOperation, false);
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

            if (collectedAmount > 0){
                carriedAmount += collectedAmount;
                CarriedAmountChanged?.Invoke(carriedAmount);
            }
                

            FinishOperation(
                CarryOperationType.Collection,
                collectedAmount > 0
            );
        }

        private IEnumerator DepositRoutine()
        {
            if (depositDuration > 0f)
                yield return new WaitForSeconds(depositDuration);
            else
                yield return null;

            int amountToDeposit = carriedAmount;

            int depositedAmount = nest != null
                ? nest.DepositFood(amountToDeposit)
                : 0;

            if (depositedAmount > 0)
            {
                carriedAmount -= depositedAmount;
                CarriedAmountChanged?.Invoke(carriedAmount);
            }

            bool success =
                amountToDeposit > 0 &&
                depositedAmount == amountToDeposit;

            FinishOperation(
                CarryOperationType.Deposit,
                success
            );
        }

        private void FinishOperation(
            CarryOperationType operationType,
            bool success)
        {
            activeOperation = null;
            currentOperation = CarryOperationType.None;
            isBusy = false;

            OperationFinished?.Invoke(operationType, success);
        }

    }
}