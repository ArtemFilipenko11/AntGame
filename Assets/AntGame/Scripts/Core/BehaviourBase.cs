using System;

namespace AntGame.Core
{
    public abstract class BehaviourBase<TController, TBehaviour>
        where TBehaviour : Enum
    {
        protected TController controller;

        public TBehaviour BehaviourType { get; }

        protected BehaviourBase(
            TController controller,
            TBehaviour behaviourType)
        {
            this.controller = controller;
            BehaviourType = behaviourType;
        }

        public virtual bool Initialize()
        {
            return true;
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }
    }
}