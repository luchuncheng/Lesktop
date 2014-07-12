namespace MsHtmHstInterop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _userHGLOBAL
    {
        public int fContext;
        public __MIDL_IWinTypes_0003 u;
    }
}

