using System.Runtime.InteropServices;

namespace FizzBuzzWithOutput
{
    /// <summary>The one Windows call the program needs: the standard output handle, to write to it as a file.</summary>
    internal static partial class NativeMethods
    {
        internal const int StdOutputHandle = -11;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        internal static partial nint GetStdHandle(int nStdHandle);
    }
}
