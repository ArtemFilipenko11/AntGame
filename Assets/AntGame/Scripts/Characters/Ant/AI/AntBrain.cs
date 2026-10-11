using AntGame.Core;
using AntGame.Food;
using AntGame.Characters.Ant.AI.Tasks;

namespace AntGame.Characters.Ant.AI
{
    public sealed class AntBrain : BrainBase<AntController>
    {
        public AntBrain(AntController controller)
            : base(controller)
        {
        }

        protected override TaskBase CreateDefaultTask()
        {
            return new PatrolTask();
        }

        public bool StartForaging(int priority = 0)
        {
            return SubmitTask(new ForageTask(priority));
        }

        public bool StartForaging(
            FoodSource initialFood,
            int priority = 0)
        {
            if (initialFood == null || !initialFood.HasFood)
                return false;

            return SubmitTask(
                new ForageTask(priority, initialFood)
            );
        }
    }
}