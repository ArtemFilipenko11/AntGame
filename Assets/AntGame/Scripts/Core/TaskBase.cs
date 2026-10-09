
using System;

namespace AntGame.Core
{
    public abstract class TaskBase
    {
        public int Priority { get; }

        protected TaskBase(int priority)
        {
            Priority = priority;
        }

        public virtual bool CanStart()
        {
            return true;
        }

        public virtual bool CanResume()
        {
            return CanStart();
        }

        public virtual bool CanBeInterrupted()
        {
            return true;
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnSuspend()
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual void OnComplete()
        {
        }

        public virtual void OnFail()
        {
        }

        public virtual void OnCancel()
        {
        }
    }
}