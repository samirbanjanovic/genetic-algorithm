using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesPerson.GeneticAlgorithm
{
    public static class MutationMethods
    {

        public static void RandomMutate(IList<Node> genesToMutate, double geneticMutationPercentageRange)
        {
            int i = 0;
            var indecies = genesToMutate.Select(m => i++).ToList();

            int mutationSize = (int)(geneticMutationPercentageRange * genesToMutate.Count);

            for (int n = 0; n < mutationSize; n += 2)
            {
                int ri = GAInstance.RandomGenerator.Next(indecies.Count);
                var ich1 = indecies[ri];
                indecies.RemoveAt(ri);

                ri = GAInstance.RandomGenerator.Next(indecies.Count);
                var ich2 = indecies[ri];
                indecies.RemoveAt(ri);

                var tmp = genesToMutate[ich1];
                genesToMutate[ich1] = genesToMutate[ich2];
                genesToMutate[ich2] = tmp;
            }
        }

        private static void RandomSwap(IList<Node> locations, IList<int> locationIndecies)
        {
            int n1Index = 0;
            int n2Index = 0;
            int metaIndex = 0;

            while (locationIndecies.Count > 3)
            {                
                metaIndex = GAInstance.RandomGenerator.Next(0, locationIndecies.Count); // pick first swap destination
                n1Index = locationIndecies[metaIndex];
                locationIndecies.RemoveAt(metaIndex);
                
                metaIndex = GAInstance.RandomGenerator.Next(0, locationIndecies.Count); // pick second swap destination
                n2Index = locationIndecies[metaIndex];
                locationIndecies.RemoveAt(metaIndex);

                var tmp = locations[n1Index];
                locations[n1Index] = locations[n2Index];
                locations[n2Index] = tmp;
            }

            var tmpF = locations[locationIndecies[0]];
            locations[locationIndecies[0]] = locations[locationIndecies[1]];
            locations[locationIndecies[1]] = tmpF;
        }
    }
}
