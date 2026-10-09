using AntGame.Core;
using AntGame.Characters.Ant.AI.Tasks;
using AntGame.Movement;

namespace AntGame.Characters.Ant.AI.Behaviours
{
    public sealed class ForageBehaviour : BehaviourBase<AntController>
    {
        private AntBrain brain;
        private ForageTask currentTask;

        public ForageBehaviour(AntController controller, AntBrain brain)
            : base(controller)
        {
            this.brain = brain;
        }

        public override bool CanHandle(TaskBase task)
        {
            return task is ForageTask;
        }

        public override bool CanStart(TaskBase task)
        {
            return CanHandle(task) && controller.IsInitialized;
        }

        public override bool Start(TaskBase task)
        {
            if (!CanStart(task))
                return false;

            currentTask = (ForageTask)task;

            return BeginForaging();
        }

        public override void Stop(TaskBase task)
        {
            controller.MovementEnded -= OnMovementEnded;
            controller.CancelMovement();
            currentTask = null;
        }

        public override bool Suspend(TaskBase task)
        {
            controller.MovementEnded -= OnMovementEnded;
            controller.CancelMovement();

            return true;
        }

        public override bool Resume(TaskBase task)
        {
            currentTask = task as ForageTask;

            if (currentTask == null)
                return false;

            return ContinueForaging();
        }

        private bool BeginForaging()
        {
            Food.FoodSource food = controller.FindClosestAvailableFood();

            if (food == null || !controller.SetTargetFood(food))
                return false;

            currentTask.SetStage(ForageStage.MoveToFood);

            controller.MovementEnded -= OnMovementEnded;
            controller.MovementEnded += OnMovementEnded;

            if (controller.MoveToFood())
                return true;

            controller.MovementEnded -= OnMovementEnded;
            return false;
        }

        private bool ContinueForaging()
        {
            controller.MovementEnded -= OnMovementEnded;
            controller.MovementEnded += OnMovementEnded;

            switch (currentTask.Stage)
            {
                case ForageStage.MoveToFood:
                    return controller.TargetFood != null
                        && controller.MoveToFood();

                case ForageStage.ReturnToNest:
                    return controller.MoveToNest();

                default:
                    return BeginForaging();
            }
        }

        private void OnMovementEnded(MovementEndReason reason)
        {
            if (currentTask == null)
                return;

            if (reason != MovementEndReason.Reached)
            {
                brain.FailCurrentTask();
                return;
            }

            switch (currentTask.Stage)
            {
                case ForageStage.MoveToFood:
                    if (!controller.TryCollectTargetFood())
                    {
                        brain.FailCurrentTask();
                        return;
                    }

                    currentTask.SetStage(ForageStage.ReturnToNest);

                    if (!controller.MoveToNest())
                        brain.FailCurrentTask();

                    break;

                case ForageStage.ReturnToNest:
                    currentTask.SetStage(ForageStage.DepositFood);

                    if (!controller.TryDepositFood())
                    {
                        brain.FailCurrentTask();
                        return;
                    }

                    currentTask.SetStage(ForageStage.Finished);
                    brain.CompleteCurrentTask();
                    break;
            }
        }
    }
}