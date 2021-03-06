﻿using System.Collections.Generic;
using System.Linq;
using AlgoKit.Algorithms.Permutations;
using AlgoKit.Extensions;
using Xunit;

namespace AlgoKit.Test.Algorithms.Permutations
{
    public class FisherYatesShuffleTests
    {
        public static IEnumerable<object[]> GetTestCases()
            => Enumerable.Range(1, 5).Select(x => x.Yield<object>().ToArray());

        [Theory]
        [MemberData(nameof(GetTestCases))]
        public void PermutationsShouldBeUnbiased(int count)
        {
            // Arrange
            var data = Enumerable.Range(1, count).ToArray();
            var permutations = PermutationLexicographicOrdering.EnumeratePermutations(data)
                .ToDictionary(Stringify, x => 0);

            var random = new RandomWithSeed();
            var shuffle = new FisherYatesShuffle(random);

            var idealNumberOfOccurrences = 10000;
            var trials = permutations.Count * idealNumberOfOccurrences;

            // Act
            for (var i = 0; i < trials; ++i)
            {
                var tmp = Enumerable.Range(1, count).ToArray();
                shuffle.Shuffle(tmp);
                permutations[Stringify(tmp)]++;
            }

            // Assert
            var deviation = 0.04;
            var maxAllowed = (int)(idealNumberOfOccurrences * (1 + deviation));
            var minAllowed = (int)(idealNumberOfOccurrences * (1 - deviation));

            foreach (var pair in permutations)
            {
                var occurences = pair.Value;
                var message = $"Expected permutation to be yielded {minAllowed} <= x <= {maxAllowed} times, " +
                              $"but was {occurences}. Seed: {random.Seed}";

                Assert.True(occurences >= minAllowed && occurences <= maxAllowed, message);
            }
        }

        private static string Stringify(IEnumerable<int> sequence) => string.Join("", sequence);
    }
}