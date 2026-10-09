using UnityEngine;

namespace AntGame.Core.Navigation
{
    public sealed class Path
    {
        private readonly Vector2[] points;

        public int Count => points.Length;

        public Vector2 this[int index] => points[index];

        public Path(Vector2[] points)
        {
            this.points = points ?? new Vector2[0];
        }
    }
}
