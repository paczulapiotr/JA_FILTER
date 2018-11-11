using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GaussFilter.Algorithm
{
    public class GaussFilter
    {
        private const double PI = 3.1415;
        private const double e = 2.7182;
        private readonly double maskValuesSum;

        public GaussFilter(int maskSize, double gaussRadius, Bitmap image)
        {
            MaskSize = maskSize;
            GaussRadius = gaussRadius;
            Image = image;
            FilteredImage = CreateNewBitmap();
            CreateMask();
            maskValuesSum = Mask.Sum(m => m);
        }

        public int MaskSize { get; }
        public double GaussRadius { get; }
        public IList<double> Mask { get; private set; }
        public Bitmap Image { get; private set; }
        public Bitmap FilteredImage { get; private set; }


        private Bitmap CreateNewBitmap()
        {
            var newWidth = Image.Width;// - (MaskSize - 1);
            var newHeigth = Image.Height;// - (MaskSize - 1);
            return new Bitmap(newWidth, newHeigth);
        }

        private void CreateMask()
        {
            int positionDiff = ((MaskSize - 1) / 2);
            Mask = new List<double>();
            for (int i = 0; i < MaskSize; i++)
            {
                for (int j = 0; j < MaskSize; j++)
                {
                    Mask.Add(CalculateWeight(j - positionDiff, i - positionDiff));
                }
            }
        }

        private double CalculateWeight(int x, int y)
        {
            double power = -(x * x + y * y) / (2 * Math.Pow(GaussRadius, 2));
            double weight = 1 / (2 * PI * Math.Pow(GaussRadius, 2)) * Math.Pow(e, power);
            return weight;
        }
        public void Save(string path)
        {
            if (FilteredImage != null)
            {
                FilteredImage.Save(path);
            }
        }
        public void Apply()
        {
            int boundSize = (MaskSize - 1) / 2;
            int lastRowIndex = FilteredImage.Height - 1;
            int lastColumnIndex = FilteredImage.Width - 1;
            for (int i = 0; i < FilteredImage.Width; i++)
            {
                for (int k = 0; k < boundSize; k++)
                {
                    FilteredImage.SetPixel(i, k, Image.GetPixel(i, k));
                    FilteredImage.SetPixel(i, lastRowIndex - k, Image.GetPixel(i, lastRowIndex - k));
                }
            }

            for (int i = boundSize; i < FilteredImage.Height - boundSize; i++)
            {
                for (int k = 0; k < boundSize; k++)
                {
                    FilteredImage.SetPixel(k, i, Image.GetPixel(k, i));
                    FilteredImage.SetPixel(lastColumnIndex - k, i, Image.GetPixel(lastColumnIndex - k, i));
                }

                for (int j = boundSize; j < FilteredImage.Width - boundSize; j++)
                {
                    Color color = CalculateNewPixel(j, i);
                    FilteredImage.SetPixel(j, i, color);
                }

            }

        }

        private Color CalculateNewPixel(int x, int y)
        {
            int positionDiff = ((MaskSize - 1) / 2);

            int startingX = x - positionDiff;
            int startingY = y - positionDiff;

            double R = 0d;
            double G = 0d;
            double B = 0d;
            double A = 0d;

            int maskCounter = 0;
            for (int i = 0; i < MaskSize; i++)
            {
                for (int j = 0; j < MaskSize; j++)
                {
                    var oldPixel = Image.GetPixel(startingX + j, startingY + i);
                    var weight = Mask [maskCounter];
                    R += oldPixel.R * weight;
                    G += oldPixel.G * weight;
                    B += oldPixel.B * weight;
                    A += oldPixel.A * weight;
                    maskCounter++;
                }
            }
            R /= maskValuesSum;
            G /= maskValuesSum;
            B /= maskValuesSum;
            A /= maskValuesSum;

            return Color.FromArgb((int)A, (int)R, (int)G, (int)B);
        }









    }
}
