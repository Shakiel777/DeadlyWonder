using System;
using System.Linq;
using System.Collections.Generic;

namespace ThunderWire.Helper.Random
{
    public static class Randomizer
    {
        private static int number;

        /// <summary>
        /// Function to generate List of random integer numbers.
        /// </summary>
        public static List<int> RandomList(int min, int max, int count)
        {
            var randomNumbers = Enumerable.Range(min, max).OrderBy(x => Guid.NewGuid()).Take(count).ToList();
            return randomNumbers;
        }

        /// <summary>
        /// Function to generate random integer number (No Duplicates).
        /// </summary>
        public static int Range(int min, int max)
        {
            number = Enumerable.Range(min, max).OrderBy(x => Guid.NewGuid()).Where(x => x != number).Take(1).Single();
            return number;
        }
    }
}
