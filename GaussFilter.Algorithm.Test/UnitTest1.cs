using System;
using System.Drawing;
using NUnit.Framework;
using Moq;

namespace GaussFilter.Algorithm.Test
{
    [TestFixture]
    public class GaussFilterTest
    {
       
        [Test]
        public void ShouldCreateProperMask()
        {
            // given
            var imageMock = new Mock<Bitmap>();
            imageMock.Setup(image => image.Height).Returns(10);
            imageMock.Setup(image => image.Width).Returns(10);
            int maskSize = 3;
            double radius = 1;

            // when
            GaussFilter filter = new GaussFilter(maskSize, radius, imageMock.Object);

            // then

        }
    }
}
