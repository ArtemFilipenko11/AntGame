using UnityEngine;
using AntGame.Core;
using AntGame.Characters.Ant;
using AntGame.Characters.Ant.AI.Tasks;
using AntGame.Food;
using AntGame.Movement;

namespace AntGame.Characters.Ant.AI.Behaviours
{
    public sealed class PatrolBehaviour : BehaviourBase<AntController>
    {
        private const float PatrolRadius = 1.5f;
        private const int MaxPointAttempts = 12;

        private readonly AntBrain brain;

        private PatrolTask currentTask;
        private Vector2 clusterCenter;
        private int remainingPoints;

        public PatrolBehaviour(
            AntController controller,
            AntBrain brain)
            : base(controller)
        {
            this.brain = brain;
        }

        public override bool CanHandle(TaskBase task)
        {
            return task is PatrolTask;
        }

        public override bool CanStart(TaskBase task)
        {
            return CanHandle(task) && controller.IsInitialized;
        }

        public override bool Start(TaskBase task)
        {
            if (!CanStart(task))
                return false;

            currentTask = (PatrolTask)task;
            clusterCenter = controller.transform.position;
            remainingPoints = Random.Range(3, 5);

            SubscribeToEvents();

            if (TryMoveToNextPoint())
                return true;

            UnsubscribeFromEvents();
            currentTask = null;

            return false;
        }

        public override void Stop(TaskBase task)
        {
            UnsubscribeFromEvents();
            controller.CancelMovement();
            currentTask = null;
        }

        public override bool Suspend(TaskBase task)
        {
            UnsubscribeFromEvents();
            controller.CancelMovement();
            return true;
        }

        public override bool Resume(TaskBase task)
        {
            currentTask = task as PatrolTask;

            if (currentTask == null)
                return false;

            SubscribeToEvents();
            return TryMoveToNextPoint();
        }

        private void SubscribeToEvents()
        {
            controller.MovementEnded -= OnMovementEnded;
            controller.MovementEnded += OnMovementEnded;
        }

        private void UnsubscribeFromEvents()
        {
            controller.MovementEnded -= OnMovementEnded;
        }

        private bool TryMoveToNextPoint()
        {
            for (int i = 0; i < MaxPointAttempts; i++)
            {
                Vector2 direction = Random.insideUnitCircle;

                if (direction.sqrMagnitude < 0.01f)
                    continue;

                float distance = Random.Range(0.5f, PatrolRadius);
                Vector2 point = clusterCenter + direction.normalized * distance;

                if (controller.MoveToPosition(point))
                    return true;
            }

            return false;
        }

        private void OnMovementEnded(MovementEndReason reason)
        {
            if (currentTask == null)
                return;

            if (reason != MovementEndReason.Reached)
            {
                BeginNewCluster();
                return;
            }

            remainingPoints--;

            if (remainingPoints > 0)
            {
                if (!TryMoveToNextPoint())
                    BeginNewCluster();

                return;
            }

            if (TryStartForaging())
                return;

            BeginNewCluster();
        }

        private void BeginNewCluster()
        {
            clusterCenter = controller.transform.position;
            remainingPoints = Random.Range(3, 5);

            if (!TryMoveToNextPoint())
                brain.FailCurrentTask();
        }

        private bool TryStartForaging()
        {
            FoodReservation reservation = controller.TryReserveBestFood();

            if (reservation == null)
                return false;

            if (reservation.Source == null || !reservation.Source.HasFood)
            {
                controller.ReleaseFoodReservation();
                return false;
            }

            if (!brain.CompleteCurrentTask(false))
            {
                controller.ReleaseFoodReservation();
                return false;
            }

            if (!brain.StartForaging(
                    reservation.Source,
                    TaskBase.DefaultPriority))
            {
                controller.ReleaseFoodReservation();
                brain.Reevaluate();
            }

            return true;
        }
    }
}