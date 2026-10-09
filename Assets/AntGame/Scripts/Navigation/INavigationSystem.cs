using System;
using UnityEngine;

namespace AntGame.Navigation
{
    public interface INavigationSystem
    {
        event Action NavigationChanged;

        bool TryFindPath(
            Vector2 start,
            Vector2 destination,
            out Vector2[] path);
    }
}