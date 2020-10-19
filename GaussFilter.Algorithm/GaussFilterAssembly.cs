using GaussFilter.Core.GaussMask;
using GaussFilter.Core.ProgressNotifier;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GaussFilter.Algorithm
{
    public class GaussFilterAssembly
    {
        private const int BYTES_IN_PIXEL = 4;
        private readonly int maskSize;
        private readonly double gaussRadius;
        private readonly Bitmap image;
        private readonly Bitmap filteredImage;
        private readonly IGaussMaskProvider maskProvider;
        private readonly Action<float> dispatcher;
        private readonly int bitmapLastIndex;
        private readonly double[] mask;
        public unsafe GaussFilterAssembly(int maskSize, double gaussRadius, Bitmap image, IGaussMaskProvider maskProvider, Action<float> dispatcher)
        {
            bitmapLastIndex = image.Width * image.Height * BYTES_IN_PIXEL;
            mask = new double[] {
                        -1, -1, -1,
                        -1,  8, -1,
                        -1, -1, -1 };//maskProvider.GetMask(maskSize, gaussRadius);
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

        [DllImport("GaussFilter.Algorithm.ASM.dll", EntryPoint = "laplace")]
        private static extern unsafe int ApplyLaplaceAsm(int startingSubpixelIndex, int subpixelsToFilter, byte* original, byte* filtered, int* mask, int subpixelImageWidth);

        [DllImport("GaussFilter.Algorithm.ASM.dll", EntryPoint = "laplace")]
        private static extern unsafe int AplyGaussAsm(int index, int arrayWidth, byte* original, byte* filtered, double* mask, int maskSize);

        [DllImport("GaussFilter.Algorithm.ASM.dll", EntryPoint = "border")]
        private static extern unsafe int SetTopBottomBorderAsm(byte* original, byte* filtered, int topBottomBorderSize, int bottomBoundStartIndex);
        public unsafe Bitmap ApplyAssemblyFilter(Bitmap _bitmap)
        {
            Bitmap bitmap = (Bitmap)_bitmap.Clone();
            int dataArraySize = BYTES_IN_PIXEL * bitmap.Width * bitmap.Height;
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* original = (byte*)data.Scan0;

            byte[] filtered = new byte[dataArraySize];
            fixed (byte* filteredPtr = filtered)
            {

                Filter(original, filteredPtr);
                Marshal.Copy(filtered, 0, (IntPtr)original, dataArraySize);
                bitmap.UnlockBits(data);
            }

            return bitmap;

        }


        private unsafe void Filter(byte* original, byte* filtered)
        {
            int boundPixelWidth = (maskSize - 1) / 2;
            int arraySize = image.Width * image.Height;
            int boundTopBottomArraySizeInPixels = image.Width * boundPixelWidth;
            int boundTopBottomArraySizeInBytes = boundTopBottomArraySizeInPixels * BYTES_IN_PIXEL;
            int bottomBoundStartIndex = (arraySize - boundTopBottomArraySizeInPixels) * BYTES_IN_PIXEL;
            int arrayWidth = image.Width;
            int byteIndex;

            fixed(int* mask = new int[] {
                        -1, -1, -1,
                        -1,  8, -1,
                        -1, -1, -1 })
            {
                var startingSubpixelIndex = image.Width * 4 + 4;
                var imageSubpixelWidth = image.Width * 4;
                var imageSubpixelWidthEditable = imageSubpixelWidth - 8;
                for (int y = 2; y < image.Height; y++)
                {
                    ApplyLaplaceAsm(startingSubpixelIndex, imageSubpixelWidthEditable, original, filtered, mask, imageSubpixelWidth);
                    startingSubpixelIndex += imageSubpixelWidth;
                }
            }


            ////  Set top and bottom bound
            //SetTopBottomBorderAsm(original, filtered, boundTopBottomArraySizeInBytes, bottomBoundStartIndex);

            //int index = boundTopBottomArraySizeInBytes;
            //int realWidth = (image.Width - 2 * boundPixelWidth) * BYTES_IN_PIXEL;
            //while (index < bottomBoundStartIndex)
            //{

            //    //Set start of the row
            //    for (int i = 0; i < boundPixelWidth; i++)
            //    {
            //        byteIndex = i * BYTES_IN_PIXEL;
            //        //R
            //        filtered[index + byteIndex] = original[index + byteIndex];
            //        //G
            //        filtered[index + byteIndex + 1] = original[index + byteIndex + 1];
            //        //B
            //        filtered[index + byteIndex + 2] = original[index + byteIndex + 2];
            //        //A
            //        filtered[index + byteIndex + 3] = original[index + byteIndex + 3];
            //    }
            //    index += boundPixelWidth * BYTES_IN_PIXEL;

            //    //Apply gauss filter

            //    fixed (double* maskPtr = mask)
            //    {
            //        AplyGaussAsm(index, arrayWidth, original, filtered, maskPtr, 3);
            //    }

            //    index += realWidth;

            //    //Set end of the row
            //    for (int i = 0; i < boundPixelWidth; i++)
            //    {
            //        byteIndex = i * BYTES_IN_PIXEL;
            //        //R
            //        filtered[index + byteIndex] = original[index + byteIndex];
            //        //G
            //        filtered[index + byteIndex + 1] = original[index + byteIndex + 1];
            //        //B
            //        filtered[index + byteIndex + 2] = original[index + byteIndex + 2];
            //        //A
            //        filtered[index + byteIndex + 3] = original[index + byteIndex + 3];
            //    }

            //    index += boundPixelWidth * BYTES_IN_PIXEL;

            //    OnProgressChanged(index / (float)bitmapLastIndex);
        }

        private event EventHandler<ProgressNotifierEventArgs> ProgressChanged;

        public void OnProgressChanged(float newValue)
        {
            ProgressChanged?.Invoke(this, new ProgressNotifierEventArgs(newValue));
        }


    }
}
