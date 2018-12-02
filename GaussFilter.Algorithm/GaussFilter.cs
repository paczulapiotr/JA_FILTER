using GaussFilter.Core.GaussMask;
using GaussFilter.Core.ProgressNotifier;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GaussFilter.Algorithm
{
    public class GaussFilter
    {
        private const int BYTES_IN_PIXEL = 3;
        private readonly double maskValuesSum;
        private readonly Action<float> _dispatcher;

        public GaussFilter(int maskSize, double gaussRadius, Bitmap image, IGaussMaskProvider maskProvider, Action<float> dispatcher)
        {
            _bitmapLastIndex = image.Width * image.Height * BYTES_IN_PIXEL;
            MaskSize = maskSize;
            GaussRadius = gaussRadius;
            Image = image;
            _dispatcher = dispatcher;
            FilteredImage = CreateNewBitmap();
            Mask = maskProvider.GetMask(maskSize, gaussRadius);
            maskValuesSum = Mask.Sum(m => m);
            ProgressChanged += GaussFilter_ProgressChanged;
        }

        private void GaussFilter_ProgressChanged(object sender, ProgressNotifierEventArgs e)
        {
            _dispatcher.Invoke(e.Percentage);
        }
        private int _bitmapLastIndex;        

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
            double weight = 1 / (2 * Math.PI * Math.Pow(GaussRadius, 2)) * Math.Pow(Math.E, power);
            return weight;
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

        public unsafe void ApplyUnsafe()
        {
            Bitmap bitmap = (Bitmap)Image.Clone();
            int dataArraySize = 3 * bitmap.Width * bitmap.Height;
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* original = (byte*)data.Scan0;

            byte [] filtered = new byte [dataArraySize];
            fixed (byte* filteredPtr = filtered)
            {

                Filter(original, filteredPtr);
                Marshal.Copy(filtered, 0, (IntPtr)original, dataArraySize);
                bitmap.UnlockBits(data);
            }

            FilteredImage = bitmap;
        }

        private unsafe void Filter(byte* original, byte* filtered)
        {
            int boundPixelWidth = (MaskSize - 1) / 2;
            int arraySize = Image.Width * Image.Height;
            int boundTopBottomArraySize = Image.Width * boundPixelWidth;
            int bottomBoundStartIndex = (arraySize - boundTopBottomArraySize) * BYTES_IN_PIXEL;

            //  Set top and bottom bound
            for (int i = 0; i < boundTopBottomArraySize; i++)
            {
                //Top bound R G B
                filtered [i * BYTES_IN_PIXEL] = original [i * BYTES_IN_PIXEL];
                filtered [i * BYTES_IN_PIXEL + 1] = original [i * BYTES_IN_PIXEL + 1];
                filtered [i * BYTES_IN_PIXEL + 2] = original [i * BYTES_IN_PIXEL + 2];
                //Bot bound R G B
                filtered [bottomBoundStartIndex + i * BYTES_IN_PIXEL] = original [bottomBoundStartIndex + i * BYTES_IN_PIXEL];
                filtered [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 1] = original [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 1];
                filtered [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 2] = original [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 2];
            }

            int index = boundTopBottomArraySize * BYTES_IN_PIXEL;
            int realWidth = (Image.Width - 2 * boundPixelWidth) * BYTES_IN_PIXEL;
            while (index < bottomBoundStartIndex)
            {

                //Set start of the row
                for (int i = 0; i < boundPixelWidth; i++)
                {
                    //R
                    filtered [index + i * BYTES_IN_PIXEL] = original [index + i * BYTES_IN_PIXEL];
                    //G
                    filtered [index + i * BYTES_IN_PIXEL + 1] = original [index + i * BYTES_IN_PIXEL + 1];
                    //B
                    filtered [index + i * BYTES_IN_PIXEL + 2] = original [index + i * BYTES_IN_PIXEL + 2];
                }
                index += boundPixelWidth * BYTES_IN_PIXEL;

                //Apply gauss filter
                for (int i = 0; i < realWidth; i++)
                {
                    ApplyFilterOnPixelUnsafe(original, filtered, Image.Width, index + i);

                }
                index += realWidth;

                //Set end of the row
                for (int i = 0; i < boundPixelWidth; i++)
                {
                    //R
                    filtered [index + i * BYTES_IN_PIXEL] = original [index + i * BYTES_IN_PIXEL];
                    //G
                    filtered [index + i * BYTES_IN_PIXEL + 1] = original [index + i * BYTES_IN_PIXEL + 1];
                    //B
                    filtered [index + i * BYTES_IN_PIXEL + 2] = original [index + i * BYTES_IN_PIXEL + 2];
                }
                //Should it be here???
                index += boundPixelWidth * BYTES_IN_PIXEL;

                OnProgressChanged((float)index / (float)_bitmapLastIndex);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original">Byte pointer to original bitmap</param>
        /// <param name="filtered">Byte pointer to filtered bitmap</param>
        /// <param name="arrayWidth">Bitmap pixels on X axis, width</param>
        /// <param name="index"></param>
        private unsafe void ApplyFilterOnPixelUnsafe(byte* original, byte* filtered, int arrayWidth, int index)
        {

            int positionDiff = ((MaskSize - 1) / 2);
            // Multiply by 3 cause params arrayWidth is the width of image in pixels(RGB as 1 pixel)
            int startingIndex = index - (positionDiff * (arrayWidth + 1) * BYTES_IN_PIXEL);

            double R = 0d;
            double G = 0d;
            double B = 0d;
            int maskCounter = 0;
            int indexR, indexG, indexB;
            int newRowParam = arrayWidth * BYTES_IN_PIXEL;
            for (int y = -positionDiff; y <= positionDiff; y++)
            {
                for (int x = -positionDiff; x <= positionDiff; x++)
                {
                    indexR = index + x * BYTES_IN_PIXEL + y * newRowParam;
                    indexG = index + x * BYTES_IN_PIXEL + y * newRowParam + 1;
                    indexB = index + x * BYTES_IN_PIXEL + y * newRowParam + 2;
                    //R
                    R += original [indexR] * Mask [maskCounter];
                    //G
                    G += original [indexG] * Mask [maskCounter];
                    //B
                    B += original [indexB] * Mask [maskCounter];
                    maskCounter++;
                }
            }
            //Set R
            filtered [index] = (byte)(R / maskValuesSum);
            //Set G
            filtered [index + 1] = (byte)(G / maskValuesSum);
            //Set B
            filtered [index + 2] = (byte)(B / maskValuesSum);
        }

        /// <summary>
        /// 
        /// </summary>
        private event EventHandler<ProgressNotifierEventArgs> ProgressChanged;

        public void OnProgressChanged(float newValue)
        {
            ProgressChanged?.Invoke(this, new ProgressNotifierEventArgs(newValue));
        }



    }
}
