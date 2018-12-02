using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussFilter.Core.GaussMask
{
    public interface IGaussMaskProvider
    {
        double [] GetMask(int size, double gaussRadius);
    }
}
