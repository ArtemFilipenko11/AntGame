
using AntGame.Core;
using AntGame.Characters.Ant;

namespace AntGame.Characters.Ant.AI.Behaviours
{
    public class IdleBehaviour : BehaviourBase<AntController>
    {
        public IdleBehaviour(AntController controller)
            : base(controller)
        {
        }

        public override bool Start(TaskBase task)
        {
            return task != null;
        }
    }
}