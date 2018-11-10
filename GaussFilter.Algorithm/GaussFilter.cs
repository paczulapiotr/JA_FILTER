using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussFilter.Algorithm
{
    class GaussFilter
    {
        public GaussFilter(int maskSize, double gaussRadius, string imagePath)
        {
            MaskSize = maskSize;
            GaussRadius = gaussRadius;
            ImagePath = imagePath;
        }

        public int MaskSize { get; }
        public double GaussRadius { get; }
        public string ImagePath { get; }

        public  void Apply()
        {
            Bitmap image = new Bitmap(ImagePath, true);
            
        }
    }
}
