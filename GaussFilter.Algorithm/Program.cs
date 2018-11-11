using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussFilter.Algorithm
{
    class Program
    {
        public static void Main(string [] args)
        {
            Bitmap image = new Bitmap(@"C:\Users\Piotr\source\repos\GaussFilter\public\picture3.jpg");
            GaussFilter gaussFilter = new GaussFilter(5, 4, image);
            gaussFilter.Apply();
            gaussFilter.Save(@"C:\Users\Piotr\source\repos\GaussFilter\public\Filtered.bmp");
        }
    }
}
