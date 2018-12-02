using GaussFilter.Core.Algorithm;
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
    public class CSharpKernelApplier : IKernelApplier
    {
        public unsafe void ApplyKernel(byte* original, byte* filtered, int arrayWidth,double* mask, int maskSize, double maskSum, int index)
        {
            const int BYTES_IN_PIXEL = 3;

            int positionDiff = ((maskSize - 1) / 2);
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
                    R += original [indexR] * mask [maskCounter];
                    //G
                    G += original [indexG] * mask [maskCounter];
                    //B
                    B += original [indexB] * mask [maskCounter];
                    maskCounter++;
                }
            }
            //Set R
            filtered [index] = (byte)(R / maskSum);
            //Set G
            filtered [index + 1] = (byte)(G / maskSum);
            //Set B
            filtered [index + 2] = (byte)(B / maskSum);
        }
    }
}
