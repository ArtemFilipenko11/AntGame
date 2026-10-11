
using System;
using System.Collections.Generic;
using UnityEngine;
using AntGame.Characters.Ant;
using AntGame.Core.Navigation;
using AntGame.Systems;

namespace AntGame.Food
{
    public sealed class FoodDiscoverySystem : IGameplayService
    {
        private sealed class ReservationGroup
        {
            private readonly Dictionary<AntController, FoodReservation>
                reservations = new();

            public int ReservedAmount { get; private set; }

            public int Count => reservations.Count;

            public IReadOnlyCollection<FoodReservation> Items =>
                reservations.Values;

            public bool ContainsAnt(AntController ant)
            {
                return reservations.ContainsKey(ant);
            }

            public bool Contains(FoodReservation reservation)
            {
                return reservations.TryGetValue(
                           reservation.Ant,
                           out FoodReservation current)
                       && ReferenceEquals(current, reservation);
            }

            public void Add(FoodReservation reservation)
            {
                reservations.Add(reservation.Ant, reservation);
                ReservedAmount += reservation.Amount;
            }

            public bool Remove(FoodReservation reservation)
            {
                if (!Contains(reservation))
                    return false;

                reservations.Remove(reservation.Ant);
                ReservedAmount -= reservation.Amount;

                return true;
            }
        }

        private readonly HashSet<FoodSource> discoveredSources = new();

        private readonly Dictionary<FoodSource, ReservationGroup>
            reservationsBySource = new();

        private readonly Dictionary<AntController, FoodReservation>
            reservationsByAnt = new();

        private bool initialized;

        public bool IsInitialized => initialized;

        public IReadOnlyCollection<FoodSource> DiscoveredSources =>
            discoveredSources;

        public event Action<FoodSource> FoodDiscovered;
        public event Action<FoodSource> FoodForgotten;
        public event Action<FoodSource> FoodStockChanged;
        public event Action<FoodReservation> ReservationInvalidated;

        public FoodDiscoverySystem()
        {
        }

        public bool Initialize()
        {
            if (initialized)
                return true;

            initialized = true;
            return true;
        }

        public void Shutdown()
        {
            if (!initialized)
                return;

            List<FoodReservation> activeReservations =
                new(reservationsByAnt.Values);

            foreach (FoodReservation reservation in activeReservations)
                RemoveReservationTransaction(reservation);

            discoveredSources.Clear();
            initialized = false;
        }

        public bool IsDiscovered(FoodSource source)
        {
            return initialized
                && source != null
                && discoveredSources.Contains(source);
        }

        public bool TryDiscover(FoodSource source)
        {
            if (!initialized ||
                source == null ||
                !source.IsInitialized ||
                !source.HasFood)
            {
                return false;
            }

            if (!discoveredSources.Add(source))
                return false;
                
            source.MarkDiscovered();
            FoodDiscovered?.Invoke(source);
            return true;
        }

        public int GetReservedAmount(FoodSource source)
        {
            if (source == null ||
                !reservationsBySource.TryGetValue(
                    source,
                    out ReservationGroup group))
            {
                return 0;
            }

            return group.ReservedAmount;
        }

        public int GetAvailableAmount(FoodSource source)
        {
            if (!initialized ||
                source == null ||
                !source.HasFood ||
                !discoveredSources.Contains(source))
            {
                return 0;
            }

            return Mathf.Max(
                0,
                source.CurrentFood - GetReservedAmount(source)
            );
        }

        public FoodReservation GetReservation(AntController ant)
        {
            if (ReferenceEquals(ant, null))
                return null;

            return reservationsByAnt.TryGetValue(
                ant,
                out FoodReservation reservation)
                ? reservation
                : null;
        }

        public FoodReservation TryReserveBestFood(
            AntController ant,
            int requestedAmount)
        {
            if (!initialized ||
                ant == null ||
                !ant.IsInitialized ||
                requestedAmount <= 0 ||
                reservationsByAnt.ContainsKey(ant))
            {
                return null;
            }

            FoodSource bestSource = null;
            int bestAmount = 0;
            float bestPathCost = float.PositiveInfinity;
            float bestEfficiency = -1f;

            foreach (FoodSource source in discoveredSources)
            {
                if (source == null ||
                    !source.IsInitialized ||
                    !source.HasFood)
                {
                    continue;
                }

                int availableAmount = GetAvailableAmount(source);

                if (availableAmount <= 0)
                    continue;

                int reservableAmount = Mathf.Min(
                    requestedAmount,
                    availableAmount
                );

                float pathCost = ant.GetPathCostToFood(source);

                if (float.IsInfinity(pathCost) || float.IsNaN(pathCost))
                    continue;

                float efficiency = reservableAmount /
                                   Mathf.Max(pathCost, 0.001f);

                bool candidateIsFull =
                    reservableAmount >= requestedAmount;

                bool bestIsFull =
                    bestAmount >= requestedAmount;

                bool shouldReplace = bestSource == null;

                if (!shouldReplace)
                {
                    if (!Mathf.Approximately(efficiency, bestEfficiency))
                    {
                        shouldReplace = efficiency > bestEfficiency;
                    }
                    else if (candidateIsFull != bestIsFull)
                    {
                        shouldReplace = candidateIsFull;
                    }
                    else
                    {
                        shouldReplace = pathCost < bestPathCost;
                    }
                }

                if (!shouldReplace)
                    continue;

                bestSource = source;
                bestAmount = reservableAmount;
                bestPathCost = pathCost;
                bestEfficiency = efficiency;
            }

            if (bestSource == null)
                return null;

            return CreateReservation(
                ant,
                bestSource,
                bestAmount
            );
        }

        public bool ReleaseReservation(AntController ant)
        {
            if (ReferenceEquals(ant, null))
                return false;

            if (!reservationsByAnt.TryGetValue(
                    ant,
                    out FoodReservation reservation))
            {
                return false;
            }

            return RemoveReservationTransaction(reservation);
        }

        private FoodReservation CreateReservation(
            AntController ant,
            FoodSource source,
            int amount)
        {
            if (reservationsByAnt.ContainsKey(ant))
                return null;

            if (amount <= 0 ||
                source == null ||
                GetAvailableAmount(source) < amount)
            {
                return null;
            }

            FoodReservation reservation = new(
                ant,
                source,
                amount
            );

            bool hasGroup = reservationsBySource.TryGetValue(
                source,
                out ReservationGroup group
            );

            if (!hasGroup)
                group = new ReservationGroup();

            if (group.ContainsAnt(ant))
                return null;

            group.Add(reservation);

            if (!hasGroup)
                reservationsBySource.Add(source, group);

            reservationsByAnt.Add(ant, reservation);

            return reservation;
        }

        private bool RemoveReservationTransaction(
            FoodReservation reservation)
        {
            if (ReferenceEquals(reservation, null))
                return false;

            if (!reservationsByAnt.TryGetValue(
                    reservation.Ant,
                    out FoodReservation currentReservation)
                || !ReferenceEquals(currentReservation, reservation))
            {
                return false;
            }

            if (!reservationsBySource.TryGetValue(
                    reservation.Source,
                    out ReservationGroup group)
                || !group.Contains(reservation))
            {
                Debug.LogError(
                    "FoodDiscoverySystem reservation indexes are inconsistent."
                );

                return false;
            }


            group.Remove(reservation);
            reservationsByAnt.Remove(reservation.Ant);

            if (group.Count == 0)
                reservationsBySource.Remove(reservation.Source);

            return true;
        }

        public void HandleFoodAmountChanged(
            FoodSource source,
            int currentFood,
            int maxFood)
        {

            if (!initialized || source == null)
                return;

            if (discoveredSources.Contains(source))
                FoodStockChanged?.Invoke(source);
        }

        public void HandleFoodUnregistered(FoodSource source)
        {
            if (!initialized || ReferenceEquals(source, null))
                return;

            bool wasDiscovered = discoveredSources.Remove(source);

            if (reservationsBySource.TryGetValue(
                source,
                out ReservationGroup group))
            {
                List<FoodReservation> reservations =
                    new(group.Items);

                foreach (FoodReservation reservation in reservations)
                {
                    if (RemoveReservationTransaction(reservation))
                        ReservationInvalidated?.Invoke(reservation);
                }
            }

            if (wasDiscovered)
                FoodForgotten?.Invoke(source);
        }

        public void HandleAntDestroyed(AntController ant)
        {
            if (ant == null)
                return;

            ReleaseReservation(ant);
        }
    }
}
