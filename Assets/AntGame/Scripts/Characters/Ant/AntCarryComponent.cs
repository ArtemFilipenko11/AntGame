using System;
using System.Collections;
using UnityEngine;
using AntGame.Food;
using AntGame.Nest;
using UnityEngine.Serialization;


namespace AntGame.Characters.Ant
{
    public sealed class AntCarryComponent : MonoBehaviour
    {
        [Header("Collection")]
        [SerializeField, Min(0f), FormerlySerializedAs("collectionDuration")]
        private float collectionDurationMin = 1.5f;

        [SerializeField, Min(0f)]
        private float collectionDurationMax = 2.5f;

        [SerializeField, Min(1)]
        private int collectionAmount = 1;

        [Header("Deposit")]
        [SerializeField, Min(0f), FormerlySerializedAs("depositDuration")]
        private float depositDurationMin = 2f;

        [SerializeField, Min(0f)]
        private float depositDurationMax = 3.5f;

        private AntNest nest;
        private Coroutine activeOperation;
        private int carriedAmount;
        private bool initialized;
        private bool isBusy;
        private CarryOperationType currentOperation;
        
        public int CollectionAmount => collectionAmount;
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

            collectionDurationMin = Mathf.Max(0f, collectionDurationMin);
            collectionDurationMax = Mathf.Max(
                collectionDurationMin,
                collectionDurationMax
            );

            collectionAmount = Mathf.Max(1, collectionAmount);

            depositDurationMin = Mathf.Max(0f, depositDurationMin);
            depositDurationMax = Mathf.Max(
                depositDurationMin,
                depositDurationMax
            );

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
            float duration = UnityEngine.Random.Range(
                collectionDurationMin,
                collectionDurationMax
            );

            activeOperation = StartCoroutine(
                CollectRoutine(source, duration)
            );
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
            float duration = UnityEngine.Random.Range(
                depositDurationMin,
                depositDurationMax
            );

            activeOperation = StartCoroutine(
                DepositRoutine(duration)
            );
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

        private IEnumerator CollectRoutine(
                FoodSource source,
                float duration)
        {
            if (duration > 0f)
                yield return new WaitForSeconds(duration);
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

        private IEnumerator DepositRoutine(float duration)
        {
            if (duration > 0f)
                yield return new WaitForSeconds(duration);
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