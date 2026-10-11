using UnityEngine;
using AntGame.Core;
using AntGame.Characters.Ant;
using AntGame.Characters.Ant.AI.Tasks;
using AntGame.Food;
using AntGame.Movement;
using System.Collections;
using AntGame.Core.Animation;

namespace AntGame.Characters.Ant.AI.Behaviours
{
    public sealed class PatrolBehaviour : BehaviourBase<AntController>
    {
        private const float PatrolRadius = 1.5f;
        private const int MaxPointAttempts = 12;
        private const float MinPauseDuration = 0.5f;
        private const float MaxPauseDuration = 1.5f;

        private readonly AntBrain brain;

        private Coroutine patrolPauseCoroutine;

        private PatrolTask currentTask;
        private Vector2 clusterCenter;
        private int remainingPoints;
        private int visitedPoints;
        private int pauseAtPoint;

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

            PrepareNewCluster();
            SubscribeToEvents();

            if (TryMoveToNextPoint())
                return true;

            UnsubscribeFromEvents();
            currentTask = null;

            return false;
        }

        public override void Stop(TaskBase task)
        {
            StopPatrolPause();
            UnsubscribeFromEvents();
            controller.CancelMovement();

            currentTask = null;
        }

        public override bool Suspend(TaskBase task)
        {
            StopPatrolPause();
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

            return ContinuePatrol();
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

        private void StopPatrolPause()
        {
            if (patrolPauseCoroutine == null)
                return;

            controller.StopCoroutine(patrolPauseCoroutine);
            patrolPauseCoroutine = null;
        }

        private void PrepareNewCluster()
        {
            clusterCenter = controller.transform.position;

            remainingPoints = Random.Range(3, 5);

            visitedPoints = 0;

            // Случайный номер точки, на которой муравей остановится.
            pauseAtPoint = Random.Range(1, remainingPoints + 1);
        }

        private bool TryMoveToNextPoint()
        {
            for (int i = 0; i < MaxPointAttempts; i++)
            {
                Vector2 direction = Random.insideUnitCircle;

                if (direction.sqrMagnitude < 0.01f)
                    continue;

                float distance = Random.Range(0.5f, PatrolRadius);
                Vector2 point =
                    clusterCenter + direction.normalized * distance;

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
            visitedPoints++;

            // Останавливаемся только на выбранной случайной точке.
            if (visitedPoints == pauseAtPoint)
            {
                if (patrolPauseCoroutine == null)
                {
                    patrolPauseCoroutine = controller.StartCoroutine(
                        WaitAtPatrolPoint()
                    );
                }

                return;
            }

            ContinuePatrol();
        }

        private IEnumerator WaitAtPatrolPoint()
        {
            controller.SetAnimationState(CharacterAnimationState.Idle);

            float pauseDuration = Random.Range(
                MinPauseDuration,
                MaxPauseDuration
            );

            yield return new WaitForSeconds(pauseDuration);

            patrolPauseCoroutine = null;

            if (currentTask == null)
                yield break;

            ContinuePatrol();
        }

        private bool ContinuePatrol()
        {
            if (currentTask == null)
                return false;

            if (remainingPoints > 0)
            {
                if (TryMoveToNextPoint())
                    return true;

                return BeginNewCluster();
            }

            if (TryStartForaging())
                return true;

            return BeginNewCluster();
        }

        private bool BeginNewCluster()
        {
            PrepareNewCluster();

            if (TryMoveToNextPoint())
                return true;

            brain.FailCurrentTask();
            return false;
        }

        private bool TryStartForaging()
        {
            FoodReservation reservation =
                controller.TryReserveBestFood();

            if (reservation == null)
                return false;

            if (reservation.Source == null ||
                !reservation.Source.HasFood)
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