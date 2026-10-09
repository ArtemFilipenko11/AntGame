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

        public bool StartForaging(int priority = 0)
        {
            return SubmitTask(new ForageTask(priority));
        }
    }
}