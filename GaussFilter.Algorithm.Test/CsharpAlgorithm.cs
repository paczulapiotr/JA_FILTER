using GaussFilter.Core.GaussMask;
using Moq;
using NUnit.Framework;
using System.Drawing;

namespace GaussFilter.Algorithm.Test
{
    [TestFixture(Author ="Piotr Paczuła")]
    class CsharpAlgorithm
    {


        [TestCase(3, 5.5, new [] {  0.109886, 0.111718, 0.109886,
                                    0.111718, 0.113580, 0.111718,
                                    0.109886, 0.111718, 0.109886 }, TestName = "GaussMaskImplementation")]
        public void ShouldCreateProperGaussMask(int maskSize, double gaussRadius, double [] expectedArray)
        {
            // given
            var provider = new StandardGaussMaskProvider();
            double delta = 0.0001;

            // when
            double [] actualArray = provider.GetMask(maskSize, gaussRadius);

            // then
            for (int i = 0; i < expectedArray.Length; i++)
            {
                Assert.AreEqual(expectedArray [i], actualArray [i], delta);
            }
        }

    }
}
