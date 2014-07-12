namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct tagPOINT
    {
        public int x;
        public int y;
    }
}

