using UnityEngine;
using AntGame.Core;
using AntGame.Characters.Ant.AI.Tasks;
using AntGame.Movement;
using AntGame.Characters.Ant;

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
            if (controller.IsCarryingFood)
            {
                currentTask.SetStage(ForageStage.ReturnToNest);

                SubscribeToControllerEvents();

                if (controller.MoveToNest())
                    return true;

                UnsubscribeFromControllerEvents();
                return false;
            }

            var food = controller.FindClosestAvailableFood();

            if (food == null || !controller.SetTargetFood(food))
                return false;

            currentTask.SetStage(ForageStage.MoveToFood);

            SubscribeToControllerEvents();

            if (controller.MoveToFood())
                return true;

            UnsubscribeFromControllerEvents();
            return false;
        }

        private bool ContinueForaging()
        {
            SubscribeToControllerEvents();

            switch (currentTask.Stage)
            {
                case ForageStage.MoveToFood:
                    return controller.TargetFood != null
                        && controller.MoveToFood();

                case ForageStage.CollectFood:
                    return controller.BeginCollectTargetFood();

                case ForageStage.ReturnToNest:
                    return controller.MoveToNest();

                case ForageStage.DepositFood:
                    return controller.BeginDepositFood();

                default:
                    return BeginForaging();
            }
        }

        private void SubscribeToControllerEvents()
        {
            controller.MovementEnded -= OnMovementEnded;
            controller.MovementEnded += OnMovementEnded;

            controller.CarryOperationCompleted -= OnCarryOperationCompleted;
            controller.CarryOperationCompleted += OnCarryOperationCompleted;
        }

        private void UnsubscribeFromControllerEvents()
        {
            controller.MovementEnded -= OnMovementEnded;

            controller.CarryOperationCompleted -= OnCarryOperationCompleted;
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

                    if (!controller.BeginDepositFood())
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

            if (!success)
            {
                brain.FailCurrentTask();
                return;
            }

            switch (operationType)
            {
                case CarryOperationType.Collection:
                    if (currentTask.Stage != ForageStage.CollectFood)
                        return;

                    currentTask.SetStage(ForageStage.ReturnToNest);

                    if (!controller.MoveToNest())
                        brain.FailCurrentTask();

                    break;

                case CarryOperationType.Deposit:
                    if (currentTask.Stage != ForageStage.DepositFood)
                        return;

                    currentTask.SetStage(ForageStage.Finished);
                    brain.CompleteCurrentTask();

                    break;
            }
        }
    }
}