using AntGame.Core;

namespace AntGame.Characters.Ant.AI.Tasks
{
    public sealed class PatrolTask : TaskBase
    {
        public PatrolTask()
            : base(TaskBase.DefaultPriority)
        {
        }
    }
}