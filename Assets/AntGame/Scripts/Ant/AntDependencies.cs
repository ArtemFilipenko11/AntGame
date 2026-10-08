using AntGame.Food;
using AntGame.Nest;

namespace AntGame.Ant
{
    public class AntDependencies
    {
        public AntNest Nest { get; }
        public FoodRegistry FoodRegistry { get; }

        public AntDependencies(
            AntNest nest,
            FoodRegistry foodRegistry)
        {
            Nest = nest;
            FoodRegistry = foodRegistry;
        }
    }
}