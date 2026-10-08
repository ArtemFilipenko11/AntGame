
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntGame.Core
{
    public abstract class ControllerBase<TController, TBehaviour> : MonoBehaviour
        where TController : ControllerBase<TController, TBehaviour>
        where TBehaviour : Enum
    {
        [NonSerialized]
        protected bool initialized;

        private readonly Dictionary<TBehaviour, BehaviourBase<TController, TBehaviour>> behaviours = new();

        private BehaviourBase<TController, TBehaviour> activeBehaviour;

        public bool IsInitialized => initialized;

        public TBehaviour CurrentBehaviour { get; private set; }

        public bool HasActiveBehaviour { get; private set; }

        public event Action<TBehaviour> BehaviourChanged;

        protected bool RegisterBehaviour(
            TBehaviour behaviourType,
            BehaviourBase<TController, TBehaviour> behaviour)
        {
            if (initialized)
                return false;

            if (behaviour == null)
                return false;

            if (!EqualityComparer<TBehaviour>.Default.Equals(
                behaviourType,
                behaviour.BehaviourType))
            {
                Debug.LogError(
                    $"Behaviour type mismatch on {name}.",
                    this);

                return false;
            }

            return behaviours.TryAdd(behaviourType, behaviour);
        }

        protected bool InitializeBehaviours()
        {
            if (initialized)
                return false;

            foreach (BehaviourBase<TController, TBehaviour> behaviour in behaviours.Values)
            {
                if (behaviour.Initialize())
                    continue;

                Debug.LogError(
                    $"Failed to initialize behaviour {behaviour.BehaviourType} on {name}.",
                    this);

                return false;
            }

            return true;
        }

        protected bool InitializeBase()
        {
            if (initialized)
                return false;

            initialized = true;
            return true;
        }

        protected bool ChangeBehaviour(TBehaviour newBehaviour)
        {
            if (!initialized)
                return false;

            if (!behaviours.TryGetValue(newBehaviour, out BehaviourBase<TController, TBehaviour> nextBehaviour))
                return false;

            if (ReferenceEquals(activeBehaviour, nextBehaviour))
                return false;

            activeBehaviour?.Exit();

            activeBehaviour = nextBehaviour;
            CurrentBehaviour = newBehaviour;
            HasActiveBehaviour = true;

            activeBehaviour.Enter();

            BehaviourChanged?.Invoke(CurrentBehaviour);

            return true;
        }

        protected bool TryGetBehaviour(
            TBehaviour behaviourType,
            out BehaviourBase<TController, TBehaviour> behaviour)
        {
            return behaviours.TryGetValue(
                behaviourType,
                out behaviour);
        }
    }
}