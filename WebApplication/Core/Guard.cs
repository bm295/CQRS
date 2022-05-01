using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WebApplication.Core
{
    public class Guard
    {
        private Guard()
        { }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NotNull(object arg, string argName)
        {
            if (arg == null)
                throw new ArgumentNullException(argName);
        }

    }
}
