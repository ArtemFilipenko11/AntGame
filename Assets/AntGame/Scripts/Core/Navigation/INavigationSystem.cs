using System;

namespace AntGame.Core.Navigation
{
    public interface INavigationSystem
    {
        event Action NavigationChanged;

        Path FindPath(
            UnityEngine.Vector2 start,
            UnityEngine.Vector2 destination);
    }
}