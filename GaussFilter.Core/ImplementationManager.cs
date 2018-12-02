using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace GaussFilter.Core
{
    public class ImplementationManager
    {


        [DllImport("GaussFilter.Algorithm.ASM.dll", EntryPoint = "gauss")]
        private static extern unsafe int AssemblyCode(byte* pixels, byte* original, int size);
        public unsafe Bitmap ApplyAssemblyFilter(Bitmap bitmap)
        {

            int dataArraySize = 3 * bitmap.Width * bitmap.Height;
            BitmapData data = bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadWrite,PixelFormat.Format24bppRgb);
            byte* pixels = (byte*)data.Scan0;
                
            fixed(byte* original = new byte [dataArraySize])
            {
                AssemblyCode(pixels, original, dataArraySize);
                data.Scan0 = new IntPtr(original);
                bitmap.UnlockBits(data);
            }

            return bitmap;

        }


    }
}
