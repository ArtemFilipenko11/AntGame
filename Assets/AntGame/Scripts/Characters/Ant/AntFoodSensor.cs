using System;
using AntGame.Food;
using UnityEngine;
using System.Collections.Generic;

namespace AntGame.Characters.Ant
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class AntFoodSensor : MonoBehaviour
    {
        private bool initialized;
        private readonly HashSet<FoodSource> reportedSources = new();

        public event Action<FoodSource> FoodDetected;

        public bool Initialize()
        {
            CircleCollider2D sensorCollider =
                GetComponent<CircleCollider2D>();

            if (sensorCollider == null)
            {
                Debug.LogError(
                    $"{nameof(AntFoodSensor)} requires a CircleCollider2D.",
                    this
                );

                return false;
            }

            if (!sensorCollider.isTrigger)
            {
                Debug.LogError(
                    $"{nameof(AntFoodSensor)} requires a trigger collider.",
                    this
                );

                return false;
            }

            initialized = true;
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDetectFood(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDetectFood(other);
        }

        private void TryDetectFood(Collider2D other)
        {
            if (!initialized || other == null)
                return;

            FoodSource source = other.GetComponentInParent<FoodSource>();

            if (source == null || !source.IsInitialized || !source.HasFood)
                return;

            if (!reportedSources.Add(source))
                return;

            FoodDetected?.Invoke(source);
        }
    }
}