using System;
using System.Collections.Generic;
using UnityEngine;
using AntGame.Characters.Scout;
using AntGame.Food;

namespace AntGame.Systems
{
    public sealed class ScoutFactory
        : CharacterFactory<ScoutController, ScoutDependencies>
    {
        public IReadOnlyCollection<ScoutController> Scouts => Characters;

        public event Action<ScoutController> ScoutRegistered;
        public event Action<ScoutController> ScoutUnregistered;
        public event Action<FoodSource> FoodSourceDetected;

        public ScoutFactory(
            ScoutController scoutPrefab,
            Transform spawnParent,
            ScoutDependencies dependencies)
            : base(scoutPrefab, spawnParent, dependencies)
        {
        }

        public bool InitializeExistingScouts()
        {
            return InitializeExistingCharacters();
        }

        public ScoutController Spawn(Vector2 position)
        {
            return SpawnCharacter(position);
        }

        public bool Despawn(ScoutController scout)
        {
            return DespawnCharacter(scout);
        }

        public bool DespawnRandomScout()
        {
            return DespawnRandomCharacter();
        }

        protected override bool IsCharacterInitialized(ScoutController scout)
        {
            return scout.IsInitialized;
        }

        protected override bool InitializeCharacter(
            ScoutController scout,
            ScoutDependencies dependencies)
        {
            return scout.Initialize(dependencies);
        }

        protected override void SubscribeToDestroyed(
            ScoutController scout,
            Action<ScoutController> handler)
        {
            scout.Destroyed += handler;
        }

        protected override void UnsubscribeFromDestroyed(
            ScoutController scout,
            Action<ScoutController> handler)
        {
            scout.Destroyed -= handler;
        }

        protected override void OnCharacterRegistered(ScoutController scout)
        {
            scout.FoodSourceDetected += OnScoutFoodSourceDetected;
            ScoutRegistered?.Invoke(scout);
        }

        protected override void OnCharacterUnregistered(ScoutController scout)
        {
            scout.FoodSourceDetected -= OnScoutFoodSourceDetected;
            ScoutUnregistered?.Invoke(scout);
        }

        private void OnScoutFoodSourceDetected(FoodSource source)
        {
            if (source == null)
                return;

            FoodSourceDetected?.Invoke(source);
        }
    }
}
