using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussFilter.Core.GaussMask
{
    public class StandardGaussMaskProvider : IGaussMaskProvider
    {
        public double [] GetMask(int maskSize, double gaussRadius)
        {
            List<double> mask = new List<double>();
            int positionDiff = ((maskSize - 1) / 2);
           
            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    mask.Add(CalculateWeight(j - positionDiff, i - positionDiff, gaussRadius));
                }
            }
            var sum = mask.Sum();
            mask = mask.Select(m => m /= sum).ToList();
            return mask.ToArray();
        }

        private double CalculateWeight(int x, int y, double gaussRadius)
        {
            double power = -(x * x + y * y) / (2 * Math.Pow(gaussRadius, 2));
            double weight = 1 / (2 * Math.PI * Math.Pow(gaussRadius, 2)) * Math.Pow(Math.E, power);
            return weight;
        }
    }
}
