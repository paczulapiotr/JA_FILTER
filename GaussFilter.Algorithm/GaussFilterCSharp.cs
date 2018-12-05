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
    public class GaussFilterCSharp
    {
        private const int BYTES_IN_PIXEL = 4;
        private readonly double maskValuesSum;
        private readonly Action<float> _dispatcher;

        public GaussFilterCSharp(int maskSize, double gaussRadius, Bitmap image, IGaussMaskProvider maskProvider, Action<float> dispatcher)
        {
            _bitmapLastIndex = image.Width * image.Height * BYTES_IN_PIXEL;
            MaskSize = maskSize;
            GaussRadius = gaussRadius;
            Image = image;
            _dispatcher = dispatcher;
            Image = image;
            FilteredImage = image.Clone() as Bitmap;
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

        public unsafe void ApplyUnsafe()
        {
            Bitmap bitmap = (Bitmap)Image.Clone();
            int dataArraySize = BYTES_IN_PIXEL * bitmap.Width * bitmap.Height;
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
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
                //Top bound R G B A
                filtered [i * BYTES_IN_PIXEL] = original [i * BYTES_IN_PIXEL];
                filtered [i * BYTES_IN_PIXEL + 1] = original [i * BYTES_IN_PIXEL + 1];
                filtered [i * BYTES_IN_PIXEL + 2] = original [i * BYTES_IN_PIXEL + 2];
                filtered [i * BYTES_IN_PIXEL + 3] = original [i * BYTES_IN_PIXEL + 3];
                //Bot bound R G B A
                filtered [bottomBoundStartIndex + i * BYTES_IN_PIXEL] = original [bottomBoundStartIndex + i * BYTES_IN_PIXEL];
                filtered [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 1] = original [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 1];
                filtered [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 2] = original [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 2];
                filtered [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 3] = original [bottomBoundStartIndex + i * BYTES_IN_PIXEL + 3];
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
                    //A
                    filtered [index + i * BYTES_IN_PIXEL + 3] = original [index + i * BYTES_IN_PIXEL + 3];
                }
                index += boundPixelWidth * BYTES_IN_PIXEL;

                //Apply gauss filter
                for (int i = 0; i < realWidth; i+=BYTES_IN_PIXEL)
                {
                    ApplyFilterOnPixelUnsafe(original, filtered, Image.Width, index + i);

                }
                index += realWidth ;

                //Set end of the row
                for (int i = 0; i < boundPixelWidth; i++)
                {
                    //R
                    filtered [index + i * BYTES_IN_PIXEL] = original [index + i * BYTES_IN_PIXEL];
                    //G
                    filtered [index + i * BYTES_IN_PIXEL + 1] = original [index + i * BYTES_IN_PIXEL + 1];
                    //B
                    filtered [index + i * BYTES_IN_PIXEL + 2] = original [index + i * BYTES_IN_PIXEL + 2];
                    //B
                    filtered [index + i * BYTES_IN_PIXEL + 3] = original [index + i * BYTES_IN_PIXEL + 3];
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
            double R = 0d;
            double G = 0d;
            double B = 0d;
            double A = 0d;
            int maskCounter = 0;
            int indexR, indexG, indexB, indexA;
            for (int y = -positionDiff; y <= positionDiff; y++)
            {
                for (int x = -positionDiff; x <= positionDiff; x++)
                {
                    indexR = index + (x + y * arrayWidth) * BYTES_IN_PIXEL;
                    indexG = index + (x + y * arrayWidth) * BYTES_IN_PIXEL + 1;
                    indexB = index + (x + y * arrayWidth) * BYTES_IN_PIXEL + 2;
                    indexA = index + (x + y * arrayWidth) * BYTES_IN_PIXEL + 3;
                    //R
                    R += original [indexR] * Mask [maskCounter];
                    //G
                    G += original [indexG] * Mask [maskCounter];
                    //B
                    B += original [indexB] * Mask [maskCounter];
                    //A
                    A += original [indexB] * Mask [maskCounter];
                    maskCounter++;
                }
            }
            //Set R
            filtered [index] = (byte)(R);// / maskValuesSum);
            //Set G
            filtered [index + 1] = (byte)(G);// / maskValuesSum);
            //Set B
            filtered [index + 2] = (byte)(B);// / maskValuesSum);
            //Set A
            filtered [index + 3] = (byte)(A);// / maskValuesSum);
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
