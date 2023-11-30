// Taken from https://github.com/DanHarltey/Fastenshtein/blob/master/src/Fastenshtein/Levenshtein.cs
// License: MIT https://github.com/DanHarltey/Fastenshtein/blob/master/LICENSE

// This is a modified version of the public version that adds an overload
// that takes an offset and length to calcilate the distance for.

namespace Tql.App.Search
{
    /// <summary>
    /// Measures the difference between two strings.
    /// Uses the Levenshtein string difference algorithm.
    /// </summary>
    public partial class Levenshtein
    {
        /*
         * WARRING this class is performance critical (Speed).
         */

        private readonly string storedValue;
        private readonly int[] costs;

        /// <summary>
        /// Creates a new instance with a value to test other values against
        /// </summary>
        /// <param Name="value">Value to compare other values to.</param>
        public Levenshtein(string value)
        {
            this.storedValue = value;
            // Create matrix row
            this.costs = new int[this.storedValue.Length];
        }

        /// <summary>
        /// gets the length of the stored value that is tested against
        /// </summary>
        public int StoredLength => this.storedValue.Length;

        /// <summary>
        /// Compares a value to the stored value.
        /// Not thread safe.
        /// </summary>
        /// <returns>Difference. 0 complete match.</returns>
        public int DistanceFrom(string value) => DistanceFrom(value, 0, value.Length);

        /// <summary>
        /// Compares a value to the stored value.
        /// Not thread safe.
        /// </summary>
        /// <returns>Difference. 0 complete match.</returns>
        public int DistanceFrom(string value, int offset, int length)
        {
            if (costs.Length == 0)
            {
                return length;
            }

            // Add indexing for insertion to first row
            for (int i = 0; i < this.costs.Length; )
            {
                this.costs[i] = ++i;
            }

            for (int i = 0; i < length; i++)
            {
                // cost of the first index
                int cost = i;
                int previousCost = i;

                // cache value for inner loop to avoid index lookup and bonds checking, profiled this is quicker
                char value1Char = value[offset + i];

                for (int j = 0; j < this.storedValue.Length; j++)
                {
                    int currentCost = cost;

                    // assigning this here reduces the array reads we do, improvement of the old version
                    cost = costs[j];

                    if (value1Char != this.storedValue[j])
                    {
                        if (previousCost < currentCost)
                        {
                            currentCost = previousCost;
                        }

                        if (cost < currentCost)
                        {
                            currentCost = cost;
                        }

                        ++currentCost;
                    }

                    /*
                     * Improvement on the older versions.
                     * Swapping the variables here results in a performance improvement for modern intel CPU’s, but I have no idea why?
                     */
                    costs[j] = currentCost;
                    previousCost = currentCost;
                }
            }

            return this.costs[this.costs.Length - 1];
        }
    }
}
