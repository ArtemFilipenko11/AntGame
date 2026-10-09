using System;
using UnityEngine;

namespace AntGame.Core.Navigation
{
    public sealed class NavigationGrid
    {
        private readonly Vector2 origin;
        private readonly float cellSize;
        private readonly LayerMask obstacleLayers;
        private readonly bool detectObstacles;
        private readonly int width;
        private readonly int height;
        private readonly bool[,] walkable;

        public event Action GridChanged;

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;

        public NavigationGrid(
            Vector2 center,
            Vector2 size,
            float cellSize,
            LayerMask obstacleLayers,
            bool detectObstacles = false)
        {
            if (size.x <= 0f || size.y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (cellSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(cellSize));

            this.cellSize = cellSize;
            this.obstacleLayers = obstacleLayers;
            this.detectObstacles = detectObstacles;

            origin = center - size * 0.5f;
            width = Mathf.CeilToInt(size.x / cellSize);
            height = Mathf.CeilToInt(size.y / cellSize);

            walkable = new bool[width, height];

            Refresh();
        }

        public Vector2Int WorldToCell(Vector2 worldPosition)
        {
            return new Vector2Int(
                Mathf.FloorToInt((worldPosition.x - origin.x) / cellSize),
                Mathf.FloorToInt((worldPosition.y - origin.y) / cellSize));
        }

        public Vector2 CellToWorld(Vector2Int cell)
        {
            return origin + new Vector2(
                (cell.x + 0.5f) * cellSize,
                (cell.y + 0.5f) * cellSize);
        }

        public bool IsInside(Vector2Int cell)
        {
            return cell.x >= 0
                && cell.y >= 0
                && cell.x < width
                && cell.y < height;
        }

        public bool IsWalkable(Vector2Int cell)
        {
            return IsInside(cell) && walkable[cell.x, cell.y];
        }

        public void Refresh()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    walkable[x, y] =
                        !detectObstacles || !HasObstacle(cell);
                }
            }

            GridChanged?.Invoke();
        }

        private bool HasObstacle(Vector2Int cell)
        {
            return Physics2D.OverlapBox(
                CellToWorld(cell),
                Vector2.one * cellSize * 0.9f,
                0f,
                obstacleLayers) != null;
        }
    }
}