using AntGame.Core;
using AntGame.Characters.Scout.AI.Tasks;

namespace AntGame.Characters.Scout.AI
{
    public sealed class ScoutBrain : BrainBase<ScoutController>
    {
        public ScoutBrain(ScoutController controller)
            : base(controller)
        {
        }

        protected override TaskBase CreateDefaultTask()
        {
            return new ScoutExploreTask();
        }
    }
}