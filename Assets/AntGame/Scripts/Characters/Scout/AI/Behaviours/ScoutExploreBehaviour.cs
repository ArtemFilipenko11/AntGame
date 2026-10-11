using System.Collections.Generic;
using UnityEngine;
using AntGame.Movement;
using AntGame.Core;
using AntGame.Characters.Scout;
using AntGame.Characters.Scout.AI.Tasks;

namespace AntGame.Characters.Scout.AI.Behaviours
{
    public sealed class ScoutExploreBehaviour
        : BehaviourBase<ScoutController>
    {
        private const float MinimumTargetDistance = 0.05f;
        private const float FrontierLookAheadSteps = 2.25f;
        private const float VisitPenalty = 4.5f;
        private const float FailedTargetPenalty = 9f;
        private const float DistanceWeight = 0.55f;
        private const float DirectionWeight = 0.45f;
        private const float RandomScoreRange = 1.5f;

        private sealed class Candidate
        {
            public Vector2Int Cell { get; }
            public Vector2 Position { get; }
            public float Score { get; }

            public Candidate(Vector2Int cell, Vector2 position, float score)
            {
                Cell = cell;
                Position = position;
                Score = score;
            }
        }

        private ScoutExploreTask currentTask;

        private int[,] visitCounts;
        private int[,] failedAttempts;

        private int columns;
        private int rows;
        private float cellWidth;
        private float cellHeight;
        private Vector2 fieldMin;
        private Vector2 fieldMax;
        private Vector2 explorationOrigin;
        private float explorationStep;
        private float maximumFieldRadius;
        private float maximumReachedRadius;

        private bool gridInitialized;
        private Vector2Int currentTargetCell;
        private Vector2 currentTargetPosition;
        private Vector2 lastMoveDirection;
        private bool hasCurrentTarget;

        public ScoutExploreBehaviour(ScoutController controller)
            : base(controller)
        {
        }

        public override bool CanHandle(TaskBase task)
        {
            return task is ScoutExploreTask;
        }

        public override bool CanStart(TaskBase task)
        {
            return CanHandle(task) && controller.IsInitialized;
        }

        public override bool Start(TaskBase task)
        {
            if (!CanStart(task))
                return false;

            currentTask = (ScoutExploreTask)task;

            if (!gridInitialized && !InitializeExplorationGrid())
            {
                currentTask = null;
                return false;
            }

            SubscribeToEvents();

            if (TryMoveToNextTarget())
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
            hasCurrentTarget = false;
        }

        public override bool Suspend(TaskBase task)
        {
            UnsubscribeFromEvents();
            controller.CancelMovement();
            hasCurrentTarget = false;
            return true;
        }

        public override bool Resume(TaskBase task)
        {
            currentTask = task as ScoutExploreTask;

            if (currentTask == null)
                return false;

            if (!gridInitialized && !InitializeExplorationGrid())
                return false;

            SubscribeToEvents();

            if (TryMoveToNextTarget())
                return true;

            UnsubscribeFromEvents();
            currentTask = null;
            return false;
        }

        private bool InitializeExplorationGrid()
        {
            Vector2 center = controller.ExplorationCenter;
            Vector2 size = controller.ExplorationSize;
            explorationStep = controller.ExplorationStep;

            if (size.x <= 0f || size.y <= 0f || explorationStep <= 0f)
                return false;

            fieldMin = center - size * 0.5f;
            fieldMax = center + size * 0.5f;

            columns = Mathf.Max(1, Mathf.CeilToInt(size.x / explorationStep));
            rows = Mathf.Max(1, Mathf.CeilToInt(size.y / explorationStep));
            cellWidth = size.x / columns;
            cellHeight = size.y / rows;

            visitCounts = new int[columns, rows];
            failedAttempts = new int[columns, rows];

            // Keep the exploration origin inside the playable field.
            explorationOrigin = new Vector2(
                Mathf.Clamp(controller.ExplorationOrigin.x, fieldMin.x, fieldMax.x),
                Mathf.Clamp(controller.ExplorationOrigin.y, fieldMin.y, fieldMax.y)
            );

            maximumFieldRadius = Mathf.Max(
                Vector2.Distance(explorationOrigin, fieldMin),
                Vector2.Distance(explorationOrigin, new Vector2(fieldMax.x, fieldMin.y)),
                Vector2.Distance(explorationOrigin, new Vector2(fieldMin.x, fieldMax.y)),
                Vector2.Distance(explorationOrigin, fieldMax)
            );

            // Begin near the anthill even if the scout was spawned elsewhere.
            maximumReachedRadius = 0f;
            lastMoveDirection = Vector2.zero;
            hasCurrentTarget = false;

            Vector2Int startingCell;
            if (TryGetCell(controller.Position, out startingCell))
                visitCounts[startingCell.x, startingCell.y]++;

            gridInitialized = true;
            return true;
        }

        private bool TryMoveToNextTarget()
        {
            if (!gridInitialized || visitCounts == null || failedAttempts == null)
                return false;

            float step = Mathf.Max(explorationStep, 0.01f);
            int maxExpansionPasses = Mathf.Max(
                1,
                Mathf.CeilToInt(maximumFieldRadius / step) + 2
            );

            // If nearby destinations are blocked, progressively widen the
            // search frontier rather than leaving the scout idle.
            for (int expansionPass = 0;
                 expansionPass < maxExpansionPasses;
                 expansionPass++)
            {
                float frontierRadius = Mathf.Min(
                    maximumFieldRadius,
                    maximumReachedRadius +
                    step * (FrontierLookAheadSteps + expansionPass * 1.5f)
                );

                float preferredRadius = Mathf.Clamp(
                    maximumReachedRadius +
                    Random.Range(-0.25f, 1.25f) * step +
                    expansionPass * 0.5f * step,
                    0f,
                    maximumFieldRadius
                );

                List<Candidate> candidates = BuildCandidates(
                    frontierRadius,
                    preferredRadius
                );

                candidates.Sort((a, b) => a.Score.CompareTo(b.Score));

                for (int i = 0; i < candidates.Count; i++)
                {
                    Candidate candidate = candidates[i];
                    Vector2 offset = candidate.Position - controller.Position;

                    if (offset.sqrMagnitude <=
                        MinimumTargetDistance * MinimumTargetDistance)
                    {
                        visitCounts[candidate.Cell.x, candidate.Cell.y]++;
                        continue;
                    }

                    if (!controller.MoveToPosition(candidate.Position))
                    {
                        failedAttempts[candidate.Cell.x, candidate.Cell.y]++;
                        continue;
                    }

                    currentTargetCell = candidate.Cell;
                    currentTargetPosition = candidate.Position;
                    lastMoveDirection = offset.normalized;
                    hasCurrentTarget = true;
                    return true;
                }
            }

            return false;
        }

        private List<Candidate> BuildCandidates(
            float frontierRadius,
            float preferredRadius)
        {
            List<Candidate> candidates = new List<Candidate>();
            Vector2 currentPosition = controller.Position;
            float step = Mathf.Max(explorationStep, 0.01f);
            float maximumRadiusSquared = frontierRadius * frontierRadius;

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector2 position = GetCellCenter(x, y);
                    Vector2 fromOrigin = position - explorationOrigin;
                    float radiusSquared = fromOrigin.sqrMagnitude;

                    if (radiusSquared > maximumRadiusSquared)
                        continue;

                    if ((position - currentPosition).sqrMagnitude <=
                        MinimumTargetDistance * MinimumTargetDistance)
                    {
                        continue;
                    }

                    float radius = Mathf.Sqrt(radiusSquared);
                    float travelDistance = Vector2.Distance(currentPosition, position);
                    float score = 0f;

                    // Prefer areas that have been visited less often.
                    score += visitCounts[x, y] * step * VisitPenalty;

                    // Keep retrying blocked destinations possible, but expensive.
                    score += failedAttempts[x, y] * step * FailedTargetPenalty;

                    // Prefer gradual outward progress with room for local wandering.
                    score += Mathf.Abs(radius - preferredRadius) * 1.2f;

                    // Avoid unnecessary long jumps between destinations.
                    score += travelDistance * DistanceWeight;

                    // Slightly prefer continuing forward, without forcing a straight line.
                    Vector2 direction = (position - currentPosition).normalized;
                    if (lastMoveDirection.sqrMagnitude > 0.001f)
                    {
                        float alignment = Vector2.Dot(lastMoveDirection, direction);
                        score += (1f - alignment) * step * DirectionWeight;
                    }

                    // Break ties and keep routes from looking identical each run.
                    score += Random.Range(0f, step * RandomScoreRange);

                    candidates.Add(new Candidate(
                        new Vector2Int(x, y),
                        position,
                        score
                    ));
                }
            }

            return candidates;
        }

        private Vector2 GetCellCenter(int x, int y)
        {
            return new Vector2(
                fieldMin.x + (x + 0.5f) * cellWidth,
                fieldMin.y + (y + 0.5f) * cellHeight
            );
        }

        private bool TryGetCell(Vector2 position, out Vector2Int cell)
        {
            cell = Vector2Int.zero;

            if (position.x < fieldMin.x || position.x >= fieldMax.x ||
                position.y < fieldMin.y || position.y >= fieldMax.y)
            {
                return false;
            }

            int x = Mathf.Clamp(
                Mathf.FloorToInt((position.x - fieldMin.x) / cellWidth),
                0,
                columns - 1
            );

            int y = Mathf.Clamp(
                Mathf.FloorToInt((position.y - fieldMin.y) / cellHeight),
                0,
                rows - 1
            );

            cell = new Vector2Int(x, y);
            return true;
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

        private void OnMovementEnded(MovementEndReason reason)
        {
            if (currentTask == null)
                return;

            if (hasCurrentTarget)
            {
                if (reason == MovementEndReason.Reached)
                {
                    visitCounts[currentTargetCell.x, currentTargetCell.y]++;
                    float reachedRadius = Vector2.Distance(
                        currentTargetPosition,
                        explorationOrigin
                    );
                    maximumReachedRadius = Mathf.Max(
                        maximumReachedRadius,
                        reachedRadius
                    );
                }
                else
                {
                    failedAttempts[currentTargetCell.x, currentTargetCell.y]++;
                }

                hasCurrentTarget = false;
            }

            if (TryMoveToNextTarget())
                return;

            UnsubscribeFromEvents();
            controller.CompleteCurrentTask();
        }
    }
}
