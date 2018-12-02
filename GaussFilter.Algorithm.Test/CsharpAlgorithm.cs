using GaussFilter.Core.GaussMask;
using Moq;
using NUnit.Framework;
using System.Drawing;

namespace GaussFilter.Algorithm.Test
{
    [TestFixture]
    class CsharpAlgorithm
    {

        [Test]
        public void ShouldFilterCorrectlyGiven()
        {
            // given
            double [] FIXED_MASK = new double []{ 1, 1, 2, 1, 1,
                                        1, 2, 4, 2, 1,
                                        2, 4, 8, 4, 2,
                                        1, 2, 4, 2, 1,
                                        1, 1, 2, 1, 1 };
            int maskSize = 5;
            double gaussRadius = 10.0;
            Mock<IGaussMaskProvider> gaussMaskMock = new Mock<IGaussMaskProvider>();
            gaussMaskMock.Setup(m => m.GetMask(maskSize, gaussRadius)).Returns(FIXED_MASK);
            //GaussFilter gaussFilter = new GaussFilter(maskSize, gaussRadius,new Bitmap("C:\\Users\\Piotr\\source\\repos\\GaussFilter\\GaussFilter.Algorithm.Test\\TestFiles\\test1.bmp"), gaussMaskMock.Object);
            // when

            // then
        }

        [TestCase(3, 5.5, new [] {  0.109886, 0.111718, 0.109886,
                                    0.111718, 0.113580, 0.111718,
                                    0.109886, 0.111718, 0.109886 }, TestName = "Basic")]
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
