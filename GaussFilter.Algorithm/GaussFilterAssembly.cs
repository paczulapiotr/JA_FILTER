using GaussFilter.Core.GaussMask;
using GaussFilter.Core.ProgressNotifier;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GaussFilter.Algorithm
{
    public class GaussFilterAssembly
    {
        private const int BYTES_IN_PIXEL = 3;
        private readonly int maskSize;
        private readonly double gaussRadius;
        private readonly Bitmap image;
        private readonly Bitmap filteredImage;
        private readonly IGaussMaskProvider maskProvider;
        private readonly Action<float> dispatcher;
        private readonly int bitmapLastIndex;
        private unsafe double* mask;
        private readonly double maskSum;
        public unsafe GaussFilterAssembly(int maskSize, double gaussRadius, Bitmap image, IGaussMaskProvider maskProvider, Action<float> dispatcher)
        {
            bitmapLastIndex = image.Width * image.Height * BYTES_IN_PIXEL;
            var maskTable = maskProvider.GetMask(maskSize, gaussRadius);
            fixed(double* maskTemp = maskTable)
                mask = maskTemp;
            maskSum = maskTable.Sum(m => m);
            ProgressChanged += GaussFilterAssembly_ProgressChanged;
            this.maskSize = maskSize;
            this.gaussRadius = gaussRadius;
            this.image = image;
            filteredImage = image.Clone() as Bitmap;
            this.maskProvider = maskProvider;
            this.dispatcher = dispatcher;
        }

        private void GaussFilterAssembly_ProgressChanged(object sender, ProgressNotifierEventArgs e)
        {
            dispatcher.Invoke(e.Percentage);
        }

        [DllImport("GaussFilter.Algorithm.ASM.dll", EntryPoint = "gauss")]
        private static extern unsafe int AssemblyCode(int index, int arrayWidth, byte* original, byte* filtered, double* mask, int maskSize,double maskSum);


        public unsafe Bitmap ApplyAssemblyFilter(Bitmap _bitmap)
        {
            Bitmap bitmap = (Bitmap)_bitmap.Clone();
            int dataArraySize = 3 * bitmap.Width * bitmap.Height;
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* original = (byte*)data.Scan0;

            byte [] filtered = new byte [dataArraySize];
            fixed (byte* filteredPtr = filtered)
            {

                Filter(original,filteredPtr);
                Marshal.Copy(filtered, 0, (IntPtr)original, dataArraySize);
                bitmap.UnlockBits(data);
            }

            return bitmap;

        }


        private unsafe void Filter(byte* original, byte* filtered)
        {
            int boundPixelWidth = (maskSize - 1) / 2;
            int arraySize = image.Width * image.Height;
            int boundTopBottomArraySize = image.Width * boundPixelWidth;
            int bottomBoundStartIndex = (arraySize - boundTopBottomArraySize) * BYTES_IN_PIXEL;
            int arrayWidth = image.Width;

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
            int realWidth = (image.Width - 2 * boundPixelWidth) * BYTES_IN_PIXEL;
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
                for (int i = 0; i < realWidth; i+=3)
                {
                    AssemblyCode(index + i, arrayWidth,original, filtered, mask, maskSize, maskSum);

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

                OnProgressChanged((float)index / (float)bitmapLastIndex);

            }
        }




        private event EventHandler<ProgressNotifierEventArgs> ProgressChanged;

        public void OnProgressChanged(float newValue)
        {
            ProgressChanged?.Invoke(this, new ProgressNotifierEventArgs(newValue));
        }


    }
}
