using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambilight.DesktopDuplication
{
    [Serializable]
    public class DesktopDuplicationException : Exception
    {
        public DesktopDuplicationException(string message)
            : base(message) { }
        public DesktopDuplicationException(string message, Exception innerException)
                    : base(message, innerException) { }


    }
}
