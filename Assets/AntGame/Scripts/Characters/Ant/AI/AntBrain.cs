using AntGame.Core;
using AntGame.Characters.Ant.AI.Tasks;

namespace AntGame.Characters.Ant.AI
{
    public sealed class AntBrain : BrainBase<AntController>
    {
        public AntBrain(AntController controller)
            : base(controller)
        {
        }

        public override bool Initialize()
        {
            if(!base.Initialize())
                return false;
        
            return true;

        }

        public bool StartForaging(int priority = 0)
        {
            return SubmitTask(new ForageTask(priority));
        }

        protected override TaskBase CreateDefaultTask()
        {
            return new ForageTask(TaskBase.DefaultPriority);
        }
    }
}