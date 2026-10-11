using AntGame.Core.Navigation;
using AntGame.Food;
using AntGame.Nest;

namespace AntGame.Characters.Ant
{
    public class AntDependencies
    {
        public AntNest Nest { get; }
        public FoodRegistry FoodRegistry { get; }
        public FoodDiscoverySystem FoodDiscovery { get; }
        public INavigationSystem Navigation { get; }

        public AntDependencies(
            AntNest nest,
            FoodRegistry foodRegistry,
            FoodDiscoverySystem foodDiscovery,
            INavigationSystem navigation)
        {
            Nest = nest;
            FoodRegistry = foodRegistry;
            FoodDiscovery = foodDiscovery;
            Navigation = navigation;
        }
    }
}