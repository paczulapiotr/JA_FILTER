using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaussFilter.Core.ProgressNotifier
{
    public interface IProgressNotifier
    {
        void OnProgress(object sender, ProgressNotifierEventArgs eventArgs);
    }
}
