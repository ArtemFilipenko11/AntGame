using AntGame.Characters.Ant;

namespace AntGame.Food
{
    public sealed class FoodReservation
    {
        public AntController Ant { get; }
        public FoodSource Source { get; }
        public int Amount { get; }

        internal FoodReservation(
            AntController ant,
            FoodSource source,
            int amount)
        {
            Ant = ant;
            Source = source;
            Amount = amount;
        }
    }
}