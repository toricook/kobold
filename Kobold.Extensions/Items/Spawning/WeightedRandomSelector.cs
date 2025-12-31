using System;
using System.Collections.Generic;
using System.Linq;

namespace Kobold.Extensions.Items.Spawning
{
    /// <summary>
    /// Generic weighted random selection algorithm.
    /// Uses cumulative weight approach for O(n) selection.
    /// </summary>
    public static class WeightedRandomSelector
    {
        /// <summary>
        /// Select one item from a weighted collection using cumulative weight algorithm
        /// </summary>
        /// <typeparam name="T">Type of items to select from</typeparam>
        /// <param name="items">Collection of items to choose from</param>
        /// <param name="weightSelector">Function that returns the weight for each item</param>
        /// <param name="random">Random number generator (uses Random.Shared if null)</param>
        /// <returns>Randomly selected item based on weights</returns>
        /// <exception cref="ArgumentException">If collection is empty or total weight is zero</exception>
        public static T SelectWeighted<T>(
            IEnumerable<T> items,
            Func<T, int> weightSelector,
            Random random = null)
        {
            random ??= Random.Shared;

            var itemList = items.ToList();
            if (itemList.Count == 0)
                throw new ArgumentException("Cannot select from empty collection");

            int totalWeight = itemList.Sum(weightSelector);
            if (totalWeight <= 0)
                throw new ArgumentException("Total weight must be greater than zero");

            // Generate random value in range [0, totalWeight)
            int randomValue = random.Next(totalWeight);
            int cumulativeWeight = 0;

            // Iterate through items, accumulating weights
            foreach (var item in itemList)
            {
                cumulativeWeight += weightSelector(item);
                if (randomValue < cumulativeWeight)
                    return item;
            }

            // Fallback (should never reach here due to randomValue < totalWeight)
            return itemList[^1];
        }

        /// <summary>
        /// Select multiple items with weighted probabilities
        /// </summary>
        /// <typeparam name="T">Type of items to select from</typeparam>
        /// <param name="items">Collection of items to choose from</param>
        /// <param name="weightSelector">Function that returns the weight for each item</param>
        /// <param name="count">Number of items to select</param>
        /// <param name="allowDuplicates">If true, same item can be selected multiple times</param>
        /// <param name="random">Random number generator (uses Random.Shared if null)</param>
        /// <returns>List of randomly selected items</returns>
        public static List<T> SelectWeightedMultiple<T>(
            IEnumerable<T> items,
            Func<T, int> weightSelector,
            int count,
            bool allowDuplicates = false,
            Random random = null)
        {
            random ??= Random.Shared;
            var results = new List<T>();
            var availableItems = items.ToList();

            for (int i = 0; i < count && availableItems.Count > 0; i++)
            {
                var selected = SelectWeighted(availableItems, weightSelector, random);
                results.Add(selected);

                if (!allowDuplicates)
                {
                    availableItems.Remove(selected);
                }
            }

            return results;
        }
    }
}
