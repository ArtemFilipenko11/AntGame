
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntGame.Core
{
    public abstract class ControllerBase<TController> : MonoBehaviour
        where TController : ControllerBase<TController>
    {
        [NonSerialized]
        protected bool initialized;

        private readonly Dictionary<Type, BehaviourBase<TController>> behaviours = new();

        private BehaviourBase<TController> activeBehaviour;
        private TaskBase activeTask;

        public bool IsInitialized => initialized;

        public bool HasActiveTask => activeTask != null;

        protected bool RegisterBehaviour<TTask>(
            BehaviourBase<TController> behaviour)
            where TTask : TaskBase
        {
            if (initialized || behaviour == null)
                return false;

            Type taskType = typeof(TTask);

            if (behaviours.ContainsKey(taskType))
            {
                Debug.LogError(
                    $"Behaviour for task type {taskType.Name} is already registered on {name}.",
                    this);

                return false;
            }

            behaviours.Add(taskType, behaviour);
            return true;
        }

        protected bool InitializeBehaviours()
        {
            if (initialized)
                return false;

            foreach (BehaviourBase<TController> behaviour in behaviours.Values)
            {
                if (behaviour.Initialize())
                    continue;

                Debug.LogError(
                    $"Failed to initialize a behaviour on {name}.",
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

        public bool CanRunTask(TaskBase task)
        {
            if (!initialized || task == null)
                return false;

            if (!behaviours.TryGetValue(
                task.GetType(),
                out BehaviourBase<TController> behaviour))
            {
                return false;
            }

            return behaviour.CanStart(task);
        }
        
        public bool CanResumeTask(TaskBase task)
        {
            if (!initialized || HasActiveTask || task == null)
                return false;

            if (!behaviours.TryGetValue(
                task.GetType(),
                out BehaviourBase<TController> behaviour))
            {
                return false;
            }

            return behaviour.CanHandle(task) && task.CanResume();
        }

        public bool StartTask(TaskBase task)
        {
            if (HasActiveTask || !CanRunTask(task))
                return false;

            BehaviourBase<TController> behaviour = behaviours[task.GetType()];

            if (!behaviour.Start(task))
                return false;

            activeBehaviour = behaviour;
            activeTask = task;

            return true;
        }

        public bool SuspendCurrentTask()
        {
            if (!HasActiveTask)
                return false;

            if (!activeBehaviour.Suspend(activeTask))
                return false;

            activeBehaviour = null;
            activeTask = null;

            return true;
        }

        public bool ResumeTask(TaskBase task)
        {
            if (HasActiveTask || !initialized || task == null)
                return false;

            if (!behaviours.TryGetValue(
                task.GetType(),
                out BehaviourBase<TController> behaviour))
            {
                return false;
            }

            if (!behaviour.CanHandle(task) || !task.CanResume())
                return false;

            if (!behaviour.Resume(task))
                return false;

            activeBehaviour = behaviour;
            activeTask = task;

            return true;
        }

        public bool StopCurrentTask()
        {
            if (!HasActiveTask)
                return false;

            activeBehaviour.Stop(activeTask);

            activeBehaviour = null;
            activeTask = null;

            return true;
        }

        protected bool TryGetBehaviour<TTask>(
            out BehaviourBase<TController> behaviour)
            where TTask : TaskBase
        {
            return behaviours.TryGetValue(
                typeof(TTask),
                out behaviour);
        }
    }
}