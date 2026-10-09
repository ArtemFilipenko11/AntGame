using System;
using UnityEngine;

namespace AntGame.Core.Navigation
{
    public sealed class GridNavigationSystem : INavigationSystem
    {
        private NavigationGrid grid;
        private AStarPathfinder pathfinder;

        public event Action NavigationChanged;

        public void Initialize(
            Vector2 center,
            Vector2 size,
            float cellSize,
            LayerMask obstacleLayers,
            bool detectObstacles)
        {
            if (grid != null)
                grid.GridChanged -= OnGridChanged;

            grid = new NavigationGrid(
                center,
                size,
                cellSize,
                obstacleLayers,
                detectObstacles);

            pathfinder = new AStarPathfinder();
            grid.GridChanged += OnGridChanged;
        }

        public Path FindPath(Vector2 start, Vector2 destination)
        {
            if (grid == null || pathfinder == null)
                return null;

            return pathfinder.FindPath(grid, start, destination);
        }

        public void Refresh()
        {
            grid?.Refresh();
        }

        private void OnGridChanged()
        {
            NavigationChanged?.Invoke();
        }
    }
}