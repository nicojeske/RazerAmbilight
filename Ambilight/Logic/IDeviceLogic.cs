using System.Drawing;

namespace Ambilight.Logic
{
    public interface IDeviceLogic
    {
        void Process(Bitmap newImage);
    }
}