using AntGame.Core;
using AntGame.Food;
using AntGame.Characters.Ant;
using AntGame.Characters.Ant.AI.Tasks;
using AntGame.Movement;

namespace AntGame.Characters.Ant.AI.Behaviours
{
    public sealed class ForageBehaviour : BehaviourBase<AntController>
    {
        private readonly AntBrain brain;

        private ForageTask currentTask;
        private bool waitingForFood;
        private bool waitingForNestSpace;

        public ForageBehaviour(
            AntController controller,
            AntBrain brain)
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
            waitingForFood = false;
            waitingForNestSpace = false;

            if (BeginForaging())
                return true;

            UnsubscribeFromControllerEvents();
            currentTask = null;

            return false;
        }

        public override void Stop(TaskBase task)
        {
            UnsubscribeFromControllerEvents();

            controller.CancelMovement();
            controller.CancelCarryOperation();

            waitingForFood = false;
            waitingForNestSpace = false;
            currentTask = null;
        }

        public override bool Suspend(TaskBase task)
        {
            UnsubscribeFromControllerEvents();

            controller.CancelMovement();
            controller.CancelCarryOperation();

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
            SubscribeToControllerEvents();

            if (controller.IsCarryingFood)
            {
                currentTask.SetStage(ForageStage.ReturnToNest);
                waitingForFood = false;

                return controller.MoveToNest();
            }

            return FindFoodOrWait();
        }

        private bool FindFoodOrWait()
        {
            FoodSource food = controller.FindClosestAvailableFood();

            if (food == null)
            {
                currentTask.SetStage(ForageStage.FindFood);
                waitingForFood = true;
                waitingForNestSpace = false;

                return true;
            }

            if (!controller.SetTargetFood(food))
                return false;

            waitingForFood = false;
            currentTask.SetStage(ForageStage.MoveToFood);

            return controller.MoveToFood();
        }

        private bool ContinueForaging()
        {
            SubscribeToControllerEvents();

            switch (currentTask.Stage)
            {
                case ForageStage.FindFood:
                    return FindFoodOrWait();

                case ForageStage.MoveToFood:
                    return controller.TargetFood != null
                        && controller.MoveToFood();

                case ForageStage.CollectFood:
                    return controller.BeginCollectTargetFood();

                case ForageStage.ReturnToNest:
                    return controller.MoveToNest();

                case ForageStage.DepositFood:
                    return BeginDepositOrWait();

                default:
                    return BeginForaging();
            }
        }

        private bool BeginDepositOrWait()
        {
            if (!controller.IsCarryingFood)
                return false;

            if (!controller.NestHasAvailableSpace)
            {
                waitingForNestSpace = true;
                return true;
            }

            waitingForNestSpace = false;

            return controller.BeginDepositFood();
        }

        private void SubscribeToControllerEvents()
        {
            controller.MovementEnded -= OnMovementEnded;
            controller.MovementEnded += OnMovementEnded;

            controller.CarryOperationCompleted -= OnCarryOperationCompleted;
            controller.CarryOperationCompleted += OnCarryOperationCompleted;

            controller.NestStorageChanged -= OnNestStorageChanged;
            controller.NestStorageChanged += OnNestStorageChanged;

            controller.FoodAvailable -= OnFoodAvailable;
            controller.FoodAvailable += OnFoodAvailable;

            controller.TargetFoodUnavailable -= OnTargetFoodUnavailable;
            controller.TargetFoodUnavailable += OnTargetFoodUnavailable;
        }

        private void UnsubscribeFromControllerEvents()
        {
            controller.MovementEnded -= OnMovementEnded;
            controller.CarryOperationCompleted -= OnCarryOperationCompleted;
            controller.NestStorageChanged -= OnNestStorageChanged;
            controller.FoodAvailable -= OnFoodAvailable;
            controller.TargetFoodUnavailable -= OnTargetFoodUnavailable;
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
                    currentTask.SetStage(ForageStage.CollectFood);

                    if (!controller.BeginCollectTargetFood())
                        brain.FailCurrentTask();

                    break;

                case ForageStage.ReturnToNest:
                    currentTask.SetStage(ForageStage.DepositFood);

                    if (!BeginDepositOrWait())
                        brain.FailCurrentTask();

                    break;
            }
        }

        private void OnCarryOperationCompleted(
            CarryOperationType operationType,
            bool success)
        {
            if (currentTask == null)
                return;

            switch (operationType)
            {
                case CarryOperationType.Collection:
                    if (currentTask.Stage != ForageStage.CollectFood)
                        return;

                    if (!success)
                    {
                        if (controller.IsCarryingFood)
                        {
                            currentTask.SetStage(ForageStage.ReturnToNest);

                            if (!controller.MoveToNest())
                                brain.FailCurrentTask();
                        }
                        else
                        {
                            currentTask.SetStage(ForageStage.FindFood);

                            if (!FindFoodOrWait())
                                brain.FailCurrentTask();
                        }

                        return;
                    }

                    currentTask.SetStage(ForageStage.ReturnToNest);

                    if (!controller.MoveToNest())
                        brain.FailCurrentTask();

                    break;

                case CarryOperationType.Deposit:
                    if (currentTask.Stage != ForageStage.DepositFood)
                        return;

                    if (controller.IsCarryingFood)
                    {
                        if (!controller.NestHasAvailableSpace)
                        {
                            waitingForNestSpace = true;
                            return;
                        }

                        if (!BeginDepositOrWait())
                            brain.FailCurrentTask();

                        return;
                    }

                    if (!success)
                    {
                        brain.FailCurrentTask();
                        return;
                    }

                    waitingForNestSpace = false;

                    if (currentTask.Priority == TaskBase.DefaultPriority)
                    {
                        currentTask.SetStage(ForageStage.FindFood);

                        if (!FindFoodOrWait())
                            brain.FailCurrentTask();

                        return;
                    }

                    currentTask.SetStage(ForageStage.Finished);
                    brain.CompleteCurrentTask();
                    break;
            }
        }

        private void OnNestStorageChanged(int storedFood, int maxFood)
        {
            if (!waitingForNestSpace ||
                currentTask == null ||
                currentTask.Stage != ForageStage.DepositFood)
            {
                return;
            }

            if (maxFood - storedFood <= 0)
                return;

            waitingForNestSpace = false;

            if (!BeginDepositOrWait())
                brain.FailCurrentTask();
        }

        private void OnFoodAvailable()
        {
            if (!waitingForFood ||
                currentTask == null ||
                currentTask.Stage != ForageStage.FindFood)
            {
                return;
            }

            waitingForFood = false;

            if (!FindFoodOrWait())
                brain.FailCurrentTask();
        }

        private void OnTargetFoodUnavailable(FoodSource source)
        {
            if (currentTask == null)
                return;

            if (currentTask.Stage != ForageStage.MoveToFood &&
                currentTask.Stage != ForageStage.CollectFood)
            {
                return;
            }

            UnsubscribeFromControllerEvents();

            controller.CancelMovement();
            controller.CancelCarryOperation();

            SubscribeToControllerEvents();

            waitingForFood = false;

            if (controller.IsCarryingFood)
            {
                currentTask.SetStage(ForageStage.ReturnToNest);

                if (!controller.MoveToNest())
                    brain.FailCurrentTask();

                return;
            }

            currentTask.SetStage(ForageStage.FindFood);

            if (!FindFoodOrWait())
                brain.FailCurrentTask();
        }
    }
}