
using System.Collections.Generic;
using UnityEngine;

namespace AntGame.Navigation
{
    public sealed class AStarPathfinder
    {
        private static readonly Vector2Int[] Directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        private const float StraightCost = 1f;
        private const float DiagonalCost = 1.41421356f;

        public bool TryFindPath(
            NavigationGrid grid,
            Vector2 start,
            Vector2 destination,
            out Vector2[] path)
        {
            path = null;

            if (grid == null)
                return false;

            if (!grid.TryGetCell(start, out Vector2Int startCell))
                return false;

            if (!grid.TryGetCell(destination, out Vector2Int goalCell))
                return false;

            if (!grid.IsWalkable(startCell)
                || !grid.IsWalkable(goalCell))
                return false;

            // Если начало и цель находятся в одной клетке,
            // достаточно направиться к самой цели.
            if (startCell == goalCell)
            {
                path = new[] { destination };
                return true;
            }

            int width = grid.Width;
            int height = grid.Height;

            float[,] gCost = new float[width, height];
            bool[,] closed = new bool[width, height];
            bool[,] hasParent = new bool[width, height];
            Vector2Int[,] parents =
                new Vector2Int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    gCost[x, y] = float.PositiveInfinity;
            }

            List<Vector2Int> open = new List<Vector2Int>();

            gCost[startCell.x, startCell.y] = 0f;
            open.Add(startCell);

            while (open.Count > 0)
            {
                int bestIndex = 0;
                float bestScore = float.PositiveInfinity;

                // Выбираем клетку с наименьшей оценкой:
                // стоимость пути до неё + оценка пути до цели.
                for (int i = 0; i < open.Count; i++)
                {
                    Vector2Int candidate = open[i];

                    float score =
                        gCost[candidate.x, candidate.y]
                        + Heuristic(candidate, goalCell);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestIndex = i;
                    }
                }

                Vector2Int current = open[bestIndex];
                open.RemoveAt(bestIndex);

                if (current == goalCell)
                {
                    path = BuildPath(
                        grid,
                        parents,
                        hasParent,
                        startCell,
                        goalCell,
                        destination);

                    return path != null;
                }

                closed[current.x, current.y] = true;

                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int neighbour = current + direction;

                    if (!grid.IsWalkable(neighbour))
                        continue;

                    if (closed[neighbour.x, neighbour.y])
                        continue;

                    bool diagonal =
                        direction.x != 0 && direction.y != 0;

                    // Не позволяем проходить по диагонали
                    // между двумя соприкасающимися препятствиями.
                    if (diagonal)
                    {
                        Vector2Int horizontal =
                            new Vector2Int(
                                current.x + direction.x,
                                current.y);

                        Vector2Int vertical =
                            new Vector2Int(
                                current.x,
                                current.y + direction.y);

                        if (!grid.IsWalkable(horizontal)
                            || !grid.IsWalkable(vertical))
                            continue;
                    }

                    float stepCost = diagonal
                        ? DiagonalCost
                        : StraightCost;

                    float tentativeCost =
                        gCost[current.x, current.y] + stepCost;

                    if (tentativeCost >=
                        gCost[neighbour.x, neighbour.y])
                        continue;

                    gCost[neighbour.x, neighbour.y] =
                        tentativeCost;

                    parents[neighbour.x, neighbour.y] = current;
                    hasParent[neighbour.x, neighbour.y] = true;

                    if (!open.Contains(neighbour))
                        open.Add(neighbour);
                }
            }

            // Цель недостижима.
            return false;
        }

        private static float Heuristic(
            Vector2Int from,
            Vector2Int to)
        {
            int dx = Mathf.Abs(from.x - to.x);
            int dy = Mathf.Abs(from.y - to.y);

            int diagonalSteps = Mathf.Min(dx, dy);
            int straightSteps = Mathf.Max(dx, dy) - diagonalSteps;

            return diagonalSteps * DiagonalCost
                + straightSteps * StraightCost;
        }

        private static Vector2[] BuildPath(
            NavigationGrid grid,
            Vector2Int[,] parents,
            bool[,] hasParent,
            Vector2Int start,
            Vector2Int goal,
            Vector2 destination)
        {
            List<Vector2Int> reversed =
                new List<Vector2Int>();

            Vector2Int current = goal;

            while (current != start)
            {
                reversed.Add(current);

                if (!hasParent[current.x, current.y])
                    return null;

                current = parents[current.x, current.y];
            }

            reversed.Reverse();

            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i < reversed.Count; i++)
            {
                Vector2Int cell = reversed[i];

                // Последняя точка ? точная позиция цели,
                // а не центр её клетки.
                if (cell == goal)
                    points.Add(destination);
                else
                    points.Add(grid.CellToWorld(cell));
            }

            return points.ToArray();
        }
    }
}