
namespace AntGame.Core
{
    public abstract class BehaviourBase<TController>
        where TController : ControllerBase<TController>
    {
        protected TController controller;

        protected BehaviourBase(TController controller)
        {
            this.controller = controller;
        }

        public virtual bool Initialize()
        {
            return true;
        }

        public virtual bool CanHandle(TaskBase task)
        {
            return task != null;
        }

        public virtual bool CanStart(TaskBase task)
        {
            return CanHandle(task) && task.CanStart();
        }

        public abstract bool Start(TaskBase task);

        public virtual bool Suspend(TaskBase task)
        {
            return true;
        }

        public virtual bool Resume(TaskBase task)
        {
            return true;
        }

        public virtual void Stop(TaskBase task)
        {
        }
    }
}