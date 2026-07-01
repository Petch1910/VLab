using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public sealed class SeededRandomService
    {
        private readonly Random random;

        public int Seed { get; }

        public SeededRandomService(int seed)
        {
            Seed = seed;
            random = new Random(seed);
        }

        public int Next(int maxExclusive)
        {
            return random.Next(maxExclusive);
        }

        public int Next(int minInclusive, int maxExclusive)
        {
            return random.Next(minInclusive, maxExclusive);
        }

        public void Shuffle<T>(IList<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            for (int i = values.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = values[i];
                values[i] = values[j];
                values[j] = temp;
            }
        }
    }
}
