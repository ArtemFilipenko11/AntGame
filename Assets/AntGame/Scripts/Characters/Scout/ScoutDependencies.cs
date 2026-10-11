using AntGame.Core.Navigation;
using UnityEngine;

namespace AntGame.Characters.Scout
{
    public sealed class ScoutDependencies
    {
        public INavigationSystem Navigation { get; }

        public Vector2 ExplorationCenter { get; }

        public Vector2 ExplorationSize { get; }

        public float ExplorationStep { get; }

        public Vector2 ExplorationOrigin { get; }

        public ScoutDependencies(
            INavigationSystem navigation,
            Vector2 explorationCenter,
            Vector2 explorationSize,
            float explorationStep,
            Vector2 explorationOrigin)
        {
            Navigation = navigation;
            ExplorationCenter = explorationCenter;
            ExplorationSize = explorationSize;
            ExplorationStep = explorationStep;
            ExplorationOrigin = explorationOrigin;
        }
    }
}