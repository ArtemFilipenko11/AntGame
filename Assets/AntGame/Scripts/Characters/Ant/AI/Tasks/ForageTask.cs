using AntGame.Core;
using AntGame.Food;

namespace AntGame.Characters.Ant.AI.Tasks
{
    public enum ForageStage
    {
        FindFood,
        MoveToFood,
        CollectFood,
        ReturnToNest,
        DepositFood,
        Finished
    }

    public sealed class ForageTask : TaskBase
    {
        private FoodSource initialFood;

        public ForageStage Stage { get; private set; }

        public ForageTask(
            int priority,
            FoodSource initialFood = null)
            : base(priority)
        {
            Stage = ForageStage.FindFood;
            this.initialFood = initialFood;
        }

        public FoodSource TakeInitialFood()
        {
            FoodSource food = initialFood;
            initialFood = null;
            return food;
        }

        public void SetStage(ForageStage stage)
        {
            Stage = stage;
        }
    }
}