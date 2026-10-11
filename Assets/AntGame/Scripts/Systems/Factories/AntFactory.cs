using System;
using System.Collections.Generic;
using UnityEngine;
using AntGame.Characters.Ant;
using AntGame.Food;

namespace AntGame.Systems
{
    public sealed class AntFactory
        : CharacterFactory<AntController, AntDependencies>
    {
        public IReadOnlyCollection<AntController> Ants => Characters;

        public event Action<AntController> AntRegistered;
        public event Action<AntController> AntUnregistered;
        public event Action AntCarryChanged;
        public event Action<FoodSource> FoodSourceDetected;

        public AntFactory(
            AntController antPrefab,
            Transform spawnParent,
            AntDependencies dependencies)
            : base(antPrefab, spawnParent, dependencies)
        {
        }

        public bool InitializeExistingAnts()
        {
            return InitializeExistingCharacters();
        }

        public AntController Spawn(Vector2 position)
        {
            return SpawnCharacter(position);
        }

        public bool Despawn(AntController ant)
        {
            return DespawnCharacter(ant);
        }

        public bool DespawnRandomAnt()
        {
            return DespawnRandomCharacter();
        }

        protected override bool IsCharacterInitialized(AntController ant)
        {
            return ant.IsInitialized;
        }

        protected override bool InitializeCharacter(
            AntController ant,
            AntDependencies dependencies)
        {
            return ant.Initialize(dependencies);
        }

        protected override void SubscribeToDestroyed(
            AntController ant,
            Action<AntController> handler)
        {
            ant.Destroyed += handler;
        }

        protected override void UnsubscribeFromDestroyed(
            AntController ant,
            Action<AntController> handler)
        {
            ant.Destroyed -= handler;
        }

        protected override void OnCharacterRegistered(AntController ant)
        {
            ant.CarriedFoodChanged += OnAntCarriedFoodChanged;
            ant.FoodSourceDetected += OnAntFoodSourceDetected;

            AntRegistered?.Invoke(ant);
        }

        protected override void OnCharacterUnregistered(AntController ant)
        {
            ant.CarriedFoodChanged -= OnAntCarriedFoodChanged;
            ant.FoodSourceDetected -= OnAntFoodSourceDetected;

            AntUnregistered?.Invoke(ant);
        }

        private void OnAntCarriedFoodChanged(int _)
        {
            AntCarryChanged?.Invoke();
        }

        private void OnAntFoodSourceDetected(FoodSource source)
        {
            if (source == null)
                return;

            FoodSourceDetected?.Invoke(source);
        }
    }
}
