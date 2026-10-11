using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntGame.Systems
{
    /// <summary>
    /// Общая механика жизненного цикла игровых персонажей:
    /// инициализация, учёт, создание и уничтожение.
    /// Специфические подписки на события персонажей остаются в наследниках.
    /// </summary>
    public abstract class CharacterFactory<TController, TDependencies>
        : IGameplayService
        where TController : MonoBehaviour
        where TDependencies : class
    {
        private readonly TController characterPrefab;
        private readonly Transform spawnParent;
        private readonly TDependencies dependencies;
        private readonly HashSet<TController> characters = new();

        private bool initialized;

        protected IReadOnlyCollection<TController> Characters => characters;

        protected TController CharacterPrefab => characterPrefab;

        protected Transform SpawnParent => spawnParent;

        protected TDependencies Dependencies => dependencies;

        public bool IsInitialized => initialized;

        protected CharacterFactory(
            TController characterPrefab,
            Transform spawnParent,
            TDependencies dependencies)
        {
            this.characterPrefab = characterPrefab;
            this.spawnParent = spawnParent;
            this.dependencies = dependencies;
        }

        public bool Initialize()
        {
            if (initialized)
                return true;

            if (characterPrefab == null)
            {
                Debug.LogError(
                    $"{GetType().Name} has no {typeof(TController).Name} prefab assigned."
                );

                return false;
            }

            if (spawnParent == null)
            {
                Debug.LogError(
                    $"{GetType().Name} has no spawn parent assigned."
                );

                return false;
            }

            if (dependencies == null)
            {
                Debug.LogError(
                    $"{GetType().Name} has no dependencies."
                );

                return false;
            }

            initialized = true;
            InitializeExistingCharacters();

            return true;
        }

        protected bool InitializeExistingCharacters()
        {
            if (!initialized)
            {
                Debug.LogError(
                    $"Cannot initialize existing {typeof(TController).Name} objects " +
                    $"before {GetType().Name} is initialized."
                );

                return false;
            }

            TController[] existingCharacters =
                spawnParent.GetComponentsInChildren<TController>(true);

            foreach (TController character in existingCharacters)
            {
                if (character == null)
                    continue;

                if (!IsCharacterInitialized(character) &&
                    !InitializeCharacter(character, dependencies))
                {
                    Debug.LogError(
                        $"Failed to initialize {typeof(TController).Name} '{character.name}'.",
                        character
                    );

                    continue;
                }

                RegisterCharacter(character);
            }

            return true;
        }

        protected TController SpawnCharacter(Vector2 position)
        {
            if (!initialized)
            {
                Debug.LogError(
                    $"Cannot spawn {typeof(TController).Name} before " +
                    $"{GetType().Name} is initialized."
                );

                return null;
            }

            TController character = UnityEngine.Object.Instantiate(
                characterPrefab,
                position,
                Quaternion.identity,
                spawnParent
            );

            if (!InitializeCharacter(character, dependencies))
            {
                UnityEngine.Object.Destroy(character.gameObject);
                return null;
            }

            RegisterCharacter(character);
            return character;
        }

        protected bool DespawnCharacter(TController character)
        {
            if (!initialized || character == null || !characters.Contains(character))
                return false;

            UnityEngine.Object.Destroy(character.gameObject);
            return true;
        }

        protected bool DespawnRandomCharacter()
        {
            if (!initialized || characters.Count == 0)
                return false;

            List<TController> candidates = new();

            foreach (TController character in characters)
            {
                if (character != null)
                    candidates.Add(character);
            }

            if (candidates.Count == 0)
                return false;

            int index = UnityEngine.Random.Range(0, candidates.Count);
            return DespawnCharacter(candidates[index]);
        }

        private void RegisterCharacter(TController character)
        {
            if (character == null || !characters.Add(character))
                return;

            SubscribeToDestroyed(character, OnCharacterDestroyed);
            OnCharacterRegistered(character);
        }

        private void UnregisterCharacter(TController character)
        {
            // ReferenceEquals нужен, поскольку Unity переопределяет оператор == для Object.
            if (ReferenceEquals(character, null) || !characters.Remove(character))
                return;

            UnsubscribeFromDestroyed(character, OnCharacterDestroyed);
            OnCharacterUnregistered(character);
        }

        private void OnCharacterDestroyed(TController character)
        {
            UnregisterCharacter(character);
        }

        protected abstract bool IsCharacterInitialized(TController character);

        protected abstract bool InitializeCharacter(
            TController character,
            TDependencies characterDependencies);

        protected abstract void SubscribeToDestroyed(
            TController character,
            Action<TController> handler);

        protected abstract void UnsubscribeFromDestroyed(
            TController character,
            Action<TController> handler);

        protected virtual void OnCharacterRegistered(TController character)
        {
        }

        protected virtual void OnCharacterUnregistered(TController character)
        {
        }
    }
}
