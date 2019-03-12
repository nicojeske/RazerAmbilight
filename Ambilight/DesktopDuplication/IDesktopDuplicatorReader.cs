using System.Threading;

namespace Ambilight.DesktopDuplication
{
    public interface IDesktopDuplicatorReader
    {
        bool IsRunning { get; }

        void Run(CancellationToken token);
    }
}