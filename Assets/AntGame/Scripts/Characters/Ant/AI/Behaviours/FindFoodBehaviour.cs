using AntGame.Core;
using AntGame.Characters.Ant.AI.Tasks;

namespace AntGame.Characters.Ant.AI.Behaviours
{
    public sealed class FindFoodBehaviour : BehaviourBase<AntController>
    {
        public FindFoodBehaviour(AntController controller)
            : base(controller)
        {
        }

        public override bool CanHandle(TaskBase task)
        {
            return task is FindFoodTask;
        }

        public override bool CanStart(TaskBase task)
        {
            return CanHandle(task)
                && controller.IsInitialized
                && controller.FindClosestAvailableFood() != null;
        }

        public override bool Start(TaskBase task)
        {
            if (!CanStart(task))
                return false;

            var food = controller.FindClosestAvailableFood();

            if (food == null || !controller.SetTargetFood(food))
                return false;

            return controller.MoveToFood();
        }
    }
}
